namespace SweepZones
{
    [System.Flags]
    internal enum ToolMode
    {
        Sweep = 1,
        SweepClear = 2,
        Forbid = 4,
        Mop = 16,
        MopClear = 32,
    }
}