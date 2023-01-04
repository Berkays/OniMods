using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Buildings;

namespace ResourceSensor
{
    internal class DistanceVisualizer : ColoredRangeVisualizer
    {
        private delegate void CachedDelegate(ColoredRangeVisualizer instance);
        private static CachedDelegate CreateVisualizers = (CachedDelegate)typeof(ColoredRangeVisualizer).GetMethod("CreateVisualizers", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(CachedDelegate));

        public List<int> visCells = new List<int>(400);

        public void Refresh()
        {
            CreateVisualizers(this);
        }

        protected override void VisualizeCells(ICollection<VisCellData> newCells)
        {
            for (int i = 0; i < visCells.Count; i++)
                newCells.Add(new VisCellData(visCells[i]));
        }
    }
}