using Terraria;
using Terraria.ModLoader;
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
            if (Netmode.IsServer) return;
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
            var targetType = Reflection.GetType(mod,
                "TerrariaAmbience.Content.Players.AmbientPlayer", "TerrariaAmbience_Message");
            if (targetType == null) return false;

            var method = Reflection.GetMethod(targetType, "OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "TerrariaAmbience_Message");
            if (method == null) return false;

            return Reflection.TryCreateILHook(method, OnEnterWorld_IL,
                out _ilHook, "TerrariaAmbience_Message");
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