using BetterUI;
using RoR2;
using System.Collections.Generic;

namespace ItemScorePlus {

    internal class Utils {

        /// <summary>
        /// Dictionary with list of item scores of all items in each tier, sorted in ascending order.
        /// Used for building detailed item score info.
        /// </summary>
        internal static Dictionary<ItemTier, List<float>> ScoresPerTier;

        /// <summary>
        /// Initialize the utils module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {
            ItemCatalog.availability.CallWhenAvailable(BuildItemDictionaries);

            Log.LogDebug($"Utilities initialized.");
        }

        /// <returns>Dictionary with item score stats for each item tier.</returns>
        private static void BuildItemDictionaries() {

            ScoresPerTier = new Dictionary<ItemTier, List<float>>();

            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (ScoresPerTier.TryGetValue(item.tier, out List<float> tierScores)) {
                    tierScores.Add(ItemCounters.GetItemScore(item));
                } else {
                    ScoresPerTier[item.tier] = new List<float> { ItemCounters.GetItemScore(item) };
                }
            }

            foreach (List<float> tierScores in ScoresPerTier.Values) {
                tierScores.Sort();
            }
        }
    }
}
