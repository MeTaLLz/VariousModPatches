using Terraria;
using Terraria.ModLoader;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VariousModPatches;

public class TerrariaAmbienceMessage : ModSystem
{
    private ILHook _ilHook_OnEnterWorld;

    public override void Unload()
    {
        _ilHook_OnEnterWorld?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchTerrariaAmbience_Message,
            "TerrariaAmbience", "TerrariaAmbience_Message", PatchSide.Client, out var mod))
            return;

        ApplyOnEnterWorldHook(mod);
    }

    private void ApplyOnEnterWorldHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod,
            "TerrariaAmbience.Content.Players.AmbientPlayer", "TerrariaAmbience_Message");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "OnEnterWorld",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "TerrariaAmbience_Message");
        if (method == null) return;

        _ilHook_OnEnterWorld = Reflection.TryCreateILHook(method, OnEnterWorld_IL, "TerrariaAmbience_Message");
    }

    private static void OnEnterWorld_IL(ILContext il)
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