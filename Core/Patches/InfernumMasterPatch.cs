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
            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (!config.PatchInfernumMasterPatch) return;

            var mod = VersionHelper.GetModAndWarn("InfernumMasterPatch", "InfernumMasterPatch");
            if (mod == null) return;            

            _difficultyType = mod.Code.GetType("InfernumMasterPatch.MasterPatchDifficulty");
            if (_difficultyType == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) MasterPatchDifficulty type not found");
                return;
            }

            _difficultyEnabledProp = _difficultyType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
            if (_difficultyEnabledProp == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) Enabled property not found");
            }

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                var sysType = calamity.Code.GetType("CalamityMod.Systems.DifficultyModeSystem");
                if (sysType != null)
                {
                    _difficultiesProp = sysType.GetProperty("Difficulties", BindingFlags.Static | BindingFlags.Public);
                    if (_difficultiesProp == null)
                    {
                        _difficultiesField = sysType.GetField("Difficulties", BindingFlags.Static | BindingFlags.Public);
                        if (_difficultiesField == null)
                        {
                            Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) Difficulties field not found");
                        }
                    }
                    _calculateMethod = sysType.GetMethod("CalculateDifficultyData", BindingFlags.Static | BindingFlags.Public);
                    if (_calculateMethod == null)
                    {
                        Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) CalculateDifficultyData method not found");
                    }
                }
                else
                {
                    Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) DifficultyModeSystem type not found");
                }

                var worldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
                if (worldType != null)
                {
                    _deathField = worldType.GetField("death", BindingFlags.Static | BindingFlags.Public);
                    if (_deathField == null)
                    {
                        Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) death field not found");
                    }
                }
                else
                {
                    Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) CalamityWorld type not found");
                }
            }

            if (ModLoader.TryGetMod("InfernumMode", out Mod infernum))
            {
                var worldSaveType = infernum.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem");
                if (worldSaveType != null)
                {
                    _infernumModeEnabledProp = worldSaveType.GetProperty("InfernumModeEnabled", BindingFlags.Static | BindingFlags.Public);
                    if (_infernumModeEnabledProp == null)
                    {
                        Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) InfernumModeEnabled property not found");
                    }
                }
                else
                {
                    Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) WorldSaveSystem type not found");
                }
            }
        }

        public override void PostWorldLoad()
        {
            if (Main.netMode == NetmodeID.Server) return;

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

        private void EnablePatch()
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
                    Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) Difficulties list is null");
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
                    Mod.Logger.Warn("[VariousModPatches] (InfernumMasterPatch) MasterPatchDifficulty not found in difficulties list");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"[VariousModPatches] (InfernumMasterPatch) EnablePatch failed: {ex.Message}");
            }
        }
    }
}

// Imitation of "saved" difficulty, no any side effects