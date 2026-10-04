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
            if (!Netmode.IsSingleplayer) return;
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
            _difficultyType = Reflection.GetType(mod, "InfernumMasterPatch.MasterPatchDifficulty", "InfernumMasterPatch");
            return _difficultyType != null;
        }

        private static bool FindEnabledProperty(out PropertyInfo prop)
        {
            prop = Reflection.GetProperty(_difficultyType, "Enabled",
                BindingFlags.Public | BindingFlags.Instance, "InfernumMasterPatch");
            return prop != null;
        }

        private static bool FindCalamityTypes()
        {
            var calamity = VersionChecker.GetModChecked("CalamityMod", "InfernumMasterPatch", isSelf: false);
            if (calamity == null) return false;

            var sysType = Reflection.GetType(calamity, "CalamityMod.Systems.DifficultyModeSystem", "InfernumMasterPatch");
            if (sysType == null) return false;

            _difficultiesField = Reflection.GetField(sysType, "Difficulties",
                BindingFlags.Static | BindingFlags.Public, "InfernumMasterPatch");
            if (_difficultiesField == null) return false;

            _calculateMethod = Reflection.GetMethod(sysType, "CalculateDifficultyData",
                BindingFlags.Static | BindingFlags.Public, "InfernumMasterPatch");
            if (_calculateMethod == null) return false;

            var worldType = Reflection.GetType(calamity, "CalamityMod.World.CalamityWorld", "InfernumMasterPatch");
            if (worldType == null) return false;

            _deathField = Reflection.GetField(worldType, "death",
                BindingFlags.Static | BindingFlags.Public, "InfernumMasterPatch");
            return _deathField != null;
        }

        private static bool FindInfernumTypes()
        {
            var infernum = VersionChecker.GetModChecked("InfernumMode", "InfernumMasterPatch", isSelf: false);
            if (infernum == null) return false;

            var worldSaveType = Reflection.GetType(infernum,
                "InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem", "InfernumMasterPatch");
            if (worldSaveType == null) return false;

            _infernumModeEnabledProp = Reflection.GetProperty(worldSaveType, "InfernumModeEnabled",
                BindingFlags.Static | BindingFlags.Public, "InfernumMasterPatch");
            return _infernumModeEnabledProp != null;
        }

        public override void PostWorldLoad()
        {
            if (!Netmode.IsSingleplayer) return;

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