using UnityEngine;

namespace SweepZones
{
    public class SweepZoneLogic : KMonoBehaviour, ISim4000ms
    {
        public void Sim4000ms(float dt)
        {
            foreach (var (cell, priority) in SaveState.Instance)
            {
                GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];

                if (obj == null)
                    continue;

                ObjectLayerListItem objectLayerListItem = obj.GetComponent<Pickupable>().objectLayerListItem;
                while (objectLayerListItem != null)
                {
                    GameObject obj2 = objectLayerListItem.gameObject;
                    objectLayerListItem = objectLayerListItem.nextItem;

                    if (!(obj2 == null) && !(obj2.GetComponent<MinionIdentity>() != null) && obj2.GetComponent<Clearable>().isClearable)
                    {
                        obj2.GetComponent<Clearable>().MarkForClear();
                        Prioritizable component = obj2.GetComponent<Prioritizable>();
                        component?.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.basic, priority));
                    }
                }
            }
        }
    }
}