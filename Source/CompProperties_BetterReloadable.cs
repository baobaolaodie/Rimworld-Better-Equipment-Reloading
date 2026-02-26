using RimWorld;
using Verse;

namespace BetterEquipmentReloading
{
    public class CompProperties_BetterReloadable : CompProperties
    {
        public int maxCharges = 3;
        public ThingDef ammoDef;
        public int ammoCountPerCharge = 0;
        public int ammoCountToRefill = 0;
        public int baseReloadTicks = 60;
        public SoundDef soundReload;

        public CompProperties_BetterReloadable()
        {
            this.compClass = typeof(CompBetterReloadable);
        }
    }
}
