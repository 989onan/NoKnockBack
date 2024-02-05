using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System;
using FrooxEngine.FinalIK;
using System.Collections.Generic;
using Elements.Core;
using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Physics;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using static FrooxEngine.SessionControlDialog;

namespace NoKnockBack
{
    public class NoKnockBack : ResoniteMod
    {
        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> maxSpeed = new ModConfigurationKey<float>("MaxSpeedChange", "The maximum speed in magnitude in one tick that can be applied till it cancels and sends an impulse with a tag \"knockbackcanceled\" to your user root.", () => 0 );

        public override string Author => "989onan";
        public override string Link => "https://github.com/989onan/NoKnockBack";
        public override string Name => "NoKnockBack";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        }

        [HarmonyPatch]
        private class PatchKnockbackNode
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ApplyCharacterForce), "Do", typeof(FrooxEngineContext))]
            private static bool knockbackPatch(ref bool __result, ApplyCharacterForce __instance, FrooxEngineContext context)
            {


                if (__instance.Force.Evaluate(context).Magnitude > Config.GetValue(maxSpeed))
                {
                    
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, );
                    __result = true;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ApplyCharacterImpulse), "Do", typeof(FrooxEngineContext))]
            private static bool knockbackPatch2(ref bool __result, ApplyCharacterImpulse __instance, FrooxEngineContext context)
            {
                if (__instance.Impulse.Evaluate(context).Magnitude > Config.GetValue(maxSpeed))
                {
                    
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    __result = true;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }
        }
    }
}