using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VariousModPatches;

public class MagicStorage : ModSystem
{
    private ILHook _ilHook;

    public override void PostSetupContent()
    {
        if (Netmode.IsServer) return;
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
        var targetType = Reflection.GetType(mod, "MagicStorage.StoragePlayer", "MagicStorage");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "PreUpdate",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "MagicStorage");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, PreUpdate_IL,
            out _ilHook, "MagicStorage");
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
