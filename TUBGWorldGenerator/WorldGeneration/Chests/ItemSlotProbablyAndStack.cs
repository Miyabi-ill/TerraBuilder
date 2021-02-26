namespace TUBGWorldGenerator.WorldGeneration.Chests
{
    using Newtonsoft.Json;

    public class ItemSlotProbablyAndStack
    {
        [JsonIgnore]
        public ItemSlotContext Context => Configs.ItemSlots[Name];

        public string Name { get; set; }

        public int Min { get; set; } = 1;

        public int Max { get; set; } = 1;

        public double Probably { get; set; } = 1.0;
    }
}
