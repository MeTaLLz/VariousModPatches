using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader;

namespace VariousModPatches;

public class InfernumMode_Metaballs : ModSystem
{
    private ILHook _ilHook_CreateMetaballs;
    
    public override void Unload()
    {
        _ilHook_CreateMetaballs?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchInfernumMode_Metaballs,
            "InfernumMode", "InfernumMode_Metaballs", PatchSide.Singleplayer, out var mod))
            return;

        ApplyCreateMetaballsHook(mod);
    }

    private void ApplyCreateMetaballsHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "InfernumMode.Utilities", "InfernumMode_Metaballs");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "CreateMetaballsFromTexture",
            BindingFlags.Public | BindingFlags.Static, "InfernumMode_Metaballs");
        if (method == null) return;

        _ilHook_CreateMetaballs = Reflection.TryCreateILHook(method, CreateMetaballsFromTexture_IL, "InfernumMode_Metaballs");
    }

    private static void CreateMetaballsFromTexture_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before,
            i => i.MatchCall("InfernumMode.Core.InfernumConfig", "get_Instance")))
            return;

        cursor.RemoveRange(3);
        cursor.Emit(OpCodes.Ldc_I4_1);
    }
}

/*
    CreateMetaballsFromTexture reads every pixel of a texture via texture.GetData to spawn metaball particles.
    This is extremely expensive for the CPU and GPU.
    This patch forces the mod to always use the fast, randomized particle generation path, skipping the pixel-reading entirely.
*/