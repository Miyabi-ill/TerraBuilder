namespace TerraBuilder.WorldGeneration.Chests
{
    using Newtonsoft.Json;

    public class ChestProbably
    {
        [JsonIgnore]
        public ChestContext ChestContext => Configs.Chests[Name];

        [JsonIgnore]
        public string Name { get; set; }

        public double Probably { get; set; } = 1.0;
    }
}
