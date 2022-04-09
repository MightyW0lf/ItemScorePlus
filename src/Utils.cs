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
        /// Dictionary with list of all item name tokens and indeces of their items.
        /// Used for retrieving item index from name tokens.
        /// </summary>
        internal static Dictionary<string, ItemIndex> ItemTokens;

        /// <summary>
        /// Initialize the utils module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {
            ItemCatalog.availability.CallWhenAvailable(() => {
                BuildItemDictionaries();
            });

            Log.LogDebug($"Utilities initialized.");
        }

        /// <returns>Dictionary with item score stats for each item tier.</returns>
        internal static void BuildItemDictionaries() {

            ScoresPerTier = new();
            ItemTokens = new();

            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (ScoresPerTier.TryGetValue(item.tier, out List<float> tierScores)) {
                    tierScores.Add(ItemCounters.GetItemScore(item));
                } else {
                    ScoresPerTier[item.tier] = new List<float> { ItemCounters.GetItemScore(item) };
                }
                ItemTokens[item.nameToken] = item.itemIndex;
            }

            foreach (List<float> tierScores in ScoresPerTier.Values) {
                tierScores.Sort();
            }
        }
    }
}
