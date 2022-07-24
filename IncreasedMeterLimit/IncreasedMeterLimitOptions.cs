using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace IncreasedMeterLimit
{
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    public class ModOptions
    {
        [Option("Liquid Meter Limit", "Maximum limit in units for liquid meters")]
        [Limit(200, 99999)]
        [JsonProperty]
        public int LiquidMeterLimit { get; set; }

        [Option("Gas Meter Limit", "Maximum limit in units for gas meters")]
        [Limit(200, 99999)]
        [JsonProperty]
        public int GasMeterLimit { get; set; }

        [Option("Conveyor Meter Limit", "Maximum limit in units for conveyor meters")]
        [Limit(500, 99999)]
        [JsonProperty]
        public int ConveyorMeterLimit { get; set; }

        public ModOptions()
        {
            LiquidMeterLimit = 5000;
            GasMeterLimit = 5000;
            ConveyorMeterLimit = 5000;
        }
    }
}
