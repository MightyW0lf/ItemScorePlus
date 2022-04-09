using BepInEx.Configuration;
using System;

namespace ItemScorePlus {
    public class Configuration {

        // Configuration.
        public enum AppearanceEnum { Compact, Spacious, Detailed, Hidden }
        public static ConfigEntry<AppearanceEnum> Appearance { get; set; }
        public static ConfigEntry<Boolean> ItemScoreStatsLogDump { get; set; }

        /// <summary>
        /// Binds all configuration fields.
        /// </summary>
        /// <param name="configFile">Configuration file to which the configuration will be bound.</param>
        public static void BindAll(ConfigFile configFile) {

            // Item descriptions
            Appearance = configFile.Bind<AppearanceEnum>(
                "Item descriptions", "Appearance", AppearanceEnum.Spacious,
                $"How should the appended string with the item score info look. \"{Enum.GetName(typeof(AppearanceEnum), AppearanceEnum.Hidden)}\" disables this feature entirely. \"{Enum.GetName(typeof(AppearanceEnum), AppearanceEnum.Detailed)}\" shows more information than just the item score."
            );

            // Miscellaneous
            ItemScoreStatsLogDump = configFile.Bind<Boolean>(
                "Item score stats", "Log dump", true,
                "If true, item score stats will be printed to log output everytime the game starts."
            );

            Log.LogDebug("Configuration loaded.");
        }
    }
}
