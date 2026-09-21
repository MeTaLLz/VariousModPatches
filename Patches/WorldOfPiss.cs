using Terraria;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;
using Terraria.ID;

namespace VariousModPatches
{
    public class WorldOfPiss : ModSystem
    {
        private ILHook _ilHook;
        private static PropertyInfo _pissMoonProp;
        private static FieldInfo _hasBeatenField;
        private static int _pissFishType = -1;

        public override void PostSetupContent()
        {
            if (!Instances.Config.PatchWorldOfPiss) return;

            var mod = VersionChecker.GetModChecked("WorldOfPiss", "WorldOfPiss");
            if (mod == null) return;

            if (!ApplyPostUpdateTimeHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
            _pissMoonProp = null;
            _hasBeatenField = null;
            _pissFishType = -1;
        }

        private bool ApplyPostUpdateTimeHook(Mod mod)
        {
            if (!ModContent.TryFind("WorldOfPiss", "PissQuestFish", out ModItem pissFish))
            {
                FileLogger.Warn("WorldOfPiss", "PissQuestFish not found");
                return false;
            }

            _pissFishType = pissFish.Type;

            var targetType = mod.Code.GetType("WorldOfPiss.PissMoon.PissMoonSystem");
            if (targetType == null)
            {
                FileLogger.Warn("WorldOfPiss", "PissMoonSystem type not found");
                return false;
            }

            _pissMoonProp = targetType.GetProperty("pissMoon");
            if (_pissMoonProp == null)
            {
                FileLogger.Warn("WorldOfPiss", "pissMoon property not found");
                return false;
            }

            _hasBeatenField = targetType.GetField("hasBeatenPissMoon",
                BindingFlags.Public | BindingFlags.Instance);
            if (_hasBeatenField == null)
            {
                FileLogger.Warn("WorldOfPiss", "hasBeatenPissMoon field not found");
                return false;
            }

            var method = targetType.GetMethod("PostUpdateTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("WorldOfPiss", "PostUpdateTime method not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(method, PostUpdateTime_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("WorldOfPiss", $"ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void PostUpdateTime_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, i => true))
            {
                cursor.Remove();
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(WorldOfPiss).GetMethod("PostUpdateTime_Replacement",
                BindingFlags.NonPublic | BindingFlags.Static));
            cursor.Emit(OpCodes.Ret);
        }

        private static void PostUpdateTime_Replacement(object self)
        {
            try
            {
                if (_pissMoonProp == null)
                    return;

                // Only by angler quest — removed random chance
                if (Main.time == 0.0 && !Main.dayTime && Main.netMode != NetmodeID.MultiplayerClient &&
                    _pissFishType != -1 &&
                    Main.anglerQuestItemNetIDs[Main.anglerQuest] == _pissFishType)
                {
                    _pissMoonProp.SetValue(self, true);
                }

                bool pissMoon = (bool)_pissMoonProp.GetValue(self);

                if (pissMoon)
                {
                    Main.raining = true;
                    Main.maxRaining = 0.9f;
                    Main.rainTime = 7200.0;
                }

                if (Main.dayTime && pissMoon)
                {
                    Main.StopRain();
                    _pissMoonProp.SetValue(self, false);
                    _hasBeatenField?.SetValue(self, true);
                }

                if (Main.snowMoon || Main.pumpkinMoon)
                {
                    _pissMoonProp.SetValue(self, false);
                }

                if (pissMoon)
                {
                    Main.bloodMoon = false;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error("WorldOfPiss", $"PostUpdateTime_Replacement error: {ex.Message}");
            }
        }
    }
}