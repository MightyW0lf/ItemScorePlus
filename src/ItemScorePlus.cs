using BepInEx;
using R2API.Utils;
using static ItemScorePlus.Configuration;

namespace ItemScorePlus {

    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.xoxfaby.BetterUI")]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]

    public class ItemScorePlus : BaseUnityPlugin {

        // General metadata.
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MightyWolf_";
        public const string PluginName = "ItemScorePlus";
        public const string PluginVersion = "1.1.2";

        /// <summary>
        /// Run at the very start when the game is initialized.
        /// </summary>
        public void Awake() {
            
            Log.Init(Logger); // Logging module.
            BindAll(Config); // Configuration bindings.
            Utils.Init(); // Utilities.

            ItemDescription.Init(); // ItemDescription module.
            ItemScoreStats.Init(); // ItemScoreStats module.
        }
    }
}
