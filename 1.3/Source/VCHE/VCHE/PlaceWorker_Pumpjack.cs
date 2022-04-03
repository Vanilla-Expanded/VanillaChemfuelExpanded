using RimWorld;
using System.Linq;
using Verse;

namespace VCHE
{
    public class PlaceWorker_Pumpjack : PlaceWorker_ShowDeepResources
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            bool canPlace = false;
            var cells = GenRadial.RadialCellsAround(loc, checkingDef.specialDisplayRadius, true);
            for (int i = 0; i < cells.Count(); i++)
            {
                var cell = cells.ElementAt(i);
                if (map.deepResourceGrid.ThingDefAt(cell) is ThingDef thingDef && thingDef.defName == "VCHE_Deepchem")
                {
                    canPlace = true;
                }
            }

            if (!canPlace)
                return new AcceptanceReport("VCHE_CantPlaceHere".Translate());

            return canPlace;
        }
    }
}