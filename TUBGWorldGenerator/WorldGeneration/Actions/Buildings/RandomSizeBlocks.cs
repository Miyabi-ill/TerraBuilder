namespace TUBGWorldGenerator.WorldGeneration.Actions.Buildings
{
    using Terraria;
    using Terraria.ID;
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// ランダムなサイズのブロックをワールド地表に設置するクラス
    /// </summary>
    public class RandomSizeBlocks : IWorldGenerationAction<RandomSizeBlocks.RandomSizeBlockContext>
    {
        public enum PlaceBiome
        {
            Surface,
            Cavern,
        }

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
        public string Description => "ランダムな大きさ、形のブロックを配置する";

        /// <inheritdoc/>
        public RandomSizeBlockContext Context { get; } = new RandomSizeBlockContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            var random = globalContext.Random;

            if (Context.PlaceBiome == PlaceBiome.Surface)
            {
                for (int c = 0; c < Context.BlockCount; c++)
                {
                    for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                    {
                        int x = random.Next(100, sandbox.TileCountX - 100);
                        bool success = PlaceBlockToSurface(
                            sandbox,
                            sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                            sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                            x);
                        if (success)
                        {
                            break;
                        }
                    }
                }
            }
            else if (Context.PlaceBiome == PlaceBiome.Cavern)
            {
                double[] cavernTop = (double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernTop"];
                double[] cavernBottom = (double[])WorldGenerationRunner.CurrentRunner.GlobalContext["CavernBottom"];

                for (int c = 0; c < Context.BlockCount; c++)
                {
                    for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                    {
                        int x = random.Next(100, sandbox.TileCountX - 100);
                        int y = random.Next((int)cavernTop[x], (int)cavernBottom[x]);
                        bool placeToTop = random.Next(2) == 0 ? true : false;
                        bool success = PlaceBlockToCavern(
                            sandbox,
                            sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                            sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                            x,
                            y,
                            placeToTop);
                        if (success)
                        {
                            break;
                        }
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

        private bool PlaceBlockToCavern(WorldSandbox sandbox, int sizeX, int sizeY, int x, int y, bool placeToTop = false)
        {
            if (sandbox.Tiles[x, y]?.active() == true)
            {
                return false;
            }

            if (placeToTop)
            {
                for (int cy = y; cy > 0; cy--)
                {
                    if (sandbox.Tiles[x, cy]?.active() == true)
                    {
                        return PlaceBlock(sandbox, sizeX, sizeY, x - (sizeX / 2), cy);
                    }
                }

                return false;
            }
            else
            {
                for (int cy = y; cy < sandbox.TileCountY; cy++)
                {
                    if (sandbox.Tiles[x, cy]?.active() == true)
                    {
                        return PlaceBlock(sandbox, sizeX, sizeY, x - (sizeX / 2), cy - sizeY);
                    }
                }

                return false;
            }
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
                        sandbox.Tiles[px, py] = new Tile() { wall = WallID.Wood };
                    }
                }
            }

            if (!string.IsNullOrEmpty(Context.ChestGroupName) && random.NextDouble() < Context.ChestProbably)
            {
                var chestContext = GenerateChest.GetChestContextByRandom(random, Context.ChestGroupName);
                if (chestContext != null)
                {
                    int chestX = random.Next(x + 1, x + sizeX - 2);
                    GenerateChest.PlaceChest(sandbox, chestX, y + sizeY - 3, random, chestContext);
                }
            }

            if (random.NextDouble() < Context.TorchProbably)
            {
                for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                {
                    int tx = random.Next(x + 1, x + sizeX - 1);
                    int ty = random.Next(y + 1, y + sizeY - 1);
                    if (!sandbox.Tiles[tx, ty].active())
                    {
                        sandbox.Tiles[tx, ty].type = TileID.Torches;
                        sandbox.Tiles[tx, ty].frameX = 0;
                        sandbox.Tiles[tx, ty].frameY = 0;
                        break;
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

            public double ChestProbably { get; set; } = 0.3;

            public string ChestGroupName { get; set; }

            public PlaceBiome PlaceBiome { get; set; } = PlaceBiome.Surface;

            public double TorchProbably { get; set; } = 0.08;
        }
    }
}
