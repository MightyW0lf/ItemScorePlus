using RoR2;
using BetterUI;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using static ItemScorePlus.Configuration;
using HarmonyLib;

namespace ItemScorePlus {

    [HarmonyPatch]
    internal class ItemDescription {

        /// <summary>
        /// Dictionary with list of item scores of all items in each tier, sorted in ascending order.
        /// Used for building detailed item score info.
        /// </summary>
        private static Dictionary<ItemTier, List<float>> ScoresPerTier;

        /// <summary>
        /// Dictionary with list of all item name tokens and indeces of their items.
        /// Used for retrieving item index from name tokens.
        /// </summary>
        private static Dictionary<string, ItemIndex> ItemTokens;

        /// <summary>
        /// Initialize the item description module. Should be called once on game initialization.
        /// </summary>
        internal static void Init() {
            Log.LogDebug($"Initializing the ItemDescription module...");

            ItemCatalog.availability.CallWhenAvailable(() => BuildItemDictionaries());

            // Hook that ensures the item score info is appended to the item's on hover description.
            On.RoR2.UI.ItemIcon.SetItemIndex += (orig, self, index, count) => {
                orig(self, index, count);
                self.tooltipProvider.overrideBodyText += GetItemScoreInfo(ItemCatalog.GetItemDef(index));
            };

            // Harmony patch.
            new PatchClassProcessor(new Harmony(ItemScorePlus.PluginGUID), typeof(ItemDescription)).Patch();

            Log.LogInfo($"ItemDescription module initialized.");
        }

        /// <returns>Dictionary with item score stats for each item tier.</returns>
        private static void BuildItemDictionaries() {

            ScoresPerTier = new();
            ItemTokens = new();

            foreach (ItemDef item in ItemCatalog.allItemDefs) {
                if (item.tier != ItemTier.NoTier) { // Including noTier items is not necessary and causes issues.
                    if (ScoresPerTier.TryGetValue(item.tier, out List<float> tierScores)) {
                        tierScores.Add(ItemCounters.GetItemScore(item));
                    } else {
                        ScoresPerTier[item.tier] = new List<float> { ItemCounters.GetItemScore(item) };
                    }
                    ItemTokens.Add(item.nameToken, item.itemIndex);
                }
            }

            foreach (List<float> tierScores in ScoresPerTier.Values) {
                tierScores.Sort();
            }
        }

        /// <returns>A string with information about the given item's item score with formatting based
        /// on <see cref="Appearance"/>Appearance.</returns>
        private static string GetItemScoreInfo(ItemDef item) {
            float itemScore = ItemCounters.GetItemScore(item);
            return Appearance.Value switch {
                AppearanceEnum.Compact => $"<style=cStack> +{Math.Round(itemScore, 2)} item score.</style>",
                AppearanceEnum.Detailed => GetDetailedItemScoreInfo(itemScore, item.tier),
                _ => $"\n\nItem score: <style=cIsUtility>{Math.Round(itemScore, 2)}</style>" // AppearanceEnum.Spacious or otherwise
            };
        }

        /// <summary>
        /// Builds a string with detailed information about the item's item score.
        /// </summary>
        /// <param name="itemScore"></param> Item's score.
        /// <param name="itemTier"></param> Item's tier.
        /// <returns>String ready to be appended to the item's description.</returns>
        private static string GetDetailedItemScoreInfo(float itemScore, ItemTier itemTier) {
            StringBuilder sb = new();
            sb.Append("\n\nItem score: <style=cIsUtility>");
            sb.Append(Math.Round(itemScore, 2));
            sb.Append("</style>");

            if (ScoresPerTier.TryGetValue(itemTier, out List<float> scores) && scores.Sum() != 0) {

                int relative = (int)Math.Round(itemScore / scores.Average() * 100);
                sb.Append("\n  > ");
                sb.Append(relative switch { // Coloring based on how "good" the item score is.
                    < 75 => "<color=#FF7F7F>",
                    > 125 => "<style=cIsHealing>",
                    _ => "<style=cIsDamage>"
                });
                sb.Append(relative);
                sb.Append("%</style></color> of this item's tier average");

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
                Log.LogDebug($"Failed to append item score to item with name token {__instance.titleToken} via ItemIcon.SetItemIndex hook (using TooltipProvider.bodyText hook instead).");

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
