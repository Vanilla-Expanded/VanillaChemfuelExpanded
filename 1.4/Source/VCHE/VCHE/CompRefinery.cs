using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VCHE
{
    public class CompProperties_Refinery : CompProperties
    {
        public CompProperties_Refinery()
        {
            compClass = typeof(CompRefinery);
        }
    }

    public class CompRefinery : ThingComp
    {
        private CompResourceProcessor resource;
        private Vector3 fireDrawPos;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            resource = parent.GetComp<CompResourceProcessor>();

            fireDrawPos = parent.DrawPos;
            fireDrawPos.y += 3f / 74f;
            fireDrawPos.z += 1.25f;
        }

        public override void PostDraw()
        {
            if (resource.Working)
            {
                CompFireOverlay.FireGraphic.Draw(fireDrawPos, Rot4.North, parent);
            }
        }
    }
}