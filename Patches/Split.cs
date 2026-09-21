using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

namespace VariousModPatches
{
    public class Split : ModSystem
    {
        private ILHook _ilHookPrefixChance;
        private ILHook _ilHookModifyTooltips;

        private static readonly string[] ArmorPrefixMods =
        [
            "Avalon"
        ];

        public override void PostSetupContent()
        {
            if (!Instances.Config.PatchSplit) return;

            bool found = false;
            foreach (var modName in ArmorPrefixMods)
            {
                if (ModLoader.TryGetMod(modName, out _))
                {
                    found = true;
                    break;
                }
            }

            if (!found) return;

            var mod = VersionChecker.GetModChecked("Split", "Split");
            if (mod == null) return;

            if (!ApplyPrefixChanceHook(mod)) return;
            if (!ApplyModifyTooltipsHook(mod)) return;
        }

        public override void Unload()
        {
            _ilHookPrefixChance?.Dispose();
            _ilHookModifyTooltips?.Dispose();
        }

        private bool ApplyPrefixChanceHook(Mod mod)
        {
            var targetType = mod.Code.GetType("Split.Core.Items.SplitGlobalItem");
            if (targetType == null)
            {
                FileLogger.Warn("Split", "SplitGlobalItem type not found");
                return false;
            }

            var method = targetType.GetMethod("PrefixChance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("Split", "PrefixChance method not found");
                return false;
            }

            try
            {
                _ilHookPrefixChance = new ILHook(method, PrefixChance_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("Split", $"PrefixChance ILHook failed: {ex.Message}");
                return false;
            }
        }

        private bool ApplyModifyTooltipsHook(Mod mod)
        {
            var targetType = mod.Code.GetType("Split.Core.Items.SplitGlobalItem");
            if (targetType == null)
            {
                FileLogger.Warn("Split", "SplitGlobalItem type not found");
                return false;
            }

            var method = targetType.GetMethod("ModifyTooltips",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                FileLogger.Warn("Split", "ModifyTooltips method not found");
                return false;
            }

            try
            {
                _ilHookModifyTooltips = new ILHook(method, ModifyTooltips_IL);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("Split", $"ModifyTooltips ILHook failed: {ex.Message}");
                return false;
            }
        }

        private void PrefixChance_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, i => true))
            {
                cursor.Remove();
            }

            cursor.Emit(OpCodes.Ldarg_0); // this
            cursor.Emit(OpCodes.Ldarg_1); // item
            cursor.Emit(OpCodes.Ldarg_2); // pre
            cursor.Emit(OpCodes.Ldarg_3); // rand
            cursor.Emit(OpCodes.Call, typeof(GlobalItem).GetMethod("PrefixChance",
                BindingFlags.Public | BindingFlags.Instance));
            cursor.Emit(OpCodes.Ret);
        }

        private void ModifyTooltips_IL(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Find ArmorReforgeWarning
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdstr("ArmorReforgeWarning")))
                return;

            // Roll back to find ldloc + brfalse pair
            if (!cursor.TryGotoPrev(MoveType.Before,
                i => i.MatchBrfalse(out _)))
                return;

            // Check if previous instruction is ldloc
            if (!cursor.TryGotoPrev(MoveType.Before,
                i => i.MatchLdloc(out _)))
                return;

            // Save index of ldloc
            int ldlocIndex = cursor.Index;

            // Find brfalse after ldloc
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchBrfalse(out _)))
                return;

            var target = cursor.Instrs[cursor.Index].Operand;

            // Remove brfalse
            cursor.Remove();

            // Remove ldloc
            cursor.Goto(ldlocIndex, MoveType.Before);
            cursor.Remove();

            // Insert br
            cursor.Emit(OpCodes.Br, target);
        }
    }
}