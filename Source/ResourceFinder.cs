using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterEquipmentReloading
{
    public static class ResourceFinder
    {
        public static int CountInventoryResources(Pawn pawn, ThingDef thingDef)
        {
            if (pawn == null || pawn.inventory == null || thingDef == null) return 0;
            
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

        public static Thing FindLargestInventoryResource(Pawn pawn, ThingDef thingDef)
        {
            if (pawn == null || pawn.inventory == null || thingDef == null) return null;
            
            Thing largest = null;
            int largestCount = 0;
            
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                if (thing.def == thingDef && thing.stackCount > 0 && thing.stackCount > largestCount)
                {
                    largest = thing;
                    largestCount = thing.stackCount;
                }
            }
            return largest;
        }

        public static List<Thing> FindReachableMapResources(Pawn pawn, ThingDef thingDef)
        {
            List<Thing> result = new List<Thing>();
            
            if (pawn == null || pawn.Map == null || thingDef == null) return result;
            
            List<Thing> allResources = pawn.Map.listerThings.ThingsOfDef(thingDef);
            
            foreach (Thing thing in allResources)
            {
                if (thing == null || thing.Destroyed || !thing.Spawned) continue;
                if (thing.stackCount <= 0) continue;
                if (!pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly)) continue;
                
                result.Add(thing);
            }
            
            result.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));
            return result;
        }

        public static List<Thing> FindAllResources(Pawn pawn, ThingDef thingDef)
        {
            List<Thing> result = new List<Thing>();
            
            if (pawn == null || thingDef == null) return result;
            
            if (pawn.inventory != null)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    if (thing.def == thingDef && thing.stackCount > 0)
                    {
                        result.Add(thing);
                    }
                }
            }
            
            if (pawn.Map != null)
            {
                List<Thing> mapResources = FindReachableMapResources(pawn, thingDef);
                result.AddRange(mapResources);
            }
            
            result.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));
            return result;
        }

        public static int CountAvailableResources(Pawn pawn, ThingDef thingDef)
        {
            if (pawn == null || thingDef == null) return 0;
            
            int count = CountInventoryResources(pawn, thingDef);
            
            if (pawn.Map != null)
            {
                List<Thing> mapResources = FindReachableMapResources(pawn, thingDef);
                foreach (Thing thing in mapResources)
                {
                    count += thing.stackCount;
                }
            }
            
            return count;
        }
    }
}
