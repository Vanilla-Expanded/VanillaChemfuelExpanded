using PipeSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPowerExpanded;
using Verse;

namespace VCHE
{
    public class CompProperties_ChemfuelPump : CompProperties
    {
        public CompProperties_ChemfuelPump()
        {
            compClass = typeof(CompChemfuelPump);
        }

        public int fuelProduced = 1;

        public float fuelInterval = 1f;
    }

    public class CompChemfuelPump : ThingComp
    {
        public Building_ChemfuelPond chemfuelPond;
        public CompResource compResource;
        public float ticksInADay = 60000f;
        public int ticksCounter = 0;

        public CompProperties_ChemfuelPump Props
        {
            get
            {
                return (CompProperties_ChemfuelPump)props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksCounter, "ticksCounter", 0, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
            compResource = parent.GetComp<CompResource>();
        }

        public override void CompTick()
        {
            if (chemfuelPond != null && chemfuelPond.fuelLeft > 0)
            {
                ticksCounter++;
                if (ticksCounter > ticksInADay * Props.fuelInterval)
                {
                    if (compResource != null && compResource.PipeNet.connectors.Count > 1 && compResource.PipeNet.AvailableCapacity >= Props.fuelProduced)
                    {
                        chemfuelPond.fuelLeft -= Props.fuelProduced;
                        compResource.PipeNet.DistributeAmongStorage(Props.fuelProduced);
                        ticksCounter = 0;
                    }
                    else
                    {
                        chemfuelPond.fuelLeft -= Props.fuelProduced;
                        Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel, null);
                        thing.stackCount = Props.fuelProduced;
                        GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near, null, null, default);
                        ticksCounter = 0;
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (chemfuelPond != null && chemfuelPond.fuelLeft > 0)
            {
                stringBuilder.Append("VPE_PondHasFuel".Translate(chemfuelPond.fuelLeft));
                stringBuilder.AppendLine();
                if (compResource != null && compResource.PipeNet.connectors.Count > 1 && compResource.PipeNet.AvailableCapacity < Props.fuelProduced)
                {
                    stringBuilder.Append("VCHE_CantPump".Translate());
                }
                else
                {
                    int ticks = (int)(ticksInADay * Props.fuelInterval) - ticksCounter;
                    stringBuilder.Append("VPE_PumpProducing".Translate(Props.fuelProduced, ticks.ToStringTicksToPeriod(true, false, true, true)));
                }

                return stringBuilder.ToString();
            }
            else
            {
                stringBuilder.Append("VPE_PondNoFuel".Translate());
                return stringBuilder.ToString();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ticksCounter = (int)((ticksInADay * Props.fuelInterval) - 100);
                    },
                    defaultLabel = "DEBUG: Produce next tick",
                    defaultDesc = "Produce next tick"
                }; ;
            }
        }
    }
}
