using BetterUI;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ItemScorePlus.Configuration;
using static ItemScorePlus.Utils;

namespace ItemScorePlus {
    internal class ItemScoreStats {

        /// <summary>
        /// Initialize the ItemScoreStats module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {
            if (ItemScoreStatsLogDump.Value) {
                ItemCatalog.availability.CallWhenAvailable(() => {
                    LogItemScoreStats();
                });
            }

            Log.LogDebug($"ItemScoreStats module initialized.");
        }

        /// <summary>
        /// Logs statistics about item scores.
        /// </summary>
        private static void LogItemScoreStats() {
            Log.LogInfo($"Printing item score statistics for each item tier for total of {ItemCatalog.itemCount} items...");
            foreach (KeyValuePair<ItemTier, List<float>> scores in ScoresPerTier) {
                StringBuilder sb = new();
                sb.Append($"Tier {scores.Key}:\n");
                sb.Append($"  > Item count: {scores.Value.Count}\n");
                sb.Append($"  > Default item score: {Math.Round(ItemCounters.GetTierScore(scores.Key), 2)}\n");
                sb.Append($"  > Average item score: {Math.Round(scores.Value.Average(), 2)}\n");
                sb.Append($"  > MIN - MAX item score: {Math.Round(scores.Value.Min(), 2)} - {Math.Round(scores.Value.Max(), 2)}");
                Log.LogInfo(sb.ToString());
            }
        }
    }
}
