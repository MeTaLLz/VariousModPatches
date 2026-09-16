using Terraria.ModLoader;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VariousModPatches
{
    public class CalamityHuntFix : ModSystem
    {
        private ILHook _ilHook;
        private ILHook _unloadHook;
        private static bool _isUnloading = false;

        public override void PostSetupContent()
        {
            if (!NetmodeHelper.IsSingleplayer) return;
            if (!Instances.Config.PatchCalamityHunt) return;

            var mod = VersionChecker.GetModChecked("CalamityHunt", "CalamityHunt");
            if (mod == null) return;

            var unloadMethod = typeof(ModContent).GetMethod("UnloadModContent",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (unloadMethod != null)
            {
                _unloadHook = new ILHook(unloadMethod, (il) =>
                {
                    var cursor = new ILCursor(il);
                    cursor.Emit(OpCodes.Call, typeof(CalamityHuntFix).GetMethod("SetUnloadingFlag",
                        BindingFlags.NonPublic | BindingFlags.Static));
                });
            }
            else
            {
                FileLogger.Warn("CalamityHunt", "UnloadModContent method not found");
            }

            var targetType = mod.Code.GetType("CalamityHunt.Common.Systems.TileEdgeHighlight");
            if (targetType == null)
            {
                FileLogger.Warn("CalamityHunt", "TileEdgeHighlight type not found");
                return;
            }

            var method = targetType.GetMethod("CombineTileTargets",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                FileLogger.Warn("CalamityHunt", "CombineTileTargets method not found");
                return;
            }

            try
            {
                _ilHook = new ILHook(method, CombineTileTargets_IL);
            }
            catch (Exception ex)
            {
                FileLogger.Error("CalamityHunt", $"ILHook failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
            _unloadHook?.Dispose();
            _isUnloading = false;
        }

        private static void SetUnloadingFlag()
        {
            _isUnloading = true;
        }

        private void CombineTileTargets_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Inserting at the beginning of the method:
            // if (IsUnloading()) return;

            var continueLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Call, typeof(CalamityHuntFix).GetMethod("IsUnloading",
                BindingFlags.Public | BindingFlags.Static));
            cursor.Emit(OpCodes.Brfalse, continueLabel); // if false, continue
            cursor.Emit(OpCodes.Ret); // if true, return

            cursor.MarkLabel(continueLabel);
        }

        public static bool IsUnloading()
        {
            return _isUnloading;
        }
    }
}

/*
    One of the problem is:
    System.NullReferenceException: Object reference not set to an instance of an object.
    at WeaponOutLite.Common.Players.WeaponOutPlayerRenderer.get_IsShowingHeldItem() in WeaponOutLite\Common\Players\WeaponOutPlayerRenderer.cs:line 52
    at WeaponOutLite.Common.WeaponOutLayerRenderer.CanDrawBaseDrawData(PlayerDrawSet drawInfo, Player& drawPlayer, WeaponOutPlayerRenderer& modPlayer, IDrawItemPose& holdStyle, Item& heldItem, Texture2D& itemTexture) in WeaponOutLite\Common\WeaponOutLayerRenderer.cs:line 387
    at WeaponOutLite.Common.WeaponOutItemHeldLayer.GetDefaultVisibility(PlayerDrawSet drawInfo) in WeaponOutLite\Common\WeaponOutDrawLayers.cs:line 46
    at Terraria.ModLoader.PlayerDrawLayer.ResetVisibility(PlayerDrawSet drawInfo) in tModLoader\Terraria\ModLoader\PlayerDrawLayer.cs:line 78
    at Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerInternal(Camera camera, Player drawPlayer, Vector2 position, Single rotation, Vector2 rotationOrigin, Single shadow, Single alpha, Single scale, Boolean headOnly) in tModLoader\Terraria\Graphics\Renderers\LegacyPlayerRenderer.cs:line 150
    at Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayer(Camera camera, Player drawPlayer, Vector2 position, Single rotation, Vector2 rotationOrigin, Single shadow, Single scale) in tModLoader\Terraria\Graphics\Renderers\LegacyPlayerRenderer.cs:line 122
    at CalamityHunt.Common.Systems.TileEdgeHighlight.CombineTileTargets(orig_UpdateAtmosphereTransparencyToSkyColor orig) in CalamityHunt\Common\Systems\TileEdgeHighlight.cs:line 80

    CalamityHunt calls Main.PlayerRenderer.DrawPlayer during mod unloading,
    which triggers draw layers from mods that are already partially unloaded (e.g. WeaponOutLite, SOTS).
    This causes NullReferenceException or ObjectDisposedException.

    Placing a hook on UnloadModContent to set a flag when unloading starts, and patching CombineTileTargets
    to check this flag and return early.

    sortBefore = CalamityHunt was added to build.txt to control load order, 
    bcs without it this mod would be unloaded before CalamityHunt.
*/