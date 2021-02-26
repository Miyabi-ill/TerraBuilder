namespace TUBGWorldGenerator.WorldGeneration.Actions.Buildings
{
    using System.Collections.Generic;
    using Terraria;
    using Terraria.DataStructures;
    using Terraria.ID;
    using TUBGWorldGenerator.Utils;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// ランダムなサイズのブロックをワールド地表に設置するクラス
    /// </summary>
    public class RandomSizeBlockWithArea : IWorldGenerationAction<RandomSizeBlockWithArea.RandomSizeBlockWithAreaContext>
    {
        private enum ActiveWallDirection : int
        {
            Top = 0,
            Left,
            Bottom,
            Right,
        }

        /// <inheritdoc/>
        public string Name => nameof(RandomSizeBlockWithArea);

        /// <inheritdoc/>
        public string Description => "ランダムな大きさ、形のブロックをワールド地表に配置する";

        /// <inheritdoc/>
        public RandomSizeBlockWithAreaContext Context { get; } = new RandomSizeBlockWithAreaContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            var random = globalContext.Random;
            var area = new List<Point16>();
            for (int i = 0; i < Context.AreaCount; i++)
            {
                int width = random.Next(Context.AreaMinWidth, Context.AreaMaxWidth);
                int areaLeft = 0;
                bool success = false;
                for (int r = 0; r < Context.MaxAreaSelectRetry; r++)
                {
                    areaLeft = random.Next(100, sandbox.TileCountX - 100 - width);
                    success = true;
                    foreach (Point16 areaPos in area)
                    {
                        if (areaPos.Y < areaLeft - Context.MinDistanceFromNearbyArea
                            || areaLeft + width + Context.MinDistanceFromNearbyArea < areaPos.X)
                        {
                            // 重なっていない
                        }
                        else
                        {
                            // 重なっている
                            success = false;
                        }
                    }

                    if (success)
                    {
                        break;
                    }
                }

                if (success)
                {
                    area.Add(new Point16(areaLeft, areaLeft + width));
                    for (int c = 0; c < Context.BlockCount; c++)
                    {
                        for (int retry = 0; retry < Context.MaxPlaceRetry; retry++)
                        {
                            int x = random.Next(areaLeft, areaLeft + width);
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

            if (random.NextDouble() < Context.ChestProbability)
            {
                int chestX = random.Next(x + 1, x + sizeX - 1);
                int chestIndex = WorldGen.PlaceChest(chestX, y + sizeY - 2);
                if (chestIndex != -1)
                {
                    int frame = sandbox.Chests[chestIndex].frame;
                    sandbox.Chests[chestIndex] = GenerateChest.GenerateChestByRandom(random, new Chests.ChestContext());
                    sandbox.Chests[chestIndex].x = chestX;
                    sandbox.Chests[chestIndex].y = y + sizeY - 2;
                    sandbox.Chests[chestIndex].frame = frame;
                }
            }

            return true;
        }

        public class RandomSizeBlockWithAreaContext : RandomSizeBlocks.RandomSizeBlockContext
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public RandomSizeBlockWithAreaContext()
            {
                BlockCount = 100;
                ChestProbability = 0.03;
            }

            public int AreaCount { get; set; } = 5;

            public int MinDistanceFromNearbyArea { get; set; } = 200;

            public int AreaMinWidth { get; set; } = 30;

            public int AreaMaxWidth { get; set; } = 150;

            public int MaxAreaSelectRetry { get; set; } = 10;
        }
    }
}
