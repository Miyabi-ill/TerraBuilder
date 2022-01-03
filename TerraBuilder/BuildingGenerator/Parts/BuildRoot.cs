namespace TerraBuilder.BuildingGenerator.Parts
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    [JsonConverter(typeof(PartsConverter))]
    public class BuildRoot : BuildParent
    {
        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();
    }
}
