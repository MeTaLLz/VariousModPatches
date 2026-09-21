using System;
using System.Collections;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VariousModPatches
{
    public class InfernumMasterPatch : ModSystem
    {
        private static Type _difficultyType;
        private static FieldInfo _difficultiesField;
        private static MethodInfo _calculateMethod;
        private static PropertyInfo _difficultyEnabledProp;
        private static PropertyInfo _infernumModeEnabledProp;
        private static FieldInfo _deathField;

        public override void PostSetupContent()
        {
            // Maybe later will support multiplayer
            if (!NetmodeHelper.IsSingleplayer) return;
            if (!Instances.Config.PatchInfernumMasterPatch) return;

            var mod = VersionChecker.GetModChecked("InfernumMasterPatch", "InfernumMasterPatch");
            if (mod == null) return;

            if (!FindDifficultyType(mod)) return;
            if (!FindEnabledProperty(out _difficultyEnabledProp)) return;
            if (!FindCalamityTypes()) return;
            if (!FindInfernumTypes()) return;
        }

        public override void Unload()
        {
            _difficultyType = null;
            _difficultiesField = null;
            _calculateMethod = null;
            _difficultyEnabledProp = null;
            _infernumModeEnabledProp = null;
            _deathField = null;
        }

        private static bool FindDifficultyType(Mod mod)
        {
            _difficultyType = mod.Code.GetType("InfernumMasterPatch.MasterPatchDifficulty");
            if (_difficultyType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "MasterPatchDifficulty type not found");
                return false;
            }
            return true;
        }

        private static bool FindEnabledProperty(out PropertyInfo prop)
        {
            prop = _difficultyType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "Enabled property not found");
                return false;
            }
            return true;
        }

        private static bool FindCalamityTypes()
        {
            var calamity = VersionChecker.GetModChecked("CalamityMod", "InfernumMasterPatch", isSelf: false);
            if (calamity == null) return false;

            var sysType = calamity.Code.GetType("CalamityMod.Systems.DifficultyModeSystem");
            if (sysType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "DifficultyModeSystem type not found");
                return false;
            }

            _difficultiesField = sysType.GetField("Difficulties", BindingFlags.Static | BindingFlags.Public);
            if (_difficultiesField == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "Difficulties field not found");
                return false;
            }

            _calculateMethod = sysType.GetMethod("CalculateDifficultyData", BindingFlags.Static | BindingFlags.Public);
            if (_calculateMethod == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "CalculateDifficultyData method not found");
                return false;
            }

            var worldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
            if (worldType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "CalamityWorld type not found");
                return false;
            }

            _deathField = worldType.GetField("death", BindingFlags.Static | BindingFlags.Public);
            if (_deathField == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "death field not found");
                return false;
            }

            return true;
        }

        private static bool FindInfernumTypes()
        {
            var infernum = VersionChecker.GetModChecked("InfernumMode", "InfernumMasterPatch", isSelf: false);
            if (infernum == null) return false;

            var worldSaveType = infernum.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem");
            if (worldSaveType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "WorldSaveSystem type not found");
                return false;
            }

            _infernumModeEnabledProp = worldSaveType.GetProperty("InfernumModeEnabled", BindingFlags.Static | BindingFlags.Public);
            if (_infernumModeEnabledProp == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "InfernumModeEnabled property not found");
                return false;
            }

            return true;
        }

        public override void PostWorldLoad()
        {
            if (!NetmodeHelper.IsSingleplayer) return;

            bool isMaster = Main.GameMode == GameModeID.Master;
            bool isDeath = false;
            bool isInfernum = false;

            if (_deathField != null)
                isDeath = (bool)_deathField.GetValue(null);

            if (_infernumModeEnabledProp != null)
                isInfernum = (bool)_infernumModeEnabledProp.GetValue(null);

            if (isMaster && isDeath && isInfernum)
            {
                EnablePatch();
            }
        }

        public static void EnablePatch()
        {
            try
            {
                // Calamity difficulty list
                IList list = null;

                if (_difficultiesField != null)
                    list = _difficultiesField.GetValue(null) as IList;

                if (list == null)
                {
                    _calculateMethod?.Invoke(null, null);
                    if (_difficultiesField != null)
                        list = _difficultiesField.GetValue(null) as IList;
                }

                if (list == null)
                {
                    FileLogger.Warn("InfernumMasterPatch", "Difficulties list is null");
                    return;
                }

                // IMP difficulty
                foreach (var diff in list)
                {
                    if (diff.GetType() == _difficultyType)
                    {
                        _difficultyEnabledProp?.SetValue(diff, true);
                        _calculateMethod?.Invoke(null, null);
                        return;
                    }
                }

                FileLogger.Warn("InfernumMasterPatch", "MasterPatchDifficulty not found in difficulties list");
            }
            catch (Exception ex)
            {
                FileLogger.Error("InfernumMasterPatch", $"EnablePatch failed: {ex.Message}");
            }
        }
    }
}

// Imitation of "saved" difficulty, no any side effects