using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;

namespace VariousModPatches;

public class InfernumMode_Polterghast : ModSystem
{
    private ILHook _ilHook_DecideNewPosition;

    public override void Unload()
    {
        _ilHook_DecideNewPosition?.Dispose();
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchInfernumMode_Polterghast,
            "InfernumMode", "InfernumMode_Polterghast", PatchSide.Singleplayer, out var mod))
            return;

        ApplyPolterghastLegHook(mod);
    }

    private void ApplyPolterghastLegHook(Mod mod)
    {
        var targetType = Reflection.GetType(mod, 
            "InfernumMode.Content.BehaviorOverrides.BossAIs.Polterghast.PolterghastLeg", 
            "InfernumMode_Polterghast");
        if (targetType == null) return;

        var method = Reflection.GetMethod(targetType, "DecideNewPositionToStickTo",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, "InfernumMode_Polterghast");
        if (method == null) return;

        _ilHook_DecideNewPosition = Reflection.TryCreateILHook(method, DecideNewPosition_IL, "InfernumMode_Polterghast");
    }

    private static void DecideNewPosition_IL(ILContext il)
    {
        var cursor = new ILCursor(il)
        {
            Index = 0
        };

        // Replace ldc.i4 20000 (try count) with 800
        if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(20000)))
            cursor.Next.Operand = 800;

        // Replace ldc.r4 20000 (checkArea divisor) with 800
        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(20000f)))
            cursor.Next.Operand = 800f;

        // Replace ldc.i4 15000 (CanHitLine threshold) with 600
        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(15000)))
            cursor.Next.Operand = 600;

        // Replace ldc.i4 17500 (SolidTile threshold) with 700  
        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(17500)))
            cursor.Next.Operand = 700;
    }
}

/*
    The DecideNewPositionToStickTo method runs a loop of 20,000 random checks to find a valid tile for Polterghast's legs to attach to.
    This causes severe CPU spikes during boss dashes when legs frequently reposition.
    This patch reduces the loop count to 800 and proportionally lowers the CanHitLine and SolidTile thresholds.
*/