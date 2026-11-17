using Terraria.ModLoader;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using Terraria.Audio;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content.Sources;

namespace VariousModPatches;

public class TerrariaAmbienceAudio : ModSystem
{
    private ILHook _ilHook_LoadFromSources;
    private static MethodInfo _findReplacementTrackMethod;

    public override void Unload()
    {
        _ilHook_LoadFromSources?.Dispose();
        _findReplacementTrackMethod = null;
    }

    public override void PostSetupContent()
    {
        if (!PatchApplier.ShouldApplyPatch(VMPConfig.Instance.PatchTerrariaAmbience_Audio,
            "TerrariaAmbience", "TerrariaAmbience_Audio", PatchSide.Singleplayer, out _))
            return;

        ApplyLoadFromSourcesHook();
    }

    private void ApplyLoadFromSourcesHook()
    {
        var audioMethod = Reflection.GetMethod(typeof(LegacyAudioSystem), "LoadFromSources",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "TerrariaAmbience_Audio");
        if (audioMethod == null) return;

        _ilHook_LoadFromSources = Reflection.TryCreateILHook(audioMethod, LoadFromSources_IL, "TerrariaAmbience_Audio");
    }

    private static void LoadFromSources_IL(ILContext il)
    {   
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.Before, i => true))
        {
            cursor.Remove();
        }

        cursor.Emit(OpCodes.Ldarg_0); // this
        cursor.Emit(OpCodes.Call, typeof(TerrariaAmbienceAudio).GetMethod("LoadFromSourcesReplacement",
            BindingFlags.NonPublic | BindingFlags.Static));
        cursor.Emit(OpCodes.Ret);
    }

    private static void LoadFromSourcesReplacement(LegacyAudioSystem system)
    {
        try
        {
            List<IContentSource> fileSources = system.FileSources;
            for (int i = 0; i < system.AudioTracks.Length; i++)
            {
                if (system.TrackNamesByIndex.TryGetValue(i, out var value))
                {
                    string assetPath = "Music" + Path.DirectorySeparatorChar + value;
                    IAudioTrack audioTrack = system.DefaultTrackByIndex[i];
                    IAudioTrack audioTrack2 = audioTrack;
                    IAudioTrack audioTrack3 = FindReplacementTrack(system, fileSources, assetPath);
                    if (audioTrack3 != null)
                    {
                        audioTrack2 = audioTrack3;
                    }

                    // Added "&& system.AudioTracks[i] != null"
                    if (system.AudioTracks[i] != audioTrack2 && system.AudioTracks[i] != null)
                    {
                        system.AudioTracks[i].Stop(AudioStopOptions.Immediate);
                    }
                    
                    // Added "&& system.AudioTracks[i] != null"
                    if (system.AudioTracks[i] != audioTrack && system.AudioTracks[i] != null)
                    {
                        system.AudioTracks[i].Dispose();
                    }

                    system.AudioTracks[i] = audioTrack2;
                }
            }
        }
        catch (Exception ex)
        {
            ModLog.Error("TerrariaAmbience_Audio", $"LoadFromSourcesReplacement error:\n{ex}");
        }
    }

    private static IAudioTrack FindReplacementTrack(LegacyAudioSystem system, List<IContentSource> sources, string assetPath)
    {
        _findReplacementTrackMethod ??= typeof(LegacyAudioSystem).GetMethod("FindReplacementTrack",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (IAudioTrack)_findReplacementTrackMethod.Invoke(system, new object[] { sources, assetPath });
    }
}

/* 
    System.NullReferenceException: Object reference not set to an instance of an object.
    at Terraria.Audio.LegacyAudioSystem.LoadFromSources() in tModLoader\Terraria\Audio\LegacyAudioSystem.cs:line 37

    The issue occurs because AudioTracks[i] can be null,
    and the original code calls Stop() and Dispose() without checking for null. Idk why 
    TerrariaAmbience creates null entries.

    Replacing the entire LoadFromSources method with a safe version
    that checks for null before calling Stop() and Dispose().
*/