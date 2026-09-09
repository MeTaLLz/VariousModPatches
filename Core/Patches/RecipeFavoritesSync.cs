using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace VariousModPatches
{
    public class RecipeFavoritesSync : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (!config.PatchRecipeFavoritesSync) return;

            var mod = VersionHelper.GetModAndWarn("RecipeFavoritesSync", "RecipeFavoritesSync");
            if (mod == null) return; 

            var targetType = mod.Code.GetType("RecipeFavoritesSync.Integration.RecipeBrowser.RecipeFavoritesSyncPlayer");
            if (targetType == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (RecipeFavoritesSync) RecipeFavoritesSyncPlayer type not found");
                return;
            }

            var method = targetType.GetMethod("OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (RecipeFavoritesSync) OnEnterWorld method not found");
                return;
            }

            try
            {
                _ilHook = new ILHook(method, OnEnterWorld_IL);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"[VariousModPatches] (RecipeFavoritesSync) ILHook failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private void OnEnterWorld_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                i => i.MatchCall(typeof(Main), "NewText")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Remove();
                cursor.Emit(OpCodes.Nop);
            }
        }
    }
}