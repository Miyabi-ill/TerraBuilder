namespace TerraBuilder.BuildingGenerator.Parts
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(PartsConverter))]
    public class Box : BuildParent
    {
    }
}
