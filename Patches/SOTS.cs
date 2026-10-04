using Terraria;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace VariousModPatches
{
    public class SOTS : ModSystem
    {
        private ILHook _ilHook;

        public override void PostSetupContent()
        {
            if (Netmode.IsServer) return;
            if (!Instances.Config.PatchSOTS) return;

            var mod = VersionChecker.GetModChecked("SOTS", "SOTS");
            if (mod == null) return;

            if (!ApplyOnEnterWorldHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
        }

        private bool ApplyOnEnterWorldHook(Mod mod)
        {
            var targetType = Reflection.GetType(mod, "SOTS.SOTSPlayer", "SOTS");
            if (targetType == null) return false;

            var method = Reflection.GetMethod(targetType, "OnEnterWorld",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, "SOTS");
            if (method == null) return false;

            return Reflection.TryCreateILHook(method, OnEnterWorld_IL,
                out _ilHook, "SOTS");
        }

        private void OnEnterWorld_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                i => i.MatchCall(typeof(Main), "NewText")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Remove();
                cursor.Emit(OpCodes.Nop);
            }
        }
    }
}