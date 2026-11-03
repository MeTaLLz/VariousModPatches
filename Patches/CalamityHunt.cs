using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VariousModPatches;

public class CalamityHunt : ModSystem
{
    private ILHook _ilHook_UnloadModContent;
    private static ILHook _ilHook_CombineTileTargets;
    private static bool _isUnloading = false;
    private static Mod _calamityHuntMod;

    public override void PostSetupContent()
    {
        if (!Netmode.IsSingleplayer) return;
        if (!Instances.Config.PatchCalamityHunt) return;

        _calamityHuntMod = VersionChecker.GetModChecked("CalamityHunt", "CalamityHunt");
        if (_calamityHuntMod == null) return;

        if (!ApplyUnloadHook()) return;
    }

    public override void Unload()
    {
        _ilHook_UnloadModContent?.Dispose();
        _ilHook_CombineTileTargets?.Dispose();
        _isUnloading = false;
        _calamityHuntMod = null;
    }

    private bool ApplyUnloadHook()
    {
        var method = Reflection.GetMethod(typeof(ModContent), "UnloadModContent",
            BindingFlags.NonPublic | BindingFlags.Static, "CalamityHunt");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, UnloadModContent_IL,
            out _ilHook_UnloadModContent, "CalamityHunt");
    }

    private void UnloadModContent_IL(ILContext il)
    {
        var cursor = new ILCursor(il);
        
        cursor.Emit(OpCodes.Call, typeof(CalamityHunt).GetMethod(nameof(OnUnloadStarted),
            BindingFlags.NonPublic | BindingFlags.Static));
    }

    private static void OnUnloadStarted()
    {
        _isUnloading = true;
        
        if (_ilHook_CombineTileTargets == null && _calamityHuntMod != null)
        {
            var targetType = Reflection.GetType(_calamityHuntMod, "CalamityHunt.Common.Systems.TileEdgeHighlight", "CalamityHunt");
            if (targetType != null)
            {
                var method = Reflection.GetMethod(targetType, "CombineTileTargets",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "CalamityHunt");
                if (method != null)
                {
                    Reflection.TryCreateILHook(method, CombineTileTargets_IL, out _ilHook_CombineTileTargets, "CalamityHunt");
                }
            }
        }
    }

    private static void CombineTileTargets_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        var continueLabel = cursor.DefineLabel();

        cursor.Emit(OpCodes.Call, typeof(CalamityHunt).GetMethod("IsUnloading",
            BindingFlags.Public | BindingFlags.Static));
        cursor.Emit(OpCodes.Brfalse, continueLabel);
        cursor.Emit(OpCodes.Ret);

        cursor.MarkLabel(continueLabel);
    }

    public static bool IsUnloading()
    {
        return _isUnloading;
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

    sortBefore = CalamityHunt was added to build.txt to control load order, 
    bcs without this mod it would be unloaded before CalamityHunt.
*/