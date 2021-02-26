namespace TUBGWorldGenerator.WorldGeneration.Actions.Buildings
{
    using Terraria;
    using Terraria.ID;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// ランダムなサイズのブロックをワールド地表に設置するクラス
    /// </summary>
    public class RandomSizeBlocks : IWorldGenerationAction<RandomSizeBlocks.RandomSizeBlockContext>
    {
        private enum ActiveWallDirection : int
        {
            Top = 0,
            Left,
            Bottom,
            Right,
        }

        /// <inheritdoc/>
        public string Name => nameof(RandomSizeBlocks);

        /// <inheritdoc/>
        public string Description => "ランダムな大きさ、形のブロックをワールド地表に配置する";

        /// <inheritdoc/>
        public RandomSizeBlockContext Context { get; } = new RandomSizeBlockContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            var random = globalContext.Random;
            for (int c = 0; c < Context.BlockCount; c++)
            {
                for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                {
                    int x = random.Next(100, sandbox.TileCountX - 100);
                    bool result = PlaceBlockToSurface(
                        sandbox,
                        sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                        sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                        x);
                    if (result)
                    {
                        break;
                    }
                }
            }

            return true;
        }

        private bool PlaceBlockToSurface(WorldSandbox sandbox, int sizeX, int sizeY, int x)
        {
            GlobalContext context = WorldGenerationRunner.CurrentRunner.GlobalContext;
            for (int y = 0; y < context.SurfaceLevel; y++)
            {
                bool foundActiveTile = false;
                bool isStacked = false;
                for (int cx = x; cx < x + sizeX; cx++)
                {
                    if (sandbox.Tiles[cx, y]?.active() == true)
                    {
                        isStacked = sandbox.Tiles[cx, y].type == TileID.WoodBlock || sandbox.Tiles[cx, y].type == TileID.Platforms;
                        foundActiveTile = true;
                        break;
                    }
                }

                if (foundActiveTile)
                {
                    if (y - sizeY >= context.SurfaceLevel - Context.MaxHeightFromSurface)
                    {
                        int py = y - sizeY;
                        py = isStacked ? py + 1 : py;
                        return PlaceBlock(sandbox, sizeX, sizeY, x, py);
                    }

                    break;
                }
            }

            return false;
        }

        private bool PlaceBlock(WorldSandbox sandbox, int sizeX, int sizeY, int x, int y)
        {
            var random = WorldGenerationRunner.CurrentRunner.GlobalContext.Random;
            ActiveWallDirection randomWallDir = (ActiveWallDirection)random.Next(0, 4);
            for (int px = x; px < x + sizeX; px++)
            {
                for (int py = y; py < y + sizeY; py++)
                {
                    if (px == x)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Left ? TileID.WoodBlock : TileID.Platforms;
                        sandbox.Tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        sandbox.Tiles[px, py].active(true);
                        WorldGen.SquareTileFrame(px, py);
                    }
                    else if (px == x + sizeX - 1)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Right ? TileID.WoodBlock : TileID.Platforms;
                        sandbox.Tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        sandbox.Tiles[px, py].active(true);
                        WorldGen.SquareTileFrame(px, py);
                    }
                    else if (py == y)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Top ? TileID.WoodBlock : TileID.Platforms;
                        sandbox.Tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        sandbox.Tiles[px, py].active(true);
                        WorldGen.SquareTileFrame(px, py);
                    }
                    else if (py == y + sizeY - 1)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Bottom ? TileID.WoodBlock : TileID.Platforms;
                        sandbox.Tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        sandbox.Tiles[px, py].active(true);
                        WorldGen.SquareTileFrame(px, py);
                    }
                    else
                    {
                        if (sandbox.Tiles[px, py] == null)
                        {
                            sandbox.Tiles[px, py] = new Tile();
                        }

                        sandbox.Tiles[px, py].wall = WallID.Wood;
                    }
                }
            }

            return true;
        }

        public class RandomSizeBlockContext : ActionContext
        {
            public int BlockMinX { get; set; } = 5;

            public int BlockMaxX { get; set; } = 15;

            public int BlockMinY { get; set; } = 5;

            public int BlockMaxY { get; set; } = 10;

            public int BlockCount { get; set; } = 200;

            public int MaxPlaceRetry { get; set; } = 100;

            public int MaxHeightFromSurface { get; set; } = 100;
        }
    }
}
