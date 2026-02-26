using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterEquipmentReloading
{
    public class JobDriver_BetterReload : JobDriver
    {
        private const TargetIndex ApparelInd = TargetIndex.A;
        private const TargetIndex ResourceInd = TargetIndex.B;

        private const int ReloadDurationTicks = 120;

        private Apparel Apparel => (Apparel)job.GetTarget(ApparelInd).Thing;

        private CompApparelReloadable ReloadableComp => Apparel?.TryGetComp<CompApparelReloadable>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo target = job.GetTarget(ResourceInd);
            if (target.IsValid && target.HasThing && target.Thing.Spawned)
            {
                if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
                {
                    return false;
                }
            }
            if (job.targetQueueB != null)
            {
                pawn.ReserveAsManyAsPossible(job.targetQueueB, job);
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Apparel == null || ReloadableComp == null);
            this.FailOn(() => Apparel.Wearer != pawn);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            Thing resource = job.GetTarget(ResourceInd).Thing;
            ThingDef resourceDef = resource?.def;
            if (resourceDef == null && job.targetQueueB != null && job.targetQueueB.Count > 0)
            {
                Thing queuedThing = job.targetQueueB[0].Thing;
                resourceDef = queuedThing?.def;
            }

            Toil takeFromInventory = ToilMaker.MakeToil("TakeFromInventory");
            takeFromInventory.initAction = delegate
            {
                if (resourceDef == null || pawn.inventory == null || job.count <= 0) return;

                int remaining = job.count;
                int availableSpace = pawn.carryTracker.AvailableStackSpace(resourceDef);
                if (availableSpace <= 0) return;

                List<Thing> inventoryThings = new List<Thing>();
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    if (thing.def == resourceDef && thing.stackCount > 0)
                    {
                        inventoryThings.Add(thing);
                    }
                }
                inventoryThings.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));

                foreach (Thing thing in inventoryThings)
                {
                    if (remaining <= 0 || availableSpace <= 0) break;
                    int toTake = Math.Min(thing.stackCount, remaining);
                    toTake = Math.Min(toTake, availableSpace);
                    if (toTake > 0)
                    {
                        pawn.inventory.innerContainer.TryTransferToContainer(thing, pawn.carryTracker.innerContainer, toTake);
                        remaining -= toTake;
                        availableSpace -= toTake;
                    }
                }

                job.count = remaining;
            };
            takeFromInventory.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return takeFromInventory;

            Toil waitToil = Toils_General.Wait(ReloadDurationTicks);
            waitToil.WithProgressBarToilDelay(ApparelInd);
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;

            Toil gotoToil = Toils_Goto.GotoThing(ResourceInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ResourceInd);

            Toil jumpIfNoMapNeeded = Toils_Jump.JumpIf(waitToil, () =>
            {
                Thing targetThing = job.GetTarget(ResourceInd).Thing;
                if (job.count <= 0) return true;
                if (resourceDef != null && pawn.carryTracker.AvailableStackSpace(resourceDef) <= 0) return true;
                return targetThing == null || targetThing.Destroyed || targetThing.ParentHolder is Pawn_InventoryTracker;
            });
            yield return jumpIfNoMapNeeded;
            yield return gotoToil;

            Toil carryToil = Toils_Haul.StartCarryThing(ResourceInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false);
            yield return carryToil;
            yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(gotoToil, ResourceInd);

            yield return waitToil;

            Toil reloadToil = ToilMaker.MakeToil("Reload");
            reloadToil.initAction = delegate
            {
                DoReload();
            };
            reloadToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return reloadToil;

            Toil dropToil = ToilMaker.MakeToil("DropCarriedThing");
            dropToil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null && !carriedThing.Destroyed && carriedThing.stackCount > 0)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
                }
            };
            dropToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropToil;
        }

        private void DoReload()
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null || carriedThing.Destroyed) return;

            if (ReloadableComp == null) return;

            ThingDef resourceDef = ReloadableComp.Props.ammoDef;
            if (resourceDef == null || carriedThing.def != resourceDef) return;

            ReloadableComp.ReloadFrom(carriedThing);
        }
    }
}
