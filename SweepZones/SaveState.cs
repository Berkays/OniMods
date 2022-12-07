using System.Collections;
using System.Collections.Generic;
using KSerialization;

namespace SweepZones
{
    public class SaveState : KMonoBehaviour, IEnumerable<KeyValuePair<int, int>>
    {
        [Serialize]
        private readonly Dictionary<int, int> zones = new Dictionary<int, int>();

        internal static SaveState Instance { get; private set; }

        public SaveState()
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
        {
            get
            {
                if (this.zones != null)
                    return this.zones[cell];

                return 5;
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

        public bool ContainsCell(int cell)
        {
            return zones.ContainsKey(cell);
        }

        public void DeleteCell(int cell)
        {
            zones.Remove(cell);
        }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            return this.zones.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.zones.GetEnumerator();
        }
    }
}