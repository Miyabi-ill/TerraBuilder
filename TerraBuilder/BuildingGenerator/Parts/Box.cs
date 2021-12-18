namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Terraria;

    [JsonConverter(typeof(PartsConverter))]
    public class Box : BuildParent
    {
    }
}
