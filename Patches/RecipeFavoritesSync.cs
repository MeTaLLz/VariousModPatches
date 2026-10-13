using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VariousModPatches;

public class RecipeFavoritesSync : ModSystem
{
    private ILHook _ilHook;

    public override void PostSetupContent()
    {
        if (Netmode.IsServer) return;
        if (!Instances.Config.PatchRecipeFavoritesSync) return;

        var mod = VersionChecker.GetModChecked("RecipeFavoritesSync", "RecipeFavoritesSync");
        if (mod == null) return;

        if (!ApplyOnEnterWorldHook(mod)) return;
    }

    public override void Unload()
    {
        _ilHook?.Dispose();
    }

    private bool ApplyOnEnterWorldHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod,
            "RecipeFavoritesSync.Integration.RecipeBrowser.RecipeFavoritesSyncPlayer",
            "RecipeFavoritesSync");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "OnEnterWorld",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "RecipeFavoritesSync");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, OnEnterWorld_IL,
            out _ilHook, "RecipeFavoritesSync");
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
