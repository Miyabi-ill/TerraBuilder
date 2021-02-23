namespace TUBGWorldGenerator.WorldGeneration.Actions.Biomes
{
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// スポーン地点を作成する。
    /// </summary>
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
            sandbox.SpawnTileX = sandbox.TileCountX / 2;
            sandbox.SpawnTileY = Context.SpawnLevel;

            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                sandbox.Tiles[x, Context.SpawnLevel] = new Tile()
                {
                    type = TileID.Titanium,
                };

                sandbox.Tiles[x, Context.SpawnLevel].active(true);
            }

            return true;
        }

        /// <summary>
        /// スポーン地点の生成に使われるコンテキスト。
        /// </summary>
        public class SpawnAreaContext : ActionContext
        {
            /// <summary>
            /// スポーン地点の高さ
            /// </summary>
            public int SpawnLevel { get; set; } = 100;
        }
    }
}
