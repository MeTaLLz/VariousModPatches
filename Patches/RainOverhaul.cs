using Terraria.ModLoader;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace VariousModPatches
{
    public class RainOverhaul : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            if (!Instances.Config.PatchRainOverhaul) return;

            var mod = VersionChecker.GetModChecked("RainOverhaul", "RainOverhaul");
            if (mod == null) return;

            if (!ApplyNPCRainHandlerHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private bool ApplyNPCRainHandlerHook(Mod mod)
        {
            var targetType = mod.Code.GetType("RainOverhaul.Source.RainSystem.NPCRainHandler");
            if (targetType == null)
            {
                FileLogger.Warn("RainOverhaul", "NPCRainHandler type not found");
                return false;
            }

            var method = targetType.GetMethod("AI",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("RainOverhaul", "AI method not found");
                return false;
            }

            var configType = mod.Code.GetType("RainOverhaul.Source.Configs.ConfigServer");
            if (configType == null)
            {
                FileLogger.Warn("RainOverhaul", "ConfigServer type not found");
                return false; 
            }

            var field = configType.GetField("isRainWorldMode",
                BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                FileLogger.Warn("RainOverhaul", "isRainWorldMode field not found");
                return false;
            }

            var instanceField = configType.GetField("Instance",
                BindingFlags.Public | BindingFlags.Static);
            if (instanceField == null)
            {
                FileLogger.Warn("RainOverhaul", "Instance field not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(method, NPCRainHandler_AI_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("RainOverhaul", $"ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void NPCRainHandler_AI_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Inserting at the beginning of the method:
            // if (!ConfigServer.Instance.isRainWorldMode) return;

            var configType = ModLoader.GetMod("RainOverhaul").Code.GetType("RainOverhaul.Source.Configs.ConfigServer");
            if (configType == null)
            {
                FileLogger.Warn("RainOverhaul", "ConfigServer type not found in IL");
                return;
            }

            var field = configType.GetField("isRainWorldMode", 
                BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                FileLogger.Warn("RainOverhaul", "isRainWorldMode field not found in IL");
                return;
            }

            var instanceField = configType.GetField("Instance", 
                BindingFlags.Public | BindingFlags.Static);
            if (instanceField == null)
            {
                FileLogger.Warn("RainOverhaul", "Instance field not found in IL");
                return;
            }

            var continueLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Ldsfld, instanceField); // load static field ConfigServer.Instance
            cursor.Emit(OpCodes.Ldfld, field); // load isRainWorldMode from ConfigServer.Instance
            cursor.Emit(OpCodes.Brtrue, continueLabel); // if true, jump to continueLabel
            cursor.Emit(OpCodes.Ret); // if false, return from method
            cursor.MarkLabel(continueLabel); // mark continueLabel for execution when true
        }
    }
}

/*
    The problem is:
    System.IndexOutOfRangeException: Index was outside the bounds of the array.
    at RainOverhaul.Source.RainSystem.NPCRainHandler.AI(NPC npc) in RainOverhaul\Source\RainSystem\NPCRainHandler.cs:line 29

    If RainWorld is not enabled, this method is unnecessary and causes 
    IndexOutOfRangeException with some NPCs (e.g. Empress of Light from WoTE).

    Placing a hook on NPCRainHandler.AI to inject a check at the method's entry point,
    skipping execution when RainWorld mode is disabled.
*/