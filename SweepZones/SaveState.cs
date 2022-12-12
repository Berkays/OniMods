using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using KSerialization;

namespace SweepZones
{
    internal class SaveState : KMonoBehaviour
    {
        [Serialize]
        private readonly Dictionary<int, PrioritySetting> zones = new Dictionary<int, PrioritySetting>();

        internal static SaveState Instance { get; private set; }

        [Serialize]
        public State Sweep = new State();
        [Serialize]
        public State Mop = new State();

        internal SaveState()
        {
            Instance = this;

            SweepZoneOverlay.SetupOverlays();
        }

        [OnDeserialized]
        private void OnDeserialized()
        {
            // Migrate old data
            if (zones != null && zones.Count > 0)
            {
                Sweep.Clear();
                foreach (var zone in zones)
                    Sweep[zone.Key] = zone.Value;

                zones.Clear();
            }

            // Remove any forbidden cells from serialized data
            if (ModIntegrations.ForbidItemsConfiguration.Enabled == false && Sweep != null && Sweep.Any(n => n.Value.priority_value == 10))
            {
                var forbiddenCells = Sweep.Where(n => n.Value.priority_value >= 10).Select(k => k.Key).ToList();
                foreach (var cell in forbiddenCells)
                    Sweep.DeleteCell(cell);
            }
        }

        [SerializationConfig(MemberSerialization.OptIn)]
        internal sealed class State : IEnumerable<KeyValuePair<int, PrioritySetting>>
        {
            [Serialize]
            private readonly Dictionary<int, PrioritySetting> zones = new Dictionary<int, PrioritySetting>();

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

                    this.zones[cell] = value;
                }
            }

            internal void Clear()
            {
                zones.Clear();
            }

            internal bool ContainsCell(int cell)
            {
                return zones.ContainsKey(cell);
            }

            internal void DeleteCell(int cell)
            {
                zones.Remove(cell);
            }

            public IEnumerator<KeyValuePair<int, PrioritySetting>> GetEnumerator()
            {
                return zones.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return zones.GetEnumerator();
            }
        }
    }
}