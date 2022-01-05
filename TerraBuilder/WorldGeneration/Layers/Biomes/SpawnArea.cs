namespace TerraBuilder.WorldGeneration.Layers.Biomes
{
    using System.Collections.Generic;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// スポーン地点を作成する.
    /// </summary>
    [Action]
    public class SpawnArea : IWorldGenerationLayer<SpawnArea.SpawnAreaContext>
    {
        /// <inheritdoc/>
        public string Name => nameof(SpawnArea);

        /// <inheritdoc/>
        public string Description => "スポーン地点の作成";

        /// <inheritdoc/>
        public SpawnAreaContext Config { get; set; } = new SpawnAreaContext();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            var globalContext = runner.GlobalConfig;

            sandbox.SpawnTileX = sandbox.TileCountX / 2;
            sandbox.SpawnTileY = globalContext.RespawnLevel;

            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                Tile asphalt = new Tile()
                {
                    type = TileID.Asphalt,
                };
                asphalt.active(true);
                asphalt.wire(true);
                asphalt.actuator(true);

                Coordinate asphaltCoord = new Coordinate(x, globalContext.RespawnLevel);
                _ = sandbox.PlaceTile(asphaltCoord, asphalt);

                Tile nodestroy = new Tile()
                {
                    type = TileID.Titanium,
                };
                nodestroy.active(true);
                nodestroy.wire(true);
                nodestroy.actuator(true);

                Coordinate nodestroyCoord = new Coordinate(x, globalContext.RespawnLevel + 1);
                _ = sandbox.PlaceTile(nodestroyCoord, nodestroy);

                sandbox.TileProtectionMap.AddProtection(asphaltCoord, TileProtectionMap.TileProtectionType.All);
                sandbox.TileProtectionMap.AddProtection(nodestroyCoord, TileProtectionMap.TileProtectionType.All);
            }

            Coordinate switchCoord = new Coordinate(5, globalContext.RespawnLevel - 1);
            Tile switchTile = new Tile() { wall = WallID.Wood };
            switchTile.wire(true);
            _ = sandbox.PlaceTile(switchCoord, switchTile);

            // TODO: sandboxへの呼び出し？
            WorldGen.PlaceTile(5, globalContext.RespawnLevel - 1, TileID.Switches, forced: true);

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        /// <summary>
        /// スポーン地点の生成に使われるコンテキスト.
        /// </summary>
        public class SpawnAreaContext : LayerConfig
        {
        }
    }
}
