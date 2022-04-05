using PipeSystem;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VCHE
{
    public class CompProperties_Pumpjack : CompProperties
    {
        public CompProperties_Pumpjack()
        {
            compClass = typeof(CompPumpjack);
        }

        public int ticksPerPortion = 60;
    }

    [StaticConstructorOnStartup]
    public class CompPumpjack : ThingComp
    {
        private static readonly Material PumpjackBottom = MaterialPool.MatFrom("Things/Building/Production/DeepchemPumpjack_Bottom");
        private static readonly Material PumpjackPump = MaterialPool.MatFrom("Things/Building/Production/DeepchemPumpjack_Pump");

        private Vector3 pumpPos = Vector3.zero;
        private Vector3 pumpPosMax;
        private Vector3 bottomPos;
        private Vector3 trueCenter;

        private CompResourceStorage resource;
        private CompPowerTrader powerComp;

        private int nextProduceTick = -1;
        private bool noCapacity = false;
        private bool goingUp = true;
        private bool cycleOver = true;
        private List<IntVec3> adjCells;

        public CompProperties_Pumpjack Props => (CompProperties_Pumpjack)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            resource = parent.GetComp<CompResourceStorage>();
            powerComp = parent.GetComp<CompPowerTrader>();
            adjCells = adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();

            trueCenter = parent.TrueCenter();
            if (pumpPos == Vector3.zero)
                pumpPos = trueCenter + new Vector3(0f, 0.75f, 0f);

            bottomPos = trueCenter + new Vector3(0f, 1f, 0f);
            pumpPosMax = trueCenter + new Vector3(0f, 0f, 1.1f);
        }

        public override void PostDeSpawn(Map map)
        {
            nextProduceTick = -1;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref nextProduceTick, "nextProduceTick", 0);
            Scribe_Values.Look(ref noCapacity, "noCapacity", false);
            Scribe_Values.Look(ref cycleOver, "cycleOver");
            Scribe_Values.Look(ref goingUp, "goingUp");
            Scribe_Values.Look(ref pumpPos, "pumpPos");
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                var ticksGame = Find.TickManager.TicksGame;
                if (nextProduceTick == -1)
                {
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                }
                else if (ticksGame >= nextProduceTick)
                {
                    TryProducePortion();
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                }

                if (!cycleOver)
                {
                    if (goingUp)
                    {
                        pumpPos.z += 0.01f;
                        if (pumpPos.z > pumpPosMax.z)
                            goingUp = false;
                    }
                    else
                    {
                        pumpPos.z -= 0.03f;
                        if (pumpPos.z < trueCenter.z)
                        {
                            cycleOver = true;
                            goingUp = true;
                        }
                    }
                }
            }
        }

        private void TryProducePortion()
        {
            // No power -> Return
            if (powerComp != null && !powerComp.PowerOn)
                return;
            // Get resource
            bool nextResource = GetNextResource(out ThingDef resDef, out int count, out IntVec3 cell);
            // Resource isn't deepchem -> Return
            if (resDef == null || resDef.defName != "VCHE_Deepchem")
                return;

            var map = parent.Map;
            // Resource comp is here
            if (resource != null)
            {
                var net = resource.PipeNet;
                if (net.connectors.Count > 1)
                {
                    noCapacity = net.AvailableCapacity == 0;

                    if (!noCapacity)
                    {
                        parent.Map.deepResourceGrid.SetAt(cell, resDef, count - 1);
                        net.DistributeAmongStorage(1);

                        if (cycleOver) cycleOver = false;
                    }

                    return;
                }
            }
            // If there is no resource comp
            // Deplete resource by one
            if (nextResource)
            {
                parent.Map.deepResourceGrid.SetAt(cell, resDef, count - 1);
                // Spawn items
                for (int i = 0; i < adjCells.Count; i++)
                {
                    // Find an output cell
                    var c = adjCells[i];
                    if (c.Walkable(map))
                    {
                        // Try find thing of the same def
                        var t = c.GetFirstThing(map, resDef);
                        if (t != null)
                        {
                            if ((t.stackCount + 1) > t.def.stackLimit)
                                continue;
                            // We found some, modifying stack size
                            t.stackCount += 1;
                        }
                        else
                        {
                            // We didn't find any, creating thing
                            t = ThingMaker.MakeThing(resDef);
                            t.stackCount = 1;
                            GenPlace.TryPlaceThing(t, c, map, ThingPlaceMode.Near);
                        }
                        break;
                    }
                }
                if (cycleOver) cycleOver = false;
            }
        }

        private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            return DeepDrillUtility.GetNextResource(parent.Position, parent.Map, out resDef, out countPresent, out cell);
        }

        public override string CompInspectStringExtra()
        {
            if (parent.Spawned)
            {
                if (noCapacity)
                {
                    return "VCHE_CantPump".Translate();
                }
                GetNextResource(out var resDef, out var _, out var _);
                if (resDef == null || resDef.defName != "VCHE_Deepchem")
                {
                    return "DeepDrillNoResources".Translate();
                }
            }
            return null;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            DrawMat(PumpjackPump, pumpPos);
            DrawMat(PumpjackBottom, bottomPos);
        }

        private void DrawMat(Material mat, Vector3 drawPos)
        {
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, parent.Rotation.AsQuat, new Vector3(3, 1, 3)), mat, 0);
        }
    }
}