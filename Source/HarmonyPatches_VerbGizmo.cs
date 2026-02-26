using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterEquipmentReloading
{
    [HarmonyPatch(typeof(Command_VerbOwner))]
    [HarmonyPatch("TopRightLabel", MethodType.Getter)]
    public static class Command_VerbOwner_TopRightLabel_Patch
    {
        static bool Prefix(Command_VerbOwner __instance, ref string __result)
        {
            try
            {
                if (__instance.verb == null)
                {
                    return true;
                }

                CompBetterReloadable reloadableComp = null;

                if (__instance.verb.DirectOwner is CompApparelVerbOwner verbOwner && verbOwner.parent is Apparel apparel)
                {
                    reloadableComp = apparel.GetComp<CompBetterReloadable>();
                }

                if (reloadableComp == null)
                {
                    return true;
                }

                if (!reloadableComp.HasUnlimitedCharges)
                {
                    __result = $"{reloadableComp.RemainingCharges}/{reloadableComp.MaxCharges}";
                    return false;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
