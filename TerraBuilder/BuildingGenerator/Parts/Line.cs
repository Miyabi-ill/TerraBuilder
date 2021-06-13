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
        public override int X { get; set; }

        /// <inheritdoc/>
        public override int Y { get; set; }

        public int EndX { get; set; }

        public int EndY { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build()
        {
            throw new NotImplementedException();
        }
    }
}
