namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Terraria;

    public class Line : BuildBase
    {
        /// <inheritdoc/>
        public override RandomValue<int> X { get; set; }

        /// <inheritdoc/>
        public override RandomValue<int> Y { get; set; }

        public RandomValue<int> EndX { get; set; }

        public RandomValue<int> EndY { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build(Random rand)
        {
            throw new NotImplementedException();
        }
    }
}
