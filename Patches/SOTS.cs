using Terraria.ModLoader;
using System.Reflection;
using Terraria.Localization;

namespace VariousModPatches
{
    public class SOTS : ModSystem
    {
        public override void OnWorldLoad()
        {
            if (NetmodeHelper.IsServer) return;
            if (!Instances.Config.PatchSOTS) return;

            var mod = VersionChecker.GetModChecked("SOTS", "SOTS");
            if (mod == null) return; 

            OverrideLocalizationKey("Mods.SOTS.Common.worldEnter", "");
            OverrideLocalizationKey("Mods.SOTS.Common.worldEnterThanks", "");
        }

        private static void OverrideLocalizationKey(string key, string newValue)
        {
            try
            {
                var langManager = LanguageManager.Instance;

                if (!langManager.Exists(key))
                {
                    FileLogger.Warn("SOTS", $"Localization key not found: {key}");
                    return;
                }

                var text = langManager.GetText(key);
                var field = typeof(LocalizedText).GetField("_value",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (field == null)
                {
                    FileLogger.Warn("SOTS", $"Cannot set value for key: {key} (field not found)");
                    return;
                }

                field.SetValue(text, newValue);
            }
            catch (System.Exception ex)
            {
                FileLogger.Error("SOTS", $"Failed to override localization key {key}: {ex.Message}");
            }
        }
    }
}

// Hidden code, cannot remove messages