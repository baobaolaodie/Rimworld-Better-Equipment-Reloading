using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterEquipmentReloading
{
    [HarmonyPatch(typeof(Command))]
    [HarmonyPatch("GizmoOnGUIInt")]
    public static class Command_GizmoOnGUIInt_Patch
    {
        static bool Prefix(Command __instance, Rect butRect, GizmoRenderParms parms, ref GizmoResult __result, ref bool ___disabled, ref string ___disabledReason)
        {
            if (Event.current.type != EventType.MouseUp || Event.current.button != 1)
            {
                return true;
            }

            if (!Mouse.IsOver(butRect))
            {
                return true;
            }

            if (!__instance.Disabled)
            {
                return true;
            }

            if (__instance.RightClickFloatMenuOptions == null)
            {
                return true;
            }

            bool hasOptions = false;
            foreach (var option in __instance.RightClickFloatMenuOptions)
            {
                hasOptions = true;
                break;
            }

            if (!hasOptions)
            {
                return true;
            }

            Event.current.Use();
            __result = new GizmoResult(GizmoState.OpenedFloatMenu, Event.current);
            return false;
        }
    }

    [HarmonyPatch(typeof(Gizmo))]
    [HarmonyPatch("RightClickFloatMenuOptions", MethodType.Getter)]
    public static class Gizmo_RightClickFloatMenuOptions_Patch
    {
        static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Gizmo __instance)
        {
            foreach (var option in __result)
            {
                yield return option;
            }

            if (__instance is Command_VerbOwner commandVerbOwner)
            {
                if (commandVerbOwner.verb == null) yield break;

                if (commandVerbOwner.verb.DirectOwner is CompApparelVerbOwner verbOwner)
                {
                    Apparel apparel = verbOwner.parent as Apparel;
                    if (apparel == null) yield break;

                    CompApparelReloadable reloadableComp = apparel.GetComp<CompApparelReloadable>();
                    if (reloadableComp == null) yield break;

                    Pawn pawn = commandVerbOwner.verb.CasterPawn;
                    if (pawn == null) yield break;

                    if (reloadableComp.RemainingCharges < reloadableComp.MaxCharges)
                    {
                        var wrapper = HarmonyPatches_ReloadableInject.GetBetterReloadableComp(reloadableComp);
                        if (wrapper != null)
                        {
                            yield return new FloatMenuOption(
                                "BetterEquipmentReloading_ReloadLabel".Translate() + $" ({reloadableComp.RemainingCharges}/{reloadableComp.MaxCharges})",
                                delegate { HarmonyPatches_ReloadableInject.StartEnhancedReloadJob(pawn, apparel, wrapper); }
                            );
                        }
                    }
                }
            }
        }
    }
}
