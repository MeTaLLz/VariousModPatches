using Terraria;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

namespace VariousModPatches
{
    public class SOTS : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            if (NetmodeHelper.IsServer) return;
            if (!Instances.Config.PatchSOTS) return;

            var mod = VersionChecker.GetModChecked("SOTS", "SOTS");
            if (mod == null) return;

            if (!ApplyOnEnterWorldHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private bool ApplyOnEnterWorldHook(Mod mod)
        {
            var targetType = mod.Code.GetType("SOTS.SOTSPlayer");
            if (targetType == null)
            {
                FileLogger.Warn("SOTS", "SOTSPlayer type not found");
                return false;
            }

            var method = targetType.GetMethod("OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("SOTS", "OnEnterWorld method not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(method, OnEnterWorld_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("SOTS", $"ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void OnEnterWorld_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                i => i.MatchCall(typeof(Main), "NewText")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Remove();
                cursor.Emit(OpCodes.Nop);
            }
        }
    }
}