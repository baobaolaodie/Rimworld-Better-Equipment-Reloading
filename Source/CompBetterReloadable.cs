using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterEquipmentReloading
{
    public class CompBetterReloadable : ThingComp, IReloadableComp
    {
        public CompProperties_BetterReloadable Props => (CompProperties_BetterReloadable)this.props;

        private int remainingCharges;

        public int RemainingCharges => remainingCharges;
        public int MaxCharges => Props.maxCharges;
        public ThingDef AmmoDef => Props.ammoDef;
        public int AmmoCountPerCharge => Props.ammoCountPerCharge;
        public int AmmoCountToRefill => Props.ammoCountToRefill;

        public bool HasUnlimitedCharges => MaxCharges <= 0;

        public bool CanUse() => HasUnlimitedCharges || remainingCharges > 0;

        public bool Use()
        {
            if (!CanUse()) return false;
            if (!HasUnlimitedCharges) remainingCharges--;
            return true;
        }

        public void RefuelFromConsumed(int consumed)
        {
            if (HasUnlimitedCharges) return;
            
            if (Props.ammoCountPerCharge > 0)
            {
                int toAdd = consumed / Props.ammoCountPerCharge;
                remainingCharges = Math.Min(remainingCharges + toAdd, MaxCharges);
            }
            else if (Props.ammoCountToRefill > 0 && consumed >= Props.ammoCountToRefill)
            {
                remainingCharges = MaxCharges;
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

        private Thing FindLargestInventoryResource(Pawn pawn, ThingDef thingDef)
        {
            if (pawn?.inventory == null) return null;
            Thing best = null;
            int bestCount = 0;
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                if (thing.def == thingDef && thing.stackCount > 0 && thing.stackCount > bestCount)
                {
                    best = thing;
                    bestCount = thing.stackCount;
                }
            }
            return best;
        }

        public void StartReloadJob(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return;

            if (HasUnlimitedCharges || remainingCharges >= MaxCharges)
            {
                Messages.Message("BetterEquipmentReloading_AlreadyFull".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            ThingDef resourceDef = Props.ammoDef;
            if (resourceDef == null)
            {
                Messages.Message("BetterEquipmentReloading_NoAmmoDef".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int costPerCharge = Props.ammoCountPerCharge;
            int ammoCountToRefill = Props.ammoCountToRefill;
            
            if (costPerCharge <= 0 && ammoCountToRefill <= 0)
            {
                Messages.Message("BetterEquipmentReloading_InvalidCost".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int maxReloads = MaxCharges - remainingCharges;

            int inventoryCount = CountInventoryResources(pawn, resourceDef);
            List<Thing> mapResources = FindReachableMapResources(pawn, resourceDef);
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
                Messages.Message("BetterEquipmentReloading_NoResource".Translate(resourceDef.label), MessageTypeDefOf.RejectInput);
                return;
            }

            if (ammoCountToRefill > 0)
            {
                neededCount = ammoCountToRefill;
            }
            else
            {
                int maxPossibleReloads = Math.Min(maxReloads, availableCount / costPerCharge);
                neededCount = maxPossibleReloads * costPerCharge;
            }

            Thing resource = null;
            if (inventoryCount >= neededCount)
            {
                resource = FindLargestInventoryResource(pawn, resourceDef);
            }
            if (resource == null && mapResources.Count > 0)
            {
                resource = mapResources[0];
            }
            if (resource == null)
            {
                Messages.Message("BetterEquipmentReloading_NoResource".Translate(resourceDef.label), MessageTypeDefOf.RejectInput);
                return;
            }

            JobDef reloadJobDef = DefDatabase<JobDef>.GetNamed("BetterEquipmentReloading_Reload", false);
            if (reloadJobDef == null)
            {
                reloadJobDef = JobDefOf.Reload;
            }

            Job job = JobMaker.MakeJob(reloadJobDef);
            job.SetTarget(TargetIndex.A, parent);
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", Props.maxCharges);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            remainingCharges = Props.maxCharges;
        }
    }
}
