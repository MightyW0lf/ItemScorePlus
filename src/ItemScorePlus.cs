using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using static ItemScorePlus.Configuration;

namespace ItemScorePlus {

    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.xoxfaby.BetterUI")]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [R2APISubmoduleDependency(nameof(LanguageAPI))]

    public class ItemScorePlus : BaseUnityPlugin {

        // General metadata.
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MightyWolf_";
        public const string PluginName = "ItemScorePlus";
        public const string PluginVersion = "1.0.0";

        /// <summary>
        /// Run at the very start when the game is initialized.
        /// </summary>
        public void Awake() {
            Log.Init(Logger);

            // Configuration bindings.
            BindAll(Config);

            // Append item scores when item catalog is ready.
            if (Appearance.Value != AppearanceEnum.Hidden) {
                ItemCatalog.availability.CallWhenAvailable(ItemDescription.AppendItemScore);
            }
        }
    }
}
