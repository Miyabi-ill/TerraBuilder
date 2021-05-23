namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class BuildingsDict
    {

        private Dictionary<string, BuildBase> buildingsDictionary = new Dictionary<string, BuildBase>();

        public string BuildingsDirectory { get; set; }

        public BuildBase this[string buildingName]
        {
            get => buildingsDictionary[buildingName];
            set
            {
                if (buildingsDictionary.ContainsKey(buildingName))
                {
                    buildingsDictionary[buildingName] = value;
                }
                else
                {
                    buildingsDictionary.Add(buildingName, value);
                }
            }
        }

        public void Update()
        {
            string path = BuildingsDirectory;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = ".";
            }

            foreach (string file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    try
                    {
                        var build = JsonConvert.DeserializeObject<BuildRoot>(sr.ReadToEnd(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        if (!string.IsNullOrEmpty(build.Name))
                        {
                            this[build.Name] = build;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }
    }
}
