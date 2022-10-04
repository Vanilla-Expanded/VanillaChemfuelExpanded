using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VCHE
{
    public class CompProperties_PumpjackAnimation : CompProperties
    {
        public CompProperties_PumpjackAnimation()
        {
            compClass = typeof(CompPumpjackAnimation);
        }
    }

    [StaticConstructorOnStartup]
    public class CompPumpjackAnimation : ThingComp
    {
        private static readonly Material PumpjackBottom = MaterialPool.MatFrom("Things/Building/Production/DeepchemPumpjack_Bottom");
        private static readonly Material PumpjackPump = MaterialPool.MatFrom("Things/Building/Production/DeepchemPumpjack_Pump");

        private Vector3 pumpPos = Vector3.zero;
        private Vector3 pumpPosMax;
        private Vector3 bottomPos;
        private Vector3 trueCenter;

        private CompDeepExtractor extractor;

        private bool goingUp = true;

        public CompProperties_PumpjackAnimation Props => (CompProperties_PumpjackAnimation)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            extractor = parent.GetComp<CompDeepExtractor>();

            trueCenter = parent.TrueCenter();
            if (pumpPos.x != trueCenter.x)
                pumpPos = trueCenter + new Vector3(0f, 0.75f, 0f);

            bottomPos = trueCenter + new Vector3(0f, 1f, 0f);
            pumpPosMax = trueCenter + new Vector3(0f, 0f, 1.1f);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref goingUp, "goingUp");
            Scribe_Values.Look(ref pumpPos, "pumpPos");
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned && !extractor.cycleOver)
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
                        goingUp = true;
                    }
                }
            }
            else if (parent.Spawned && extractor.cycleOver && pumpPos.z > trueCenter.z)
            {
                pumpPos.z -= 0.03f;
                if (pumpPos.z < trueCenter.z)
                {
                    goingUp = true;
                }
            }
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