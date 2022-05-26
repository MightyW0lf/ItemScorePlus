using RoR2;
using BetterUI;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using static ItemScorePlus.Configuration;
using static ItemScorePlus.Utils;

namespace ItemScorePlus {

    [HarmonyPatch]
    internal class ItemDescription {

        /// <summary>
        /// Initialize the ItemDescription module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {
            // Harmony patch that ensures the item score info is appended to the item's on hover description.
            new PatchClassProcessor(new Harmony(ItemScorePlus.PluginGUID), typeof(ItemDescription)).Patch();

            Log.LogDebug($"ItemDescription module initialized.");
        }

        /// <returns>A string with information about the given item's item score with formatting based
        /// on <see cref="Appearance"/>Appearance.</returns>
        private static string GetItemScoreInfo(ItemDef item) {
            return Appearance.Value switch {
                AppearanceEnum.Compact => $"<style=cStack> +{Math.Round(ItemCounters.GetItemScore(item), 2)} item score.</style>",
                AppearanceEnum.Detailed => GetDetailedItemScoreInfo(item),
                _ => $"\n\nItem score: <style=cIsUtility>{Math.Round(ItemCounters.GetItemScore(item), 2)}</style>" // AppearanceEnum.Spacious or otherwise
            };
        }

        /// <summary>
        /// Builds a string with detailed information about the item's item score.
        /// </summary>
        /// <returns>String ready to be appended to the item's description.</returns>
        private static string GetDetailedItemScoreInfo(ItemDef item) {

            float itemScore = ItemCounters.GetItemScore(item);
            float tierScore = ItemCounters.GetTierScore(item.tier);

            StringBuilder sb = new();
            sb.Append("\n\nItem score: <style=cIsUtility>");
            sb.Append(Math.Round(itemScore, 2));
            sb.Append("</style>");

            if (ScoresPerTier.TryGetValue(item.tier, out List<float> scores)) {
                if (scores.Sum() != 0) {

                    if (tierScore != 0) {
                        int relative = (int)Math.Round(itemScore / tierScore * 100);
                        sb.Append("\n  > ");
                        sb.Append(relative switch
                        {
                            // Coloring based on how high the relative item score is.
                            < 75 => "<color=#FF7F7F>",
                            > 125 => "<style=cIsHealing>",
                            _ => "<style=cIsDamage>"
                        });
                        sb.Append(relative);
                        sb.Append("%</style></color> of this item's tier default <style=cStack>(");
                        sb.Append(Math.Round(tierScore, 2));
                        sb.Append(")</style>");
                    } else { // TODO: Handle tierScore == 0 (separate message)
                        sb.Append("\n  > This item's tier default is 0.");
                    }

                    // Lower is % of items with lower item score, higher is % of items with lower OR EQUAL item score.
                    int perfLower = (int)Math.Round((double)scores.FindIndex(score => score == itemScore) / scores.Count() * 100);
                    int perfHigher = (int)Math.Round((double)(scores.FindLastIndex(score => score == itemScore) + 1) / scores.Count() * 100);
                    sb.Append("\n  > Higher than ");
                    sb.Append(((perfLower + perfHigher) / 2) switch { // Coloring based on an average of those two values.
                        < 40 => "<color=#FF7F7F>",
                        > 60 => "<style=cIsHealing>",
                        _ => "<style=cIsDamage>"
                    });
                    if (perfLower != perfHigher) {
                        sb.Append(perfLower);
                        sb.Append("-");
                        sb.Append(perfHigher);
                    } else {
                        sb.Append(perfLower);
                    }
                    sb.Append("%</style></color> of items in the same tier");
                } else {
                    sb.Append("\n  <style=cStack>> All items in this tier have 0 item score.</style>");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Checks if item tooltip contains item score info and adds it if it doesn't. Functions as a fallback when
        /// item score added via a hook is overriden by another mod.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.UI.TooltipProvider), "get_bodyText")]
        public static void PatchMissingScore(RoR2.UI.TooltipProvider __instance, ref string __result) {

            // Check if this is an ItemIcon's tooltip and if it doesn't have an item score in description.
            // (use IndexOf instead of Contains for case-insensitivity).
            RoR2.UI.ItemIcon icon = __instance.GetComponentInParent<RoR2.UI.ItemIcon>();
            if (icon != null && icon.tooltipProvider.overrideBodyText.IndexOf("item score", StringComparison.OrdinalIgnoreCase) < 0) {

                // ItemIcon.itemIndex is inaccessible, try to retrieve the item index using the titleToken.
                if (ItemTokens.TryGetValue(__instance.titleToken, out ItemIndex itemIndex)) {
                    __result += GetItemScoreInfo(ItemCatalog.GetItemDef(itemIndex));
                } else {
                    Log.LogWarning($"Failed to get item index for item with name token {__instance.titleToken}.");
                }
            }
        }
    }
}
