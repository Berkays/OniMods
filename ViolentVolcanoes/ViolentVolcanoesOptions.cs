using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ViolentVolcanoes
{
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    public class ModOptions
    {
        [Option("Emit natural tiles", "Natural tiles will be created on meteor impact")]
        [JsonProperty]
        public bool CreateNaturalTiles { get; set; }

        [Option("Natural tile elemenet")]
        [JsonProperty]
        public SimHashes TileMaterial { get; set; }

        public ModOptions()
        {
            CreateNaturalTiles = true;
            TileMaterial = SimHashes.Diamond;
        }

    }
}
