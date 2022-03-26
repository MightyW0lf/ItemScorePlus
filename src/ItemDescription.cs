using RoR2;
using BetterUI;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using R2API;
using static ItemScorePlus.Configuration;

namespace ItemScorePlus {

    internal class ItemDescription {

        /// <summary>
        /// Dictionary with list of item scores of all items in each tier, sorted in ascending order.
        /// </summary>
        private static Dictionary<ItemTier, List<int>> ScoresPerTier;

        /// <summary>
        /// Appends BetterUI's item score to the descriptions of all items.
        /// </summary>
        internal static void AppendItemScore() {

            if (Appearance.Value == AppearanceEnum.Detailed) {
                ScoresPerTier = GetScoresPerTier(); // Needed only for detailed item score description.
            }

            Log.LogDebug($"Appending item score info to item descriptions (appearance={Appearance.Value}).");
            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (item.tier != ItemTier.NoTier) { // Ignore notier items as they cause issues and are not needed.
                    int itemScore = ItemCounters.GetItemScore(item);
                    string toAppend = Appearance.Value switch {
                        AppearanceEnum.Compact => $"<style=cStack> +{itemScore} item score.</style>",
                        AppearanceEnum.Detailed => GetDetailedItemScoreAppend(itemScore, item.tier),
                        _ => $"\n\nItem score: <style=cIsUtility>{itemScore}</style>" // AppearanceEnum.Spacious or otherwise
                    };

                    // Append via LanguageAPI to make it visible in logbook (and anywhere else in general) as well.
                    LanguageAPI.Add(item.descriptionToken, $"{Language.GetString(item.descriptionToken)}{toAppend}");

                    Log.LogDebug($"Appended item score {itemScore} to item {item.name}.");
                }
            }
            Log.LogInfo("BetterUI's item score appended to all item descriptions.");
        }

        /// <summary>
        /// Builds a string with detailed information about the item's item score.
        /// </summary>
        /// <param name="itemScore"></param> Item's score.
        /// <param name="itemTier"></param> Item's tier.
        /// <returns>String ready to be appended to the item's description.</returns>
        private static string GetDetailedItemScoreAppend(int itemScore, ItemTier itemTier) {
            StringBuilder sb = new();
            sb.Append("\n\nItem score: <style=cIsUtility>");
            sb.Append(itemScore);
            sb.Append("</style>");

            if (ScoresPerTier.TryGetValue(itemTier, out List<int> scores) && scores.Sum() != 0) {

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
        private static Dictionary<ItemTier, List<int>> GetScoresPerTier() {
            Dictionary<ItemTier, List<int>> scores = new();
            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (item.tier != ItemTier.NoTier) {
                    if (scores.TryGetValue(item.tier, out List<int> tierScores)) {
                        tierScores.Add(ItemCounters.GetItemScore(item));
                    } else {
                        scores[item.tier] = new List<int> { ItemCounters.GetItemScore(item) };
                    }
                }
            }

            foreach (List<int> tierScores in scores.Values) {
                tierScores.Sort();
            }

            return scores;
        }
    }
}
