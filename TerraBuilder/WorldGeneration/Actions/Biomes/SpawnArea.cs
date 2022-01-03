namespace TerraBuilder.WorldGeneration.Actions.Biomes
{
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// スポーン地点を作成する.
    /// </summary>
    [Action]
    public class SpawnArea : IWorldGenerationAction<SpawnArea.SpawnAreaContext>
    {
        /// <inheritdoc/>
        public string Name => nameof(SpawnArea);

        /// <inheritdoc/>
        public string Description => "スポーン地点の作成";

        /// <inheritdoc/>
        public SpawnAreaContext Context { get; set; } = new SpawnAreaContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            var globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;

            sandbox.SpawnTileX = sandbox.TileCountX / 2;
            sandbox.SpawnTileY = globalContext.RespawnLevel;

            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                var asphalt = new Tile()
                {
                    type = TileID.Asphalt,
                };
                asphalt.active(true);
                asphalt.wire(true);
                asphalt.actuator(true);

                sandbox.Tiles[x, globalContext.RespawnLevel] = asphalt;

                var nodestroy = new Tile()
                {
                    type = TileID.Titanium,
                };
                nodestroy.active(true);
                nodestroy.wire(true);
                nodestroy.actuator(true);

                sandbox.Tiles[x, globalContext.RespawnLevel + 1] = nodestroy;

                sandbox.TileProtectionMap[x, globalContext.RespawnLevel] = TileProtectionMap.TileProtectionType.All;
                sandbox.TileProtectionMap[x, globalContext.RespawnLevel + 1] = TileProtectionMap.TileProtectionType.All;
            }

            sandbox.Tiles[5, globalContext.RespawnLevel - 1] = new Tile() { wall = WallID.Wood };
            WorldGen.PlaceTile(5, globalContext.RespawnLevel - 1, TileID.Switches, forced: true);
            sandbox.Tiles[5, globalContext.RespawnLevel - 1].wire(true);

            return true;
        }

        /// <summary>
        /// スポーン地点の生成に使われるコンテキスト.
        /// </summary>
        public class SpawnAreaContext : ActionConfig
        {
        }
    }
}
