using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System;
using FrooxEngine.FinalIK;
using System.Collections.Generic;

namespace ActualSearchBlock
{
    public class ActualSearchBlock : ResoniteMod
    {
        public override string Author => "989onan";
        public override string Link => "Nope";
        public override string Name => "ActualSearchBlock";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            harmony.PatchAll();
        }

        [HarmonyPatch]
        private class PatchSearchChildByName
        {

            public static bool MatchSlot(Slot slot, string name, bool matchSubstring, bool ignoreCase)
            {
                if (slot.Name == null)
                {
                    return name == null;
                }
                if (name == null)
                {
                    return false;
                }
                if (slot.Name == "")
                {
                    return name == "";
                }
                StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (matchSubstring)
                {
                    return slot.Name.IndexOf(name, comparisonType) >= 0;
                }
                return string.Compare(slot.Name, name, comparisonType) == 0;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Slot),"FindChild", typeof(string), typeof(bool), typeof(bool), typeof(int))]
            private static bool FindChildPrefix(Slot __result, Slot __instance, string name, bool matchSubstring, bool ignoreCase, int maxDepth = -1)
            {
                Slot result = __instance.FindChild((Slot s) => MatchSlot(s, name, matchSubstring, ignoreCase), maxDepth);
                if (result == null) {
                    return true;
                }
                VRIK avatarroot = result.GetComponentInParents<VRIK>();
                if (avatarroot == null)
                {
                    return true;
                }
                if (avatarroot.Slot.GetComponent<SearchBlock>() != null)
                {
                    if(avatarroot.Slot.GetComponent<SearchBlock>().Enabled == true)
                    {
                        __result = null;
                        return false;
                    }
                    else
                    {
                        return true;

                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}