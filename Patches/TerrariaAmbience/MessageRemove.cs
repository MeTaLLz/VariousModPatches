using Terraria;
using Terraria.ModLoader;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VariousModPatches
{
    public class TerrariaAmbienceMessage : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            if (NetmodeHelper.IsServer) return;
            if (!Instances.Config.PatchTerrariaAmbience_Message) return;

            var mod = VersionChecker.GetModChecked("TerrariaAmbience", "TerrariaAmbience_Message");
            if (mod == null) return;

            var targetType = mod.Code.GetType("TerrariaAmbience.Content.Players.AmbientPlayer");
            if (targetType == null)
            {
                FileLogger.Warn("TerrariaAmbience_Message", "AmbientPlayer type not found");
                return;
            }

            var method = targetType.GetMethod("OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                FileLogger.Warn("TerrariaAmbience_Message", "OnEnterWorld method not found");
                return;
            }

            try
            {
                _ilHook = new ILHook(method, OnEnterWorld_IL);
            }
            catch (Exception ex)
            {
                FileLogger.Error("TerrariaAmbience_Message", $"ILHook failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private void OnEnterWorld_IL(ILContext il)
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
}