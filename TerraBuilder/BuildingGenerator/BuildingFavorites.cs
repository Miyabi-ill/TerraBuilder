namespace TerraBuilder.BuildingGenerator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class BuildingFavorites
    {
        [JsonProperty]
        public Dictionary<string, bool> Favorites { get; private set; } = new Dictionary<string, bool>();

        public bool IsFavorite(string buildingName)
        {
            if (Favorites.ContainsKey(buildingName))
            {
                return Favorites[buildingName];
            }

            return false;
        }

        public void SetFavorite(string buildingName, bool isFavorite = false)
        {
            if (!isFavorite)
            {
                if (Favorites.ContainsKey(buildingName))
                {
                    Favorites.Remove(buildingName);
                    return;
                }

                return;
            }

            if (Favorites.ContainsKey(buildingName))
            {
                Favorites[buildingName] = true;
            }
            else
            {
                Favorites.Add(buildingName, true);
            }
        }
    }
}
