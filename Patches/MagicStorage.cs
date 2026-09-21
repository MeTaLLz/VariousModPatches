using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace VariousModPatches
{
    public class MagicStorage : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            if (NetmodeHelper.IsServer) return;
            if (!Instances.Config.PatchMagicStorage) return;

            var mod = VersionChecker.GetModChecked("MagicStorage", "MagicStorage");
            if (mod == null) return;

            if (!ApplyPreUpdateHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private bool ApplyPreUpdateHook(Mod mod)
        {
            var targetType = mod.Code.GetType("MagicStorage.StoragePlayer");
            if (targetType == null)
            {
                FileLogger.Warn("MagicStorage", "StoragePlayer type not found");
                return false;
            }

            var method = targetType.GetMethod("PreUpdate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("MagicStorage", "PreUpdate method not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(method, PreUpdate_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("MagicStorage", $"ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void PreUpdate_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                i => i.MatchCall(typeof(Main), "NewTextMultiline")))
            {
                cursor.Emit(OpCodes.Pop); // remove parameters
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Remove(); // remove original call
                cursor.Emit(OpCodes.Nop); // insert no-op
            }
        }
    }
}