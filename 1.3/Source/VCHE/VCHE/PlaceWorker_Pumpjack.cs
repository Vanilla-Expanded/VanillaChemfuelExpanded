using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VCHE
{
    public class PlaceWorker_Pumpjack : PlaceWorker_ShowDeepResources
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            bool canPlace = false;
            var cell = IntVec3.Invalid;
            // Check if any cell is on top of deepchem
            if (map.deepResourceGrid.ThingDefAt(loc) is ThingDef thingDef && thingDef.defName == "VCHE_Deepchem")
            {
                canPlace = true;
                cell = loc;
            }
            // Draw
            if (cell != IntVec3.Invalid)
            {
                var good = new List<IntVec3>();
                var treated = new HashSet<IntVec3>();
                var toCheck = new Queue<IntVec3>();

                toCheck.Enqueue(cell);
                treated.Add(cell);

                while (toCheck.Count > 0)
                {
                    var temp = toCheck.Dequeue();
                    good.Add(temp);

                    var neighbours = GenAdjFast.AdjacentCellsCardinal(temp);
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        var n = neighbours[i];
                        if (!treated.Contains(n) && map.deepResourceGrid.ThingDefAt(n) is ThingDef r && r.defName == "VCHE_Deepchem")
                        {
                            treated.Add(n);
                            toCheck.Enqueue(n);
                        }
                    }
                }
                GenDraw.DrawFieldEdges(good, Color.white);
            }

            if (!canPlace)
                return new AcceptanceReport("VCHE_CantPlaceHere".Translate());

            return canPlace;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);
            if (thing != null && thing.TryGetComp<CompPumpjack>() is CompPumpjack c)
            {
                GenDraw.DrawFieldEdges(c.lumpCells, Color.white);
            }
        }
    }
}