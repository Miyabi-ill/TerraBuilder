namespace TerraBuilder.WorldGeneration.Chests
{
    using Newtonsoft.Json;

    public class ItemSlotOrItemProbablyAndStack
    {
        [JsonIgnore]
        public ItemSlotContext ItemSlotContext
        {
            get
            {
                if (Configs.ItemSlots.ContainsKey(Name))
                {
                    return Configs.ItemSlots[Name];
                }
                else
                {
                    return null;
                }
            }
        }

        [JsonIgnore]
        public ItemContext ItemContext
        {
            get
            {
                if (Configs.Items.ContainsKey(Name))
                {
                    return Configs.Items[Name];
                }
                else
                {
                    return null;
                }
            }
        }

        public string Name { get; set; } = string.Empty;

        public int Min { get; set; } = 1;

        public int Max { get; set; } = 1;

        public double Probably { get; set; } = 100.0;
    }
}
