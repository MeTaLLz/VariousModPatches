using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

namespace VariousModPatches;

public class KZ_VisualMod : ModSystem
{
    private ILHook _ilHook_Broken;
    private ILHook _ilHook_Wrapper;

    public override void Load()
    {
        if (!Netmode.IsSingleplayer) return;
        if (!Instances.Config.PatchKZVisualMod) return;

        var mod = VersionChecker.GetModChecked("KZ_VisualMod", "KZ_VisualMod");
        if (mod == null) return;

        if (!ApplyBrokenMethodHook(mod)) return;
        if (!ApplyWrapperMethodHook(mod)) return;
    }

    public override void Unload()
    {
        _ilHook_Broken?.Dispose();
        _ilHook_Wrapper?.Dispose();
    }

    private bool ApplyBrokenMethodHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "KZ_VisualMod.KZ_VisualMod", "KZ_VisualMod");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "_KZ_On_Lighting_NextLightMode",
            BindingFlags.NonPublic | BindingFlags.Static, "KZ_VisualMod");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, ClearMethod_IL,
            out _ilHook_Broken, "KZ_VisualMod");
    }

    private bool ApplyWrapperMethodHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, "KZ_VisualMod.KZ_VisualMod", "KZ_VisualMod");
        if (targetType == null) return false;

        var method = Reflection.GetMethod(targetType, "KZ_On_Lighting_NextLightMode",
            BindingFlags.NonPublic | BindingFlags.Instance, "KZ_VisualMod");
        if (method == null) return false;

        return Reflection.TryCreateILHook(method, CallOrig_IL,
            out _ilHook_Wrapper, "KZ_VisualMod");
    }

    private void ClearMethod_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.Before, i => true))
            cursor.Remove();

        il.Body.ExceptionHandlers.Clear();

        cursor.Emit(OpCodes.Ret);
    }

    private void CallOrig_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.Before, i => true))
            cursor.Remove();

        cursor.Emit(OpCodes.Ldarg_0); // this
        cursor.Emit(OpCodes.Ldarg_1); // orig
        cursor.EmitDelegate<Action<object, object>>((self, orig) =>
        {
            var invoke = orig.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            invoke?.Invoke(orig, null);
        });

        cursor.Emit(OpCodes.Ret);
    }
}

/*
    The problem is:
    KZ_VisualMod's _KZ_On_Lighting_NextLightMode() breaks lighting after mods reload.
    It increments Lighting.Mode and then forces it to Color (3), but if the mode was already Color,
    the increment goes out of bounds and resets to White (0).

    Fixing two methods:
    1. _KZ_On_Lighting_NextLightMode — clearing entirely, so it won't break lighting.
    2. KZ_On_Lighting_NextLightMode — patching to call orig, so the lighting button will work correct.

    Added KZ_VisualMod to sortBefore to build.txt to control load order, 
    bcs without it this mod would be loaded after KZ_VisualMod.
*/