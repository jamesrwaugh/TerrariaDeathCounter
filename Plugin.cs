using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TerrariaApi;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;

namespace TerrariaDeathCounter
{
    [ApiVersion(2, 1)]
    public class TerrariaDeathCounterPlugin : TerrariaPlugin
    {
        public override string Name
        {
            get
            {
                return "Death Recorder";
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }
        
        public override string Author
        {
            get
            {
                return "Discoveri";
            }
        }

        public override string Description
        {
            get
            {
                return "Records and reports player deaths from each source, for laughs and stats.";
            }
        }

        private static string saveFilename = "DeathRecords.json";
        private static IDeathRepository deathRecords = new JsonDeathRepository(saveFilename);
        private static ILogWriter logWriter = new ServerLogWriter("DeathOutput.log");

        public TerrariaDeathCounterPlugin(Main game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }

        private void OnGetData(GetDataEventArgs e)
        {
            if (e.Handled)
                return;

            PacketTypes type = e.MsgID;
            if (type != PacketTypes.PlayerDeathV2)
            {
                return;
            }

            using (MemoryStream data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length - 1))
            {
                var player = Main.player[e.Msg.whoAmI];
                RecordPlayerDeath(player, data);
            }
        }

        private bool RecordPlayerDeath(Player player, MemoryStream data)
        {
            // Unused initial ID, read byte so FromReader is properly aligned in stream.
            data.ReadByte();
            PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(new BinaryReader(data));

            // Log the main PlayerDeathReason object for reference.
            logWriter.ServerWriteLine(JsonConvert.SerializeObject(playerDeathReason), TraceLevel.Info);

            // Record failure in current death repository.
            string killerName = GetNameOfKiller(player, playerDeathReason);
            int totalDeathCount = deathRecords.RecordDeath(player.name, killerName);

            // Broadcast message to server.
            string serverMessage = GetServerMessage(player, killerName, totalDeathCount);
            TShockAPI.Utils.Instance.Broadcast(serverMessage, 255, 0, 0);

            return true;
        }

        private string GetServerMessage(Player player, string killerName, int totalDeathCount)
        {
            StringBuilder message = new StringBuilder();

            if(totalDeathCount == 1)
            {
                message.Append(string.Format("This is the first time {0} has died to a {1}.", player.name, killerName));
            }
            else if(totalDeathCount == 2)
            {
                message.Append(string.Format("This is now the second time {0} has died to a {1}.", player.name, killerName));
            }
            else
            {
                message.Append(string.Format("{0} has now died to a {1} {2} times.", player.name, killerName, totalDeathCount));
            }

            if(killerName == "a deadly fall")
            {
                message.Append(" Don't do that.");
            }

            return message.ToString();
        }

        private string GetNameOfKiller(Player player, PlayerDeathReason reason)
        {
            if(IsFallDamageDeath(player, reason))
            {
                return "a deadly fall";
            }
            if(reason.SourceNPCIndex != -1)
            {
                int NpcId = Main.npc[reason.SourceNPCIndex].netID;
                return Lang.GetNPCNameValue(NpcId);
            }
            else if(reason.SourceProjectileType != -1)
            {
                return Lang.GetProjectileName(reason.SourceProjectileType).Value;
            }
            else if(reason.SourcePlayerIndex != -1)
            {
                return Main.player[reason.SourcePlayerIndex].name;
            }
            else if(reason.SourceItemType != -1)
            {
                return Main.item[reason.SourceItemType].Name;
            }
            else
            {
                return "Unknown Killer?!";
            }
        }

        private bool IsFallDamageDeath(Player player, PlayerDeathReason reason)
        {
            string deathText = reason.GetDeathText(player.name).ToString();
            return deathText.Contains("fell to their death") || deathText.Contains("didn't bounce");
        }
    }
}