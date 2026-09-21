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

namespace VariousModPatches
{
    public class TerrariaAmbienceAudio : ModSystem
    {
        private ILHook _ilHook;
        private static MethodInfo _findReplacementTrackMethod;

        public override void PostSetupContent()
        {
            if (!NetmodeHelper.IsSingleplayer) return;
            if (!Instances.Config.PatchTerrariaAmbience_Audio) return;

            var mod = VersionChecker.GetModChecked("TerrariaAmbience", "TerrariaAmbience_Audio");
            if (mod == null) return;

            if (!ApplyLoadFromSourcesHook()) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
            _findReplacementTrackMethod = null;
        }

        private bool ApplyLoadFromSourcesHook()
        {
            var audioMethod = typeof(LegacyAudioSystem).GetMethod("LoadFromSources",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (audioMethod == null)
            {
                FileLogger.Warn("TerrariaAmbience_Audio", "LoadFromSources method not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(audioMethod, LoadFromSources_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("TerrariaAmbience_Audio", $"ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void LoadFromSources_IL(ILContext il)
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
                FileLogger.Error("TerrariaAmbience_Audio", $"LoadFromSourcesReplacement error: {ex.Message}");
            }
        }

        private static IAudioTrack FindReplacementTrack(LegacyAudioSystem system, List<IContentSource> sources, string assetPath)
        {
            _findReplacementTrackMethod ??= typeof(LegacyAudioSystem).GetMethod("FindReplacementTrack",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (IAudioTrack)_findReplacementTrackMethod.Invoke(system, new object[] { sources, assetPath });
        }
    }
}

/* 
    The problem is:
    System.NullReferenceException: Object reference not set to an instance of an object.
    at Terraria.Audio.LegacyAudioSystem.LoadFromSources() in tModLoader\Terraria\Audio\LegacyAudioSystem.cs:line 37

    The issue occurs because AudioTracks[i] can be null,
    and the original code calls Stop() and Dispose() without checking for null. Idk why 
    TerrariaAmbience creates null entries.

    Replacing the entire LoadFromSources method with a safe version
    that checks for null before calling Stop() and Dispose().
*/