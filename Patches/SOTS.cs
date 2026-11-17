using Terraria;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace VariousModPatches;

public class SOTS : ModSystem
{
    private ILHook _ilHook_OnEnterWorld;

    public override void Unload()
    {
        _ilHook_OnEnterWorld?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchSOTS,
            "SOTS", "SOTS", PatchSide.Client, out var mod))
            return;

        ApplyOnEnterWorldHook(mod);
    }

    private void ApplyOnEnterWorldHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "SOTS.SOTSPlayer", "SOTS");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "OnEnterWorld",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "SOTS");
        if (method == null) return;

        _ilHook_OnEnterWorld = Reflection.TryCreateILHook(method, OnEnterWorld_IL, "SOTS");
    }

    private static void OnEnterWorld_IL(ILContext il)
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