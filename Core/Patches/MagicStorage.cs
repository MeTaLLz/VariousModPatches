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
            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (!config.PatchMagicStorage) return;

            var mod = VersionHelper.GetModAndWarn("MagicStorage", "MagicStorage");
            if (mod == null) return; 

            var targetType = mod.Code.GetType("MagicStorage.StoragePlayer");
            if (targetType == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (MagicStorage) StoragePlayer type not found");
                return;
            }

            var method = targetType.GetMethod("PreUpdate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (MagicStorage) PreUpdate method not found");
                return;
            }

            try
            {
                _ilHook = new ILHook(method, PreUpdate_IL);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"[VariousModPatches] (MagicStorage) ILHook failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
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