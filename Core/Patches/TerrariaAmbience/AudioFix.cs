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

        public override void PostSetupContent()
        {
            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (!config.PatchTerrariaAmbience_Audio) return;

            var mod = VersionHelper.GetModAndWarn("TerrariaAmbience", "TerrariaAmbience_Audio");
            if (mod == null) return;

            var audioMethod = typeof(LegacyAudioSystem).GetMethod("LoadFromSources",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (audioMethod == null)
            {
                Mod.Logger.Warn("[VariousModPatches] (TerrariaAmbience_Audio) LoadFromSources method not found");
                return;
            }

            try
            {
                _ilHook = new ILHook(audioMethod, LoadFromSources_IL);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"[VariousModPatches] (TerrariaAmbience_Audio) ILHook failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private void LoadFromSources_IL(ILContext il)
        {   
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, i => true))
            {
                cursor.Remove();
            }

            cursor.Index = 0;
            cursor.Emit(OpCodes.Ldarg_0); // load first argument (this) onto the stack
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
                ModContent.GetInstance<VariousModPatches>().Logger.Error(
                    $"[VariousModPatches] (TerrariaAmbience_Audio) LoadFromSourcesReplacement error: {ex.Message}");
            }
        }

        private static IAudioTrack FindReplacementTrack(LegacyAudioSystem system, List<IContentSource> sources, string assetPath)
        {
            var method = typeof(LegacyAudioSystem).GetMethod("FindReplacementTrack",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (IAudioTrack)method.Invoke(system, [sources, assetPath]);
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