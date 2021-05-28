namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class BuildRoot : BuildParent
    {
        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();
    }
}
