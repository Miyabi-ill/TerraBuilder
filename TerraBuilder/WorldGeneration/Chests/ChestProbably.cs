namespace TerraBuilder.WorldGeneration.Chests
{
    using System.ComponentModel;
    using Newtonsoft.Json;

    public class ChestProbably
    {
        [JsonIgnore]
        public ChestContext ChestContext => Configs.Chests[Name];

        public string Name { get; set; }

        public double Probably { get; set; } = 100.0;
    }
}
