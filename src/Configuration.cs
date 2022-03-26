using BepInEx.Configuration;
using System;

namespace ItemScorePlus {
    public class Configuration {

        // Configuration.
        public enum AppearanceEnum { Compact, Spacious, Detailed, Hidden }
        public static ConfigEntry<AppearanceEnum> Appearance { get; set; }

        /// <summary>
        /// Binds all configuration fields.
        /// </summary>
        /// <param name="configFile">Configuration file to which the configuration will be bound.</param>
        public static void BindAll(ConfigFile configFile) {
            Appearance = configFile.Bind<AppearanceEnum>(
                "Item descriptions", "Appearance", AppearanceEnum.Spacious,
                $"How should the appended string with the item score info look. \"{Enum.GetName(typeof(AppearanceEnum), AppearanceEnum.Hidden)}\" disables this feature entirely. \"{Enum.GetName(typeof(AppearanceEnum), AppearanceEnum.Detailed)}\" shows more information than just the item score."
            );
        }
    }
}
