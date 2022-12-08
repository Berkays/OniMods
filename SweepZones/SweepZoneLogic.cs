using UnityEngine;

namespace SweepZones
{
    public class SweepZoneLogic : KMonoBehaviour, ISim4000ms
    {
        public void Sim4000ms(float dt)
        {
            foreach (var (cell, prioritySetting) in SaveState.Instance)
            {
                GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];

                if (obj == null)
                    continue;

                ObjectLayerListItem objectLayerListItem = obj.GetComponent<Pickupable>().objectLayerListItem;
                while (objectLayerListItem != null)
                {
                    GameObject obj2 = objectLayerListItem.gameObject;
                    objectLayerListItem = objectLayerListItem.nextItem;

                    if (obj2 != null && obj2.GetComponent<MinionIdentity>() == null && obj2.GetComponent<Clearable>().isClearable)
                    {
                        var priorityValue = prioritySetting.priority_value;
                        if (priorityValue < 10)
                        {
                            obj2.GetComponent<Clearable>().MarkForClear();
                            Prioritizable component = obj2.GetComponent<Prioritizable>();
                            component?.SetMasterPriority(prioritySetting);
                            return;
                        }

                        if (ModIntegrations.ForbidItemsConfiguration.Enabled && priorityValue == 10 && checkForbidCondition(obj2, out KPrefabID kpid))
                        {
                            // Mark forbid
                            kpid.AddTag(ModIntegrations.ForbidItemsConfiguration.Tag, true);
                        }
                    }
                }
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