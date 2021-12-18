namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    [JsonConverter(typeof(PartsConverter))]
    public class BuildRoot : BuildParent
    {
        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();
    }
}
