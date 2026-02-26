using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterEquipmentReloading
{
    [HarmonyPatch]
    public static class HarmonyPatches_ReloadableInject
    {
        public static IReloadableComp GetBetterReloadableComp(CompApparelReloadable comp)
        {
            if (comp == null) return null;
            return new VanillaReloadableWrapper(comp);
        }

        public static IReloadableComp GetBetterReloadableComp(ThingWithComps thing)
        {
            if (thing == null) return null;
            var comp = thing.TryGetComp<CompApparelReloadable>();
            return GetBetterReloadableComp(comp);
        }

        [HarmonyPatch(typeof(CompApparelReloadable), "NeedsReload")]
        [HarmonyPostfix]
        public static void NeedsReload_Postfix(CompApparelReloadable __instance, ref bool __result)
        {
            if (!__result) return;
            var wrapper = GetBetterReloadableComp(__instance);
            if (wrapper == null) return;
            __result = wrapper.RemainingCharges < wrapper.MaxCharges;
        }

        public static void StartEnhancedReloadJob(Pawn pawn, Apparel apparel, IReloadableComp wrapper)
        {
            if (pawn == null || pawn.Map == null || apparel == null || wrapper == null) return;

            ThingDef ammoDef = wrapper.AmmoDef;
            if (ammoDef == null)
            {
                Messages.Message("BetterEquipmentReloading_NoAmmoDef".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int costPerCharge = wrapper.AmmoCountPerCharge;
            int ammoCountToRefill = wrapper.AmmoCountToRefill;
            
            if (costPerCharge <= 0 && ammoCountToRefill <= 0)
            {
                Messages.Message("BetterEquipmentReloading_InvalidCost".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int maxReloads = wrapper.MaxCharges - wrapper.RemainingCharges;
            if (maxReloads <= 0)
            {
                Messages.Message("BetterEquipmentReloading_AlreadyFull".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int inventoryCount = wrapper.CountInventoryResources(pawn, ammoDef);
            List<Thing> mapResources = wrapper.FindReachableMapResources(pawn, ammoDef);

            int mapCount = 0;
            foreach (Thing thing in mapResources)
            {
                mapCount += thing.stackCount;
            }

            int availableCount = inventoryCount + mapCount;
            int neededCount;

            if (ammoCountToRefill > 0)
            {
                neededCount = ammoCountToRefill;
            }
            else
            {
                neededCount = costPerCharge;
            }

            if (availableCount < neededCount)
            {
                Messages.Message("BetterEquipmentReloading_NoResource".Translate(ammoDef.label), MessageTypeDefOf.RejectInput);
                return;
            }

            if (ammoCountToRefill > 0)
            {
                neededCount = ammoCountToRefill;
            }
            else
            {
                int maxPossibleReloads = System.Math.Min(maxReloads, availableCount / costPerCharge);
                neededCount = maxPossibleReloads * costPerCharge;
            }

            Thing resource = FindBestResource(pawn, ammoDef, inventoryCount, mapResources, neededCount);
            if (resource == null)
            {
                Messages.Message("BetterEquipmentReloading_NoResource".Translate(ammoDef.label), MessageTypeDefOf.RejectInput);
                return;
            }

            JobDef reloadJobDef = DefDatabase<JobDef>.GetNamed("BetterEquipmentReloading_Reload", false);
            if (reloadJobDef == null)
            {
                reloadJobDef = JobDefOf.Reload;
            }

            Job job = JobMaker.MakeJob(reloadJobDef);
            job.SetTarget(TargetIndex.A, apparel);
            job.SetTarget(TargetIndex.B, resource);

            if (mapResources.Count > 1 && resource == mapResources[0])
            {
                if (job.targetQueueB == null) job.targetQueueB = new List<LocalTargetInfo>();
                for (int i = 1; i < mapResources.Count && job.targetQueueB.Count < 10; i++)
                {
                    job.targetQueueB.Add(mapResources[i]);
                }
            }

            job.count = neededCount;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private static Thing FindBestResource(Pawn pawn, ThingDef ammoDef, int inventoryCount, List<Thing> mapResources, int neededCount)
        {
            if (inventoryCount >= neededCount)
            {
                if (pawn?.inventory != null)
                {
                    Thing best = null;
                    int bestCount = 0;
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def == ammoDef && thing.stackCount > 0 && thing.stackCount > bestCount)
                        {
                            best = thing;
                            bestCount = thing.stackCount;
                        }
                    }
                    if (best != null) return best;
                }
            }

            if (mapResources.Count > 0)
            {
                return mapResources[0];
            }

            return null;
        }
    }

    public class VanillaReloadableWrapper : IReloadableComp
    {
        private readonly CompApparelReloadable comp;

        public VanillaReloadableWrapper(CompApparelReloadable comp)
        {
            this.comp = comp;
        }

        public int RemainingCharges => comp?.RemainingCharges ?? 0;
        public int MaxCharges => comp?.MaxCharges ?? 0;
        public ThingDef AmmoDef => comp?.Props?.ammoDef;
        public int AmmoCountPerCharge => comp?.Props?.ammoCountPerCharge ?? 1;
        public int AmmoCountToRefill => comp?.Props?.ammoCountToRefill ?? 0;

        public bool CanUse()
        {
            string reason;
            return comp?.CanBeUsed(out reason) ?? false;
        }

        public bool Use()
        {
            if (comp == null) return false;
            comp.UsedOnce();
            return true;
        }

        public void RefuelFromConsumed(int consumed)
        {
            if (comp == null) return;
            var carriedThing = comp.Wearer?.carryTracker?.CarriedThing;
            if (carriedThing != null)
            {
                comp.ReloadFrom(carriedThing);
            }
        }

        public List<Thing> FindAllResources(Pawn pawn, ThingDef thingDef)
        {
            List<Thing> resources = new List<Thing>();

            if (pawn == null || pawn.Map == null) return resources;

            if (pawn.inventory != null)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    if (thing.def == thingDef && thing.stackCount > 0)
                    {
                        resources.Add(thing);
                    }
                }
            }

            foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(thingDef))
            {
                if (thing != null && !thing.Destroyed && thing.Spawned && thing.stackCount > 0)
                {
                    resources.Add(thing);
                }
            }

            resources.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));
            return resources;
        }

        public int CountInventoryResources(Pawn pawn, ThingDef thingDef)
        {
            if (pawn?.inventory == null) return 0;
            int count = 0;
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                if (thing.def == thingDef && thing.stackCount > 0)
                {
                    count += thing.stackCount;
                }
            }
            return count;
        }

        public List<Thing> FindReachableMapResources(Pawn pawn, ThingDef thingDef)
        {
            List<Thing> resources = new List<Thing>();
            if (pawn?.Map == null) return resources;
            foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(thingDef))
            {
                if (thing != null && !thing.Destroyed && thing.Spawned && thing.stackCount > 0 &&
                    pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    resources.Add(thing);
                }
            }
            resources.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));
            return resources;
        }

        public void StartReloadJob(Pawn pawn)
        {
            if (pawn == null || comp == null) return;
            HarmonyPatches_ReloadableInject.StartEnhancedReloadJob(pawn, comp.parent as Apparel, this);
        }
    }
}
