using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterEquipmentReloading
{
    public interface IReloadableComp
    {
        int RemainingCharges { get; }
        int MaxCharges { get; }
        ThingDef AmmoDef { get; }
        int AmmoCountPerCharge { get; }
        int AmmoCountToRefill { get; }
        bool CanUse();
        bool Use();
        void RefuelFromConsumed(int consumed);
        List<Thing> FindAllResources(Pawn pawn, ThingDef thingDef);
        int CountInventoryResources(Pawn pawn, ThingDef thingDef);
        List<Thing> FindReachableMapResources(Pawn pawn, ThingDef thingDef);
        void StartReloadJob(Pawn pawn);
    }
}
