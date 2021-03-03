namespace TUBGWorldGenerator.WorldGeneration.Chests
{
    using Newtonsoft.Json;

    public class ItemContext : ActionContext
    {
        [JsonIgnore]
        public string Name { get; set; }

        public int PrefixID { get; set; }

        public int ItemID { get; set; }
    }
}
