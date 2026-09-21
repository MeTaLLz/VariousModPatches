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

            if (!ApplyOnEnterWorldHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private bool ApplyOnEnterWorldHook(Mod mod)
        {
            var targetType = mod.Code.GetType("TerrariaAmbience.Content.Players.AmbientPlayer");
            if (targetType == null)
            {
                FileLogger.Warn("TerrariaAmbience_Message", "AmbientPlayer type not found");
                return false;
            }

            var method = targetType.GetMethod("OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("TerrariaAmbience_Message", "OnEnterWorld method not found");
                return false;
            }

            try
            {
                _ilHook = new ILHook(method, OnEnterWorld_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("TerrariaAmbience_Message", $"ILHook failed: {ex.Message}");
                return false;
            }
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