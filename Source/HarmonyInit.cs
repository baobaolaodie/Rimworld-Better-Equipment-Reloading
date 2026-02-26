using HarmonyLib;
using Verse;

namespace BetterEquipmentReloading
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("SkylarTech.BetterEquipmentReloading");
            harmony.PatchAll();
        }
    }
}
