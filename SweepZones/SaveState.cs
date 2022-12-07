using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using KSerialization;

namespace SweepZones
{
    internal class SaveState : KMonoBehaviour, IEnumerable<KeyValuePair<int, PrioritySetting>>
    {
        [Serialize]
        private readonly Dictionary<int, PrioritySetting> zones = new Dictionary<int, PrioritySetting>();

        internal static SaveState Instance { get; private set; }

        internal SaveState()
        {
            Instance = this;
        }

        [OnDeserialized]
        private void OnDeserialized()
        {
            // Remove any forbidden cells from serialized data
            if (ModIntegrations.ForbidItemsConfiguration.Enabled == false && zones.Any(n => n.Value.priority_value == 10))
            {
                var forbiddenCells = zones.Where(n => n.Value.priority_value >= 10).Select(k => k.Key);
                foreach (var cell in forbiddenCells)
                    zones.Remove(cell);
            }
        }

        internal PrioritySetting this[int cell]
        {
            get
            {
                if (this.zones != null)
                    return this.zones[cell];

                return new PrioritySetting();
            }

            set
            {
                if (this.zones == null)
                    return;

                if (this.zones.ContainsKey(cell))
                {
                    this.zones[cell] = value;
                    return;
                }

                this.zones.Add(cell, value);
            }
        }

        internal bool ContainsCell(int cell)
        {
            return zones.ContainsKey(cell);
        }

        internal void DeleteCell(int cell)
        {
            zones.Remove(cell);
        }

        IEnumerator<KeyValuePair<int, PrioritySetting>> IEnumerable<KeyValuePair<int, PrioritySetting>>.GetEnumerator()
        {
            return this.zones.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.zones.GetEnumerator();
        }
    }
}