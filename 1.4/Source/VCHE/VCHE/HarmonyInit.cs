using HarmonyLib;
using UnityEngine;
using Verse;

namespace VCHE
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.VanillaChemfuelExpanded");
            harmonyInstance.PatchAll();
        }
    }

    [HarmonyPatch(typeof(DeepResourceGrid))]
    [HarmonyPatch("DeepResourcesOnGUI", MethodType.Normal)]
    public static class Designator_Install_Patch
    {
        public static void Postfix()
        {
            Thing thing = Find.Selector.SingleSelectedThing;
            if (thing != null)
            {
                Map map = thing.Map;
                if (thing.TryGetComp<CompPumpjack>() is CompPumpjack _
                    && map.deepResourceGrid.AnyActiveDeepScannersOnMap())
                {
                    IntVec3 c = UI.MouseCell();
                    if (!c.InBounds(map))
                    {
                        return;
                    }
                    ThingDef thingDef = map.deepResourceGrid.ThingDefAt(c);
                    if (thingDef != null)
                    {
                        int num = map.deepResourceGrid.CountAt(c);
                        if (num > 0)
                        {
                            Vector2 vector = c.ToVector3().MapToUIPosition();
                            GUI.color = Color.white;
                            Text.Font = GameFont.Small;
                            Text.Anchor = TextAnchor.MiddleLeft;
                            float num2 = (UI.CurUICellSize() - 27f) / 2f;
                            Rect rect = new Rect(vector.x + num2, vector.y - UI.CurUICellSize() + num2, 27f, 27f);
                            Widgets.ThingIcon(rect, thingDef);
                            Widgets.Label(new Rect(rect.xMax + 4f, rect.y, 999f, 29f), "DeepResourceRemaining".Translate(NamedArgumentUtility.Named(thingDef, "RESOURCE"), num.Named("COUNT")));
                            Text.Anchor = TextAnchor.UpperLeft;
                        }
                    }
                }
            }
        }
    }
}
