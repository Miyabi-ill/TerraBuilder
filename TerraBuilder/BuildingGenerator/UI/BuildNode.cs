namespace TerraBuilder.BuildingGenerator.UI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TerraBuilder.BuildingGenerator.Parts;

    public class BuildNode
    {
        public BuildNode(BuildBase build)
        {
            Build = build;
            if (build is BuildParent parent)
            {
                foreach (var child in parent.Childs)
                {
                    Child.Add(new BuildNode(child));
                }

                // できたらChilds情報を同期させたい
                // parent.Childs.CollectionChanged += Childs_CollectionChanged;
            }
            else if (build is Repeat repeat)
            {
                if (repeat.Building != null)
                {
                    Child.Add(new BuildNode(repeat.Building));
                }
            }
        }

        public BuildBase Build { get; }

        public string DisplayName => string.IsNullOrEmpty(Build?.Name) ? Build?.GetType()?.Name : Build.Name;

        public ObservableCollection<BuildNode> Child { get; } = new ObservableCollection<BuildNode>();
    }
}
