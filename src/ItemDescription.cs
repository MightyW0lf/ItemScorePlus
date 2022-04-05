using RoR2;
using BetterUI;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using static ItemScorePlus.Configuration;

namespace ItemScorePlus {

    internal class ItemDescription {

        /// <summary>
        /// Dictionary with list of item scores of all items in each tier, sorted in ascending order.
        /// </summary>
        private static Dictionary<ItemTier, List<float>> ScoresPerTier;

        /// <summary>
        /// Initialize the item description module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {

            if (Appearance.Value == AppearanceEnum.Detailed) { // Needed only for detailed item score description.
                Log.LogDebug("Item description formatting is set to detailed, gathering stats to display...");
                ItemCatalog.availability.CallWhenAvailable(() => ScoresPerTier = GetScoresPerTier());
            }

            // Hook that ensures the item score info is appended to the item's on hover description.
            On.RoR2.UI.ItemIcon.SetItemIndex += (orig, self, index, count) => {
                orig(self, index, count);
                self.tooltipProvider.overrideBodyText += GetItemScoreInfo(ItemCatalog.GetItemDef(index));
            };
        }

        /// <returns>A string with information about the given item's item score with formatting based 
        /// on <see cref="Appearance"/>Appearance.</returns>
        private static string GetItemScoreInfo(ItemDef item) {
            float itemScore = ItemCounters.GetItemScore(item);
            return Appearance.Value switch {
                AppearanceEnum.Compact => $"<style=cStack> +{Math.Round(itemScore, 2)} item score.</style>",
                AppearanceEnum.Detailed => GetDetailedItemScoreAppend(itemScore, item.tier),
                _ => $"\n\nItem score: <style=cIsUtility>{Math.Round(itemScore, 2)}</style>" // AppearanceEnum.Spacious or otherwise
            };
        }

        /// <summary>
        /// Builds a string with detailed information about the item's item score.
        /// </summary>
        /// <param name="itemScore"></param> Item's score.
        /// <param name="itemTier"></param> Item's tier.
        /// <returns>String ready to be appended to the item's description.</returns>
        private static string GetDetailedItemScoreAppend(float itemScore, ItemTier itemTier) {
            StringBuilder sb = new();
            sb.Append("\n\nItem score: <style=cIsUtility>");
            sb.Append(Math.Round(itemScore, 2));
            sb.Append("</style>");

            if (ScoresPerTier.TryGetValue(itemTier, out List<float> scores) && scores.Sum() != 0) {

                int relative = (int) Math.Round(itemScore / scores.Average() * 100);
                sb.Append("\n  > ");
                sb.Append(relative switch { // Coloring based on how "good" the item score is.
                    <  75 => "<color=#FF7F7F>",
                    > 125 => "<style=cIsHealing>",
                    _     => "<style=cIsDamage>"
                });
                sb.Append(relative);
                sb.Append("%</style></color> of this item's tier average");

                // Lower is % of items with lower item score, higher is % of items with lower OR EQUAL item score.
                int perfLower = (int) Math.Round((double) scores.FindIndex(score => score == itemScore) / scores.Count() * 100);
                int perfHigher = (int) Math.Round((double) (scores.FindLastIndex(score => score == itemScore) + 1) / scores.Count() * 100);
                sb.Append("\n  > Higher than ");
                sb.Append(((perfLower + perfHigher) / 2) switch { // Coloring based on an average of those two values.
                    < 40 => "<color=#FF7F7F>",
                    > 60 => "<style=cIsHealing>",
                    _    => "<style=cIsDamage>"
                });
                if (perfLower != perfHigher) {
                    sb.Append(perfLower);
                    sb.Append("-");
                    sb.Append(perfHigher);
                } else {
                    sb.Append(perfLower);
                }
                sb.Append("%</style></color> of items in the same tier");
            }
            return sb.ToString();
        }

        /// <returns>Dictionary with item score stats for each item tier.</returns>
        private static Dictionary<ItemTier, List<float>> GetScoresPerTier() {
            Dictionary<ItemTier, List<float>> scores = new();
            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (item.tier != ItemTier.NoTier) {
                    if (scores.TryGetValue(item.tier, out List<float> tierScores)) {
                        tierScores.Add(ItemCounters.GetItemScore(item));
                    } else {
                        scores[item.tier] = new List<float> { ItemCounters.GetItemScore(item) };
                    }
                }
            }

            foreach (List<float> tierScores in scores.Values) {
                tierScores.Sort();
            }

            return scores;
        }
    }
}
