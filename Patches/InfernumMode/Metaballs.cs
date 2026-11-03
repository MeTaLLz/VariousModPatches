using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader;

namespace VariousModPatches;

public class InfernumMode_Metaballs : ModSystem
{
    private ILHook _ilHook_CreateMetaballs;

    public override void PostSetupContent()
    {
        if (!Netmode.IsSingleplayer) return;
        if (!Instances.Config.PatchInfernumMode_Metaballs) return;

        var mod = VersionChecker.GetModChecked("InfernumMode", "InfernumMode_Metaballs");
        if (mod == null) return;

        if (!ApplyCreateMetaballsHook(mod)) return;
    }

    public override void Unload()
    {
        _ilHook_CreateMetaballs?.Dispose();
    }

    private bool ApplyCreateMetaballsHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "InfernumMode.Utilities", "InfernumMode_Metaballs");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "CreateMetaballsFromTexture",
            BindingFlags.Public | BindingFlags.Static, "InfernumMode_Metaballs");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, CreateMetaballsFromTexture_IL,
            out _ilHook_CreateMetaballs, "InfernumMode_Metaballs");
    }

    private void CreateMetaballsFromTexture_IL(ILContext il)
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