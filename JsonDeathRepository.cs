using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

namespace TerrariaDeathCounter
{
    class JsonDeathRepository : IDeathRepository
    {
        private string filename = string.Empty;
        private DeathLedger ledger = new DeathLedger();

        public JsonDeathRepository(string filename)
        {
            this.filename = filename;

            if(!File.Exists(filename))
            {
                File.CreateText(filename);
            }
        }

        public int GetNumberOfDeaths(string playerName, string killerName)
        {
            try
            {
                return ledger.records[playerName].deathCounts[killerName];
            }
            catch(KeyNotFoundException)
            {
                // Player not found, or have not died to this killer.
                return 0;
            }
        }

        public int RecordDeath(string playerName, string killerName)
        {                                                   
            if (ledger.records.ContainsKey(playerName))
            {
                DeathRecord playerRecord = ledger.records[playerName];

                if (playerRecord.deathCounts.ContainsKey(killerName))
                {
                    // Record additional death by this killer
                    playerRecord.deathCounts[killerName] += 1;
                }
                else
                {
                    // Record first death by this killer
                    playerRecord.deathCounts.Add(killerName, 1);
                }
            }
            else
            {
                // Player has not died before. Create new record, and add one for this killer.
                ledger.records.Add(playerName, new DeathRecord()
                {
                    deathCounts = new Dictionary<string, int>()
                    {
                        { killerName, 1 }
                    }
                });
            }

            // Record pretty-printed JSON to file.
            File.WriteAllText(filename, JsonConvert.SerializeObject(ledger, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() }));

            return ledger.records[playerName].deathCounts[killerName];
        }
    }

    class DeathLedger
    {
        public Dictionary<string, DeathRecord> records = new Dictionary<string, DeathRecord>();
    }

    class DeathRecord
    {
        public Dictionary<string, int> deathCounts = new Dictionary<string, int>();
    }
}
