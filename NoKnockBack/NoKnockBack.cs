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
using ProtoFlux.Core;

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
        public override string Version => "1.2.0";

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
                CharacterController characterController = __instance.Character.Evaluate(context);
                if (characterController == null)
                {
                    return false;
                }

                bool num = __instance.IgnoreMass.Evaluate(context, defaultValue: false);
                float3 v = __instance.Force.Evaluate(context);
                float3 v2 = v * context.World.Time.Delta;
                if (!num)
                {
                    v2 /= characterController.ActualMass;
                }

                v = characterController.LinearVelocity;
                float3 finalvelocity = v + v2;

                if (finalvelocity.Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, finalvelocity, context);
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
                CharacterController characterController = __instance.Character.Evaluate(context);
                if (characterController == null)
                {
                    return false;
                }

                bool num = __instance.IgnoreMass.Evaluate(context, defaultValue: false);
                float3 v = __instance.Impulse.Evaluate(context);
                if (!num)
                {
                    v /= characterController.ActualMass;
                }

                float3 a = characterController.LinearVelocity;
                float3 finalvelocity = a + v;
                
                if (finalvelocity.Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {
                    
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, finalvelocity, context);
                    __result = __instance.Character.Evaluate(context) != null;
                    Msg("prevented knockback");
                    return false;

                }
                Msg("Finished code for knockback, not powerful enough.");
                return true;
            }


            [HarmonyReversePatch]
            [HarmonyPatch(typeof(CharacterForceField), "GetForce", typeof(CharacterController))]
            public static float3 GetForce(CharacterForceField __instance, CharacterController characterController)
            {
                throw new NotImplementedException("Stub GetForce");
            }

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(CharacterForceField), "ShouldApplyForce", typeof(CharacterController))]
            public static bool ShouldApplyForce(CharacterForceField __instance, CharacterController characterController)
            {
                throw new NotImplementedException("Stub GetForce");
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterForceField), "OnContactStart", typeof(ICollider), typeof(ICollider))]
            private static bool OnContactStartPatch(CharacterForceField __instance, ICollider collider, ICollider other)
            {
                float3 finalvelocity = float3.Zero;
                CharacterController characterController = other.ColliderOwner as CharacterController;
                if (characterController != null && ShouldApplyForce(__instance, characterController))
                {
                    float3 a = GetForce(__instance, characterController);
                    switch (__instance.ForceMode.Value)
                    {
                        case CharacterForceField.Mode.Impulse:
                            {
                                float3 a2 = characterController.LinearVelocity;
                                finalvelocity = a2 + a;
                                break;
                            }
                        case CharacterForceField.Mode.SetVelocity:
                            finalvelocity = a;
                            break;
                        case CharacterForceField.Mode.SetComponent:
                            {
                                float3 a2 = characterController.LinearVelocity;
                                float3 b = a.Normalized;
                                float3 b2 = MathX.Reject(in a2, in b);
                                finalvelocity = a + b2;
                                break;
                            }
                    }
                    if (finalvelocity.Magnitude > Config.GetValue(maxSpeed) && characterController.IsUnderLocalUser)
                    {

                        DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(__instance.World.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, null);
                        DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(__instance.World.LocalUser.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, finalvelocity, null);
                        Msg("prevented knockback");
                        return false;

                    }
                    Msg("Finished code for knockback, not powerful enough.");
                    return true;
                }
                Msg("Finished code for knockback, not powerful enough.");
                return false;


            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterForceField), "OnContactStay", typeof(ICollider), typeof(ICollider))]
            private static bool OnContactStayPatch(CharacterForceField __instance, ICollider collider, ICollider other)
            {

                CharacterController characterController = other.ColliderOwner as CharacterController;
                if (characterController == null || !ShouldApplyForce(__instance, characterController))
                {
                    return false;
                }

                float3 a = GetForce(__instance, characterController);
                float3 a2 = characterController.LinearVelocity;
                switch (__instance.ForceMode.Value)
                {
                    case CharacterForceField.Mode.ConstantForce:
                        {
                            float3 b = float3.Zero;
                            if (a == b)
                            {
                                return false;
                            }

                            float3 b2 = a.Normalized;
                            
                            float num = MathX.Dot(in a2, in b2);
                            float num2 = (characterController.Jump ? __instance.HoldJumpMaxForceVelocity.Value : __instance.MaxForceVelocity.Value);
                            if (num < num2)
                            {
                                float3 b3 = MathX.Reject(in a2, in b2);
                                float3 a3 = MathX.Project(in a2, in b2);
                                b = a * __instance.Time.Delta;
                                a3 += b;
                                float magnitude = a3.Magnitude;
                                if (magnitude > num2)
                                {
                                    a3 *= num2 / magnitude;
                                }

                                a2 = (a2 = a3 + b3);
                            }

                            break;
                        }
                    case CharacterForceField.Mode.ConstantVelocity:
                        a2 = a;
                        break;
                }

                if (__instance.MinCharacterVelocity.Value > 0f || __instance.MaxCharacterVelocity.Value < float.MaxValue)
                {
                    float3 b = a2;
                    a2 = MathX.ClampMagnitude(in b, __instance.MaxCharacterVelocity, __instance.MinCharacterVelocity);
                }

                if (__instance.CharacterVelocityDampeningSpeed.Value > 0f)
                {
                    float3 b = a2;
                    float3 to = float3.Zero;
                    a2 = MathX.Lerp(in b, in to, __instance.Time.Delta * (float)__instance.CharacterVelocityDampeningSpeed);
                }
                
                float3 finalvelocity = a2;
                if (finalvelocity.Magnitude > Config.GetValue(maxSpeed) && characterController.IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(__instance.World.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, null);
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(__instance.World.LocalUser.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, finalvelocity, null);
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
                CharacterController characterController = __instance.Character.Evaluate(context);
                if (characterController == null)
                {
                    return false;
                }

                float3 finalvelocity = (__instance.Velocity.Evaluate(context));

                if (finalvelocity.Magnitude > Config.GetValue(maxSpeed) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                {

                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, context);
                    DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "knockbackcanceled", true, finalvelocity, context);
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
                if (context != null)
                {
                    if (__instance.Character.Evaluate(context) != null)
                    {
                        if (!Config.GetValue(allowGravityChange) && __instance.Character.Evaluate(context).IsUnderLocalUser)
                        {

                            DynamicImpulseHelper.Singleton.TriggerDynamicImpulse(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "gravitychangecanceled", true, context);
                            DynamicImpulseHelper.Singleton.TriggerDynamicImpulseWithArgument<float3>(context.LocalUser.GetBodyNodeSlot(BodyNode.Root), "gravitychangecanceled", true, __instance.Gravity.Evaluate(context), context);
                            __result = __instance.Character.Evaluate(context) != null;
                            Msg("prevented gravity change");
                            return false;

                        }
                    }
                }
                
                return true;
            }
        }
    }
}