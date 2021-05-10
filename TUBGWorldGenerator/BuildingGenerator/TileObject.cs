namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using Terraria;

    public class TileObject : BuildBase
    {
        /// <inheritdoc/>
        public override int X { get; set; }

        /// <inheritdoc/>
        public override int Y { get; set; }

        public string ItemName { get; set; }

        public int TileID { get; set; }

        public int Style { get; set; }

        /// <inheritdoc/>
        public override Tile[,] Build()
        {
            throw new NotImplementedException();
        }
    }
}
