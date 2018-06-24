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

namespace TerrariaDeathCounter
{
    [ApiVersion(2, 1)]
    class TerrariaDeathCounterPlugin : TerrariaPlugin
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
                e.Handled = true;
                return;
            }

            Debug.WriteLine("Recv: {0:X}: {2} ({1:XX})", e.Msg.whoAmI, (byte)type, type);

            using (MemoryStream data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length - 1))
            {
                // Exceptions are already handled
                var player = Main.player[e.Msg.whoAmI];
                e.Handled = RecordPlayerDeath(player, data);
            }
        }

        private bool RecordPlayerDeath(Player player, MemoryStream data)
        {
            PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            Debug.WriteLine(string.Format("-> {0} -> {1}", GetNameOfKiller(playerDeathReason), playerDeathReason.GetDeathText(player.name)));

            string killerName = GetNameOfKiller(playerDeathReason);
            int totalDeaths = deathRecords.RecordDeath(player.name, killerName);

            string severMessage = string.Format("{0} has now died to '{1}' {2} times.", player.name, killerName, totalDeaths);
            TShockAPI.Utils.Instance.Broadcast(severMessage, 128, 0, 0);

            return true;
        }

        private string GetNameOfKiller(PlayerDeathReason reason)
        {
            if(reason.SourceNPCIndex != -1)
            {
                return Main.npc[reason.SourceNPCIndex].GetGivenOrTypeNetName()._text;
            }
            else if(reason.SourceProjectileType != -1)
            {
                return Lang.GetProjectileName(reason.SourceProjectileType).Key;
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
                return "Unkown Killer?!";
            }
        }
    }
}