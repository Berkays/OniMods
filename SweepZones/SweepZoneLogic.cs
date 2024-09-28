using System.Reflection;
using UnityEngine;

namespace SweepZones
{
    public class SweepZoneLogic : KMonoBehaviour, ISim4000ms
    {
        private readonly FieldInfo isMarkedForClearFieldInfo;

        private readonly float maxMopAmt = 150f;
        private readonly GameObject Placer;
        private readonly Grid.SceneLayer visualizerLayer;

        public SweepZoneLogic()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            isMarkedForClearFieldInfo = typeof(Clearable).GetField("isMarkedForClear", flags);

            Placer = (GameObject)typeof(MopTool).GetField("Placer", flags).GetValue(MopTool.Instance);

            flags = BindingFlags.Instance | BindingFlags.Public;
            visualizerLayer = (Grid.SceneLayer)typeof(InterfaceTool).GetField("visualizerLayer", flags).GetValue(MopTool.Instance);

        }

        public void Sim4000ms(float dt)
        {
            foreach (var (cell, prioritySetting) in SaveState.Instance.Sweep)
            {
                GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];

                if (obj == null)
                    continue;

                ObjectLayerListItem objectLayerListItem = obj.GetComponent<Pickupable>().objectLayerListItem;
                while (objectLayerListItem != null)
                {
                    GameObject obj2 = objectLayerListItem.gameObject;
                    objectLayerListItem = objectLayerListItem.nextItem;

                    if (obj2 != null && obj2.GetComponent<MinionIdentity>() == null)
                    {
                        Clearable clearable = obj2.GetComponent<Clearable>();

                        if (clearable == null || !clearable.isClearable || clearable.PrefabID() == GameTags.Robots.Models.MorbRover)
                            continue;

                        var priorityValue = prioritySetting.priority_value;
                        if (priorityValue < 10 && ((bool)isMarkedForClearFieldInfo.GetValue(clearable)) == false)
                        {
                            clearable.MarkForClear();
                            Prioritizable component = obj2.GetComponent<Prioritizable>();
                            component?.SetMasterPriority(prioritySetting);
                            continue;
                        }

                        if (ModIntegrations.ForbidItemsConfiguration.Enabled && priorityValue == 10 && checkForbidCondition(obj2, out KPrefabID kpid))
                        {
                            // Mark forbid
                            kpid.AddTag(ModIntegrations.ForbidItemsConfiguration.Tag, true);
                        }
                    }
                }
            }

            foreach (var (cell, prioritySetting) in SaveState.Instance.Mop)
            {
                int objectLayer = (int)ObjectLayer.MopPlacer;
                GameObject obj = Grid.Objects[cell, objectLayer];
                if (Grid.Solid[cell] == false && obj == null && Grid.Element[cell].IsLiquid)
                {
                    if (Grid.Solid[Grid.CellBelow(cell)] && Grid.Mass[cell] <= maxMopAmt)
                    {
                        obj = Grid.Objects[cell, objectLayer] = Util.KInstantiate(Placer);
                        Vector3 position = Grid.CellToPosCBC(cell, visualizerLayer);
                        float num = -0.15f;
                        position.z += num;
                        obj.transform.SetPosition(position);
                        obj.SetActive(true);
                    }
                }
                if (obj == null)
                    continue;
                obj.GetComponent<Prioritizable>()?.SetMasterPriority(prioritySetting);
            }
        }

        private bool checkForbidCondition(GameObject obj, out KPrefabID kpid)
        {
            return obj.TryGetComponent(out kpid) && !kpid.HasTag(GameTags.Stored)
                    && obj.GetComponent("Forbiddable") != null
                    && kpid.HasTag(ModIntegrations.ForbidItemsConfiguration.Tag) == false;
        }
    }
}