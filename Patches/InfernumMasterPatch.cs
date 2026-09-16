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
        private static PropertyInfo _difficultiesProp;
        private static MethodInfo _calculateMethod;
        private static PropertyInfo _difficultyEnabledProp;
        private static FieldInfo _difficultiesField;
        private static PropertyInfo _infernumModeEnabledProp;
        private static FieldInfo _deathField;

        public override void PostSetupContent()
        {
            if (!Instances.Config.PatchInfernumMasterPatch) return;

            var mod = VersionChecker.GetModChecked("InfernumMasterPatch", "InfernumMasterPatch");
            if (mod == null) return;

            _difficultyType = mod.Code.GetType("InfernumMasterPatch.MasterPatchDifficulty");
            if (_difficultyType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "MasterPatchDifficulty type not found");
                return;
            }

            _difficultyEnabledProp = FindEnabledProperty();
            FindCalamityTypes();
            FindInfernumTypes();
        }

        private static PropertyInfo FindEnabledProperty()
        {
            var prop = _difficultyType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                FileLogger.Warn("InfernumMasterPatch", "Enabled property not found");
            return prop;
        }

        private static void FindCalamityTypes()
        {
            var calamity = VersionChecker.GetModChecked("CalamityMod", "InfernumMasterPatch", isSelf: false);

            var sysType = calamity.Code.GetType("CalamityMod.Systems.DifficultyModeSystem");
            if (sysType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "DifficultyModeSystem type not found");
                return;
            }

            _difficultiesProp = sysType.GetProperty("Difficulties", BindingFlags.Static | BindingFlags.Public);
            if (_difficultiesProp == null)
            {
                _difficultiesField = sysType.GetField("Difficulties", BindingFlags.Static | BindingFlags.Public);
                if (_difficultiesField == null)
                    FileLogger.Warn("InfernumMasterPatch", "Difficulties field not found");
            }

            _calculateMethod = sysType.GetMethod("CalculateDifficultyData", BindingFlags.Static | BindingFlags.Public);
            if (_calculateMethod == null)
                FileLogger.Warn("InfernumMasterPatch", "CalculateDifficultyData method not found");

            var worldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
            if (worldType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "CalamityWorld type not found");
                return;
            }

            _deathField = worldType.GetField("death", BindingFlags.Static | BindingFlags.Public);
            if (_deathField == null)
                FileLogger.Warn("InfernumMasterPatch", "death field not found");
        }

        private static void FindInfernumTypes()
        {
            var infernum = VersionChecker.GetModChecked("InfernumMode", "InfernumMasterPatch", isSelf: false);

            var worldSaveType = infernum.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem");
            if (worldSaveType == null)
            {
                FileLogger.Warn("InfernumMasterPatch", "WorldSaveSystem type not found");
                return;
            }

            _infernumModeEnabledProp = worldSaveType.GetProperty("InfernumModeEnabled", BindingFlags.Static | BindingFlags.Public);
            if (_infernumModeEnabledProp == null)
                FileLogger.Warn("InfernumMasterPatch", "InfernumModeEnabled property not found");
        }

        public override void Unload()
        {
            _difficultyType = null;
            _difficultiesProp = null;
            _calculateMethod = null;
            _difficultyEnabledProp = null;
            _difficultiesField = null;
            _infernumModeEnabledProp = null;
            _deathField = null;
        }

        public override void PostWorldLoad()
        {
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

        private static void EnablePatch()
        {
            try
            {
                // Calamity difficulties list
                IList list = null;

                if (_difficultiesProp != null)
                    list = _difficultiesProp.GetValue(null) as IList;
                else if (_difficultiesField != null)
                    list = _difficultiesField.GetValue(null) as IList;

                if (list == null)
                {
                    _calculateMethod?.Invoke(null, null);
                    if (_difficultiesProp != null)
                        list = _difficultiesProp.GetValue(null) as IList;
                    else if (_difficultiesField != null)
                        list = _difficultiesField.GetValue(null) as IList;
                }

                if (list == null)
                {
                    FileLogger.Warn("InfernumMasterPatch", "Difficulties list is null");
                    return;
                }

                // Searching for IMP difficulty
                bool found = false;
                foreach (var diff in list)
                {
                    if (diff.GetType() == _difficultyType)
                    {
                        _difficultyEnabledProp?.SetValue(diff, true);
                        _calculateMethod?.Invoke(null, null);
                        found = true;
                        return;
                    }
                }

                if (!found)
                {
                    FileLogger.Warn("InfernumMasterPatch", "MasterPatchDifficulty not found in difficulties list");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error("InfernumMasterPatch", $"EnablePatch failed: {ex.Message}");
            }
        }
    }
}

// Imitation of "saved" difficulty, no any side effects