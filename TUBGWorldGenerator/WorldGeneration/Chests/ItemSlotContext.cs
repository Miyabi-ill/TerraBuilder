namespace TUBGWorldGenerator.WorldGeneration.Chests
{
    using System.Collections.Generic;

    /// <summary>
    /// アイテムのスロットの設定。
    /// </summary>
    public class ItemSlotContext : ActionContext
    {
        public string Name { get; set; }

        public List<ProbablyAndStackContext<ItemContext>> Items { get; private set; } = new List<ProbablyAndStackContext<ItemContext>>();
    }
}
