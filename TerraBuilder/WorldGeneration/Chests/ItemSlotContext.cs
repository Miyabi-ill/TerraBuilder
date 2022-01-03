namespace TerraBuilder.WorldGeneration.Chests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// アイテムのスロットの設定.
    /// </summary>
    public class ItemSlotContext : ActionContext
    {
        [JsonIgnore]
        public string Name { get; set; }

        public List<ItemSlotOrItemProbablyAndStack> Items { get; private set; } = new List<ItemSlotOrItemProbablyAndStack>();
    }
}
