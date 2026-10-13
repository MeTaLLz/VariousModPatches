using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader;

namespace VariousModPatches;

public class InfernumMode : ModSystem
{
    private ILHook _ilHook_CreateMetaballs;
    private ILHook _ilHook_EctoplasmShot;
    private ILHook _ilHook_CirclingEctoplasm;

    public override void PostSetupContent()
    {
        if (!Netmode.IsSingleplayer) return;
        if (!Instances.Config.PatchInfernumMode) return;

        var mod = VersionChecker.GetModChecked("InfernumMode", "InfernumMode");
        if (mod == null) return;

        if (!ApplyCreateMetaballsHook(mod)) return;
        if (!ApplyEctoplasmShotHook(mod)) return;
        if (!ApplyCirclingEctoplasmHook(mod)) return;
    }

    public override void Unload()
    {
        _ilHook_CreateMetaballs?.Dispose();
        _ilHook_EctoplasmShot?.Dispose();
        _ilHook_CirclingEctoplasm?.Dispose();
    }

    private bool ApplyCreateMetaballsHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "InfernumMode.Utilities", "InfernumMode");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "CreateMetaballsFromTexture",
            BindingFlags.Public | BindingFlags.Static, "InfernumMode");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, CreateMetaballsFromTexture_IL,
            out _ilHook_CreateMetaballs, "InfernumMode");
    }

    private bool ApplyEctoplasmShotHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod,
            "InfernumMode.Content.BehaviorOverrides.BossAIs.Polterghast.EctoplasmShot",
            "InfernumMode");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "PreDraw",
            BindingFlags.Public | BindingFlags.Instance, "InfernumMode");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, EctoplasmShot_PreDraw_IL,
            out _ilHook_EctoplasmShot, "InfernumMode");
    }

    private bool ApplyCirclingEctoplasmHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod,
            "InfernumMode.Content.BehaviorOverrides.BossAIs.Polterghast.CirclingEctoplasm",
            "InfernumMode");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "DefaultDraw",
            BindingFlags.Public | BindingFlags.Instance, "InfernumMode");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, CirclingEctoplasm_DefaultDraw_IL,
            out _ilHook_CirclingEctoplasm, "InfernumMode");
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

    private void EctoplasmShot_PreDraw_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before,
            i => i.MatchCall("InfernumMode.Core.InfernumConfig", "get_Instance")))
            return;

        cursor.RemoveRange(2);
        cursor.Emit(OpCodes.Ldc_I4_1);
    }

    private void CirclingEctoplasm_DefaultDraw_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before,
            i => i.MatchLdcI4(1),
            i => i.MatchAdd(),
            i => i.MatchStloc(1)))
            return;

        cursor.Next.OpCode = OpCodes.Ldc_I4_2;
    }
}