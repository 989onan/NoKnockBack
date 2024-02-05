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
using System.Runtime.Remoting.Contexts;

namespace NoKnockBack
{
    public class NoKnockBack : ResoniteMod
    {
        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> maxSpeed = new ModConfigurationKey<float>("MaxSpeedChange", "The maximum speed in magnitude in one tick that can be applied till it cancels and sends an impulse with a tag \"knockbackcanceled\" to your user root.", () => 0);

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> allowGravityChange = new ModConfigurationKey<bool>("Allow Gravity changes", "wiether to allow gravity changes", () => true);

        public override string Author => "989onan";
        public override string Link => "https://github.com/989onan/NoKnockBack";
        public override string Name => "NoKnockBack";
        public override string Version => "1.0.1";

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


                if (__instance.Force.Evaluate(context).Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {
                    
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    __result = __instance.Character.Evaluate(context) != null;
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
                if (__instance.Impulse.Evaluate(context).Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {
                    
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    __result = __instance.Character.Evaluate(context) != null;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterForceField), "ShouldApplyForce", typeof(CharacterController))]
            private static bool ShouldApplyForcePatch(ref bool __result, CharacterForceField __instance, CharacterController character)
            {


                if (__instance.Force.Value.Magnitude > Config.GetValue(maxSpeed) && character.IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(__instance.World.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, null);
                    __result = false;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SetCharacterVelocity), "Do", typeof(FrooxEngineContext))]
            private static bool knockbackPatch4(ref bool __result, SetCharacterVelocity __instance, FrooxEngineContext context)
            {


                if (__instance.Velocity.Evaluate(context).Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    __result = __instance.Character.Evaluate(context) != null;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }


            [HarmonyPrefix]
            [HarmonyPatch(typeof(SetCharacterGravity), "Do", typeof(FrooxEngineContext))]
            private static bool knockbackPatch5(ref bool __result, SetCharacterGravity __instance, FrooxEngineContext context)
            {


                if (!Config.GetValue(allowGravityChange) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "gravitychangecanceled", true, context);
                    __result = __instance.Character.Evaluate(context) != null;
                    Msg("prevented gravity change");
                    return false;

                }
                return true;
            }
        }
    }
}