namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Terraria;

    [JsonConverter(typeof(PartsConverter))]
    public class Line : BuildBase
    {
        /// <inheritdoc/>
        public override RandomValue X { get; set; }

        /// <inheritdoc/>
        public override RandomValue Y { get; set; }

        public RandomValue EndX { get; set; }

        public RandomValue EndY { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build(Random rand)
        {
            throw new NotImplementedException();
        }
    }
}
