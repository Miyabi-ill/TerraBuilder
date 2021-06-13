namespace TerraBuilder.WorldGeneration.Actions.Buildings
{
    using System.ComponentModel;
    using Terraria;
    using Terraria.ID;
    using TerraBuilder.Utils;
    using TerraBuilder.WorldGeneration;

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
        public virtual string Name => nameof(RandomSizeBlocks);

        /// <inheritdoc/>
        public virtual string Description => "ランダムな大きさ、形のブロックを配置する";

        /// <inheritdoc/>
        public virtual RandomSizeBlockContext Context { get; } = new RandomSizeBlockContext();

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
                            Context,
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
                        bool placeToTop = random.Next(2) == 0;
                        int? maxY = placeToTop ? (int?)((int)cavernTop[x] - 2) : null;
                        bool success = PlaceBlockToCavern(
                            sandbox,
                            Context,
                            sizeX: random.Next(Context.BlockMinX, Context.BlockMaxX),
                            sizeY: random.Next(Context.BlockMinY, Context.BlockMaxY),
                            x,
                            y,
                            placeToTop,
                            maxY);
                        if (success)
                        {
                            break;
                        }
                    }
                }
            }

            return true;
        }

        public static bool PlaceBlockToSurface(WorldSandbox sandbox, RandomSizeBlockContext context, int sizeX, int sizeY, int x)
        {
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            for (int y = 0; y < globalContext.SurfaceLevel; y++)
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
                    if (y - sizeY >= globalContext.SurfaceLevel - context.MaxHeightFromSurface)
                    {
                        int py = y - sizeY;
                        py = isStacked ? py + 1 : py;
                        return PlaceBlock(sandbox, context, sizeX, sizeY, x, py);
                    }

                    break;
                }
            }

            return false;
        }

        public static bool PlaceBlockToCavern(WorldSandbox sandbox, RandomSizeBlockContext context, int sizeX, int sizeY, int x, int y, bool placeToTop = false, int? maxTileY = null)
        {
            if (!maxTileY.HasValue)
            {
                maxTileY = placeToTop ? 0 : sandbox.TileCountY;
            }

            for (int cx = x; cx < x + sizeX; cx++)
            {
                for (int cy = y; cy < y + sizeY; cy++)
                {
                    if (sandbox.Tiles[cx, cy]?.active() == true || (sandbox.Tiles[cx, cy]?.wall != 0 && sandbox.Tiles[cx, cy]?.wall != WallID.GrayBrick))
                    {
                        return false;
                    }
                }
            }

            if (placeToTop)
            {
                for (int cy = y; cy > maxTileY; cy--)
                {
                    bool foundBuildingBlock = false;
                    for (int cx = x; cx < x + sizeX; cx++)
                    {
                        if (sandbox.Tiles[cx, cy] != null && (sandbox.Tiles[cx, cy].type == TileID.WoodBlock || sandbox.Tiles[cx, cy].type == TileID.Platforms))
                        {
                            foundBuildingBlock = true;
                            break;
                        }
                    }

                    if (sandbox.Tiles[x, cy]?.active() == true || foundBuildingBlock)
                    {
                        return PlaceBlock(sandbox, context, sizeX, sizeY, x - (sizeX / 2), cy);
                    }
                }

                return false;
            }
            else
            {
                for (int cy = y; cy < maxTileY; cy++)
                {
                    bool foundBuildingBlock = false;
                    for (int cx = x; cx < x + sizeX; cx++)
                    {
                        if (sandbox.Tiles[cx, cy] != null && (sandbox.Tiles[cx, cy].type == TileID.WoodBlock || sandbox.Tiles[cx, cy].type == TileID.Platforms))
                        {
                            foundBuildingBlock = true;
                            break;
                        }
                    }

                    if (sandbox.Tiles[x, cy]?.active() == true || foundBuildingBlock)
                    {
                        return PlaceBlock(sandbox, context, sizeX, sizeY, x - (sizeX / 2), cy - sizeY);
                    }
                }

                return false;
            }
        }

        public static bool PlaceBlock(WorldSandbox sandbox, RandomSizeBlockContext context, int sizeX, int sizeY, int x, int y)
        {
            var random = WorldGenerationRunner.CurrentRunner.GlobalContext.Random;

            Tile[,] tiles = new Tile[sizeX, sizeY];
            TileProtectionMap.TileProtectionType[,] tileProtectionTypes = new TileProtectionMap.TileProtectionType[sizeX, sizeY];

            ActiveWallDirection randomWallDir = (ActiveWallDirection)random.Next(0, 4);
            for (int px = 0; px < sizeX; px++)
            {
                for (int py = 0; py < sizeY; py++)
                {
                    if (px == 0)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Left ? TileID.WoodBlock : TileID.Platforms;
                        tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        tiles[px, py].active(true);
                        tileProtectionTypes[px, py] = TileProtectionMap.TileProtectionType.TopSolid;
                    }
                    else if (px == sizeX - 1)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Right ? TileID.WoodBlock : TileID.Platforms;
                        tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        tiles[px, py].active(true);
                        tileProtectionTypes[px, py] = TileProtectionMap.TileProtectionType.TopSolid;
                    }
                    else if (py == 0)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Top ? TileID.WoodBlock : TileID.Platforms;
                        tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        tiles[px, py].active(true);
                        tileProtectionTypes[px, py] = TileProtectionMap.TileProtectionType.TopSolid;
                    }
                    else if (py == sizeY - 1)
                    {
                        ushort tileType = randomWallDir == ActiveWallDirection.Bottom ? TileID.WoodBlock : TileID.Platforms;
                        tiles[px, py] = new Tile()
                        {
                            type = tileType,
                        };
                        tiles[px, py].active(true);
                        tileProtectionTypes[px, py] = TileProtectionMap.TileProtectionType.TopSolid;
                    }
                    else
                    {
                        tiles[px, py] = new Tile() { wall = WallID.Wood };
                        tileProtectionTypes[px, py] = TileProtectionMap.TileProtectionType.None;
                    }
                }
            }

            sandbox.TileProtectionMap.PlaceTiles(sandbox, tileProtectionTypes, tiles, x, y);

            if (!string.IsNullOrEmpty(context.ChestGroupName) && random.NextDouble() < context.ChestProbably)
            {
                var chestContext = GenerateChest.GetChestContextByRandom(random, context.ChestGroupName);
                if (chestContext != null)
                {
                    int chestX = random.Next(x + 1, x + sizeX - 2);
                    bool success = GenerateChest.PlaceChest(sandbox, chestX, y + sizeY - 3, random, chestContext);
                    if (success)
                    {
                        sandbox.TileProtectionMap.SetProtection(TileProtectionMap.TileProtectionType.All, chestX, y + sizeY - 3, chestX + 1, y + sizeY - 2);
                    }
                }
            }

            if (random.NextDouble() < context.TorchProbably)
            {
                for (int retry = 0; retry < context.MaxPlaceRetry; retry++)
                {
                    int tx = random.Next(x + 1, x + sizeX - 1);
                    int ty = random.Next(y + 1, y + sizeY - 1);
                    if (!sandbox.Tiles[tx, ty].active())
                    {
                        WorldGen.PlaceTile(tx, ty, TileID.Torches);
                        break;
                    }
                }
            }

            return true;
        }

        public class RandomSizeBlockContext : ActionContext
        {
            [Category("部屋")]
            [DisplayName("部屋の最小幅(壁含む)")]
            [Description("部屋の最小の幅。壁を含んでいるので、通り抜け可能な最小の幅は4ブロック。")]
            public int BlockMinX { get; set; } = 5;

            [Category("部屋")]
            [DisplayName("部屋の最大幅(壁含む)")]
            [Description("部屋の最大の幅。壁を含んでいるので、通り抜け可能な最小の幅は4ブロック。")]
            public int BlockMaxX { get; set; } = 15;

            [Category("部屋")]
            [DisplayName("部屋の最小高さ(壁含む)")]
            [Description("部屋の最小の高さ。壁を含んでいるので、通り抜け可能な最小の高さは5ブロック。")]
            public int BlockMinY { get; set; } = 5;

            [Category("部屋")]
            [DisplayName("部屋の最大高さ(壁含む)")]
            [Description("部屋の最大の高さ。壁を含んでいるので、通り抜け可能な最小の高さは5ブロック。")]
            public int BlockMaxY { get; set; } = 10;

            [Category("部屋設置")]
            [DisplayName("部屋の個数")]
            [Description("設置する部屋の個数")]
            public int BlockCount { get; set; } = 200;

            [Category("部屋設置")]
            [DisplayName("部屋設置最大リトライ数")]
            [Description("設置する部屋の設置に失敗したときの最大リトライ数。設置に失敗するケース=既に建造物がある場合など")]
            public int MaxPlaceRetry { get; set; } = 100;

            [Category("部屋設置")]
            [DisplayName("部屋設置最大高さ")]
            [Description("部屋の設置を最大で地上からどこまでの高さに制限するか。初期地点から建物が見えることを防ぐ。")]
            public int MaxHeightFromSurface { get; set; } = 100;

            [Category("部屋設置")]
            [DisplayName("チェスト生成確率")]
            [Description("部屋の中にチェストを設置する確率。十分なスペースがなければ設置されない。")]
            public double ChestProbably { get; set; } = 0.3;

            [Category("部屋設置")]
            [DisplayName("チェストグループ名")]
            [Description("部屋の中にチェストを設置する場合に使われるチェストグループ名。")]
            public string ChestGroupName { get; set; }

            [Category("部屋設置")]
            [DisplayName("部屋設置場所")]
            [Description("部屋を設置する場所(バイオーム)。地下、地上など。")]
            public PlaceBiome PlaceBiome { get; set; } = PlaceBiome.Surface;

            [Category("部屋設置")]
            [DisplayName("松明設置確率")]
            [Description("部屋の中に松明を設置する確率。")]
            public double TorchProbably { get; set; } = 0.08;
        }
    }
}
