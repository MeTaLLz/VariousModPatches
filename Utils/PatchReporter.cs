using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace VariousModPatches;

public static class PatchReporter
{
    private static readonly HashSet<string> FailedPatches = new();

    public static void RegisterFailure(string patchName)
    {
        FailedPatches.Add(patchName);
    }

    public static void Report()
    {
        if (FailedPatches.Count == 0) return;
        if (Netmode.IsServer) return;

        string list = string.Join(", ", FailedPatches);
        string msg = $"[VMP] Some patches failed. Check client.log.\nFailed: {list}";
        
        Main.NewText(msg, 255, 0, 0);
        FailedPatches.Clear();
    }
}

public class PatchReportSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        PatchReporter.Report();
    }
}