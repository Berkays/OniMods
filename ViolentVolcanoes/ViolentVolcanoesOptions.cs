using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ViolentVolcanoes
{
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    public class ModOptions
    {
        [Option("Emit diamond tiles", "Natural diamond tiles will be created on meteor impact")]
        [JsonProperty]
        public bool CreateDiamondTiles { get; set; }

        public ModOptions()
        {
            CreateDiamondTiles = true;
        }
    }
}
