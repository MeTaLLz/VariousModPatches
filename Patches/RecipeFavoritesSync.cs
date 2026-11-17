using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VariousModPatches;

public class RecipeFavoritesSync : ModSystem
{
    private ILHook _ilHook_OnEnterWorld;

    public override void Unload()
    {
        _ilHook_OnEnterWorld?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchRecipeFavoritesSync,
            "RecipeFavoritesSync", "RecipeFavoritesSync", PatchSide.Client, out var mod))
            return;

        ApplyOnEnterWorldHook(mod);
    }

    private void ApplyOnEnterWorldHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod,
            "RecipeFavoritesSync.Integration.RecipeBrowser.RecipeFavoritesSyncPlayer",
            "RecipeFavoritesSync");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "OnEnterWorld",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "RecipeFavoritesSync");
        if (method == null) return;

        _ilHook_OnEnterWorld = Reflection.TryCreateILHook(method, OnEnterWorld_IL, "RecipeFavoritesSync");
    }

    private static void OnEnterWorld_IL(ILContext il)
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