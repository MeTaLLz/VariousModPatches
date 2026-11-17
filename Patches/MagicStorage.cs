using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VariousModPatches;

public class MagicStorage : ModSystem
{
    private ILHook _ilHook_PreUpdate;

    public override void Unload()
    {
        _ilHook_PreUpdate?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchMagicStorage,
            "MagicStorage", "MagicStorage", PatchSide.Client, out var mod))
            return;

        ApplyPreUpdateHook(mod);
    }

    private void ApplyPreUpdateHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "MagicStorage.StoragePlayer", "MagicStorage");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "PreUpdate",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "MagicStorage");
        if (method == null) return;

        _ilHook_PreUpdate = Reflection.TryCreateILHook(method, PreUpdate_IL, "MagicStorage");
    }

    private static void PreUpdate_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.Before,
            i => i.MatchCall(typeof(Main), "NewTextMultiline")))
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