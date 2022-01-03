namespace TerraBuilder.WorldGeneration.Actions.Biomes
{
    using System;
    using System.ComponentModel;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// 地表の生成を行う.
    /// </summary>
    [Action]
    public class Surface : IWorldGenerationAction<Surface.SurfaceContext>
    {
        /// <summary>
        /// 地表生成のパターン.ランダムな区間ごとに選ばれ、変更される
        /// </summary>
        private enum SurfaceType : int
        {
            Flat = 0,
            Up,
            Down,
            StrongUp,
            StrongDown,
        }

        /// <inheritdoc/>
        public string Name => nameof(Surface);

        /// <inheritdoc/>
        public string Description => "地表生成を行う";

        /// <inheritdoc/>
        public SurfaceContext Context { get; set; } = new SurfaceContext();

        /// <inheritdoc/>
        public bool Run(WorldSandbox sandbox)
        {
            // Terraria.GameContent.Biomes.TerrainPassに近い生成を行う.
            GlobalContext globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;
            Random rand = globalContext.Random;

            int maxSurfaceLevel = Context.SurfaceLevel - Context.SurfaceMaxHeight;

            int nextTypeChangeSpan = rand.Next(5, 40);
            int currentPosition = rand.Next(maxSurfaceLevel, Context.SurfaceLevel);
            SurfaceType currentSurface = SurfaceType.Flat;
            for (int x = 0; x < sandbox.TileCountX; x++)
            {
                // 次の地表タイプを選定、区間も決定する
                if (nextTypeChangeSpan == 0)
                {
                    double currentPercentileY = (currentPosition - maxSurfaceLevel) / (double)Context.SurfaceLevel;
                    nextTypeChangeSpan = rand.Next(5, 40);

                    // 下に行き過ぎ
                    if (currentPercentileY > 0.9)
                    {
                        switch (rand.Next(0, 4))
                        {
                            case 0:
                                currentSurface = SurfaceType.Flat;
                                break;
                            case 1:
                                currentSurface = SurfaceType.Up;
                                break;
                            case 2:
                                currentSurface = SurfaceType.StrongUp;
                                break;
                            case 3:
                                currentSurface = SurfaceType.Down;
                                break;
                            default:
                                currentSurface = SurfaceType.Flat;
                                break;
                        }
                    }

                    // 上に行き過ぎ
                    else if (currentPercentileY < 0.1)
                    {
                        switch (rand.Next(0, 4))
                        {
                            case 0:
                                currentSurface = SurfaceType.Flat;
                                break;
                            case 1:
                                currentSurface = SurfaceType.Up;
                                break;
                            case 2:
                                currentSurface = SurfaceType.Down;
                                break;
                            case 3:
                                currentSurface = SurfaceType.StrongDown;
                                break;
                            default:
                                currentSurface = SurfaceType.Flat;
                                break;
                        }
                    }

                    // 標準
                    else
                    {
                        switch (rand.Next(0, 5))
                        {
                            case 0:
                                currentSurface = SurfaceType.Flat;
                                break;
                            case 1:
                                currentSurface = SurfaceType.Up;
                                break;
                            case 2:
                                currentSurface = SurfaceType.StrongUp;
                                break;
                            case 3:
                                currentSurface = SurfaceType.Down;
                                break;
                            case 4:
                                currentSurface = SurfaceType.StrongDown;
                                break;
                            default:
                                currentSurface = SurfaceType.Flat;
                                break;
                        }
                    }

                    // 平らな地表タイプなら、区間を伸ばす
                    if (currentSurface == SurfaceType.Flat)
                    {
                        nextTypeChangeSpan *= rand.Next(1, 5) * 5;
                    }
                }

                // 地表タイプに応じてタイルの上下を決定
                switch (currentSurface)
                {
                    case SurfaceType.Flat:
                        while (rand.Next(0, 7) == 0)
                        {
                            currentPosition += rand.Next(-1, 2);
                        }

                        break;
                    case SurfaceType.Up:
                        while (rand.Next(0, 4) == 0)
                        {
                            currentPosition--;
                        }

                        while (rand.Next(0, 10) == 0)
                        {
                            currentPosition++;
                        }

                        break;
                    case SurfaceType.StrongUp:
                        while (rand.Next(0, 2) == 0)
                        {
                            currentPosition--;
                        }

                        while (rand.Next(0, 6) == 0)
                        {
                            currentPosition++;
                        }

                        break;
                    case SurfaceType.Down:
                        while (rand.Next(0, 4) == 0)
                        {
                            currentPosition++;
                        }

                        while (rand.Next(0, 10) == 0)
                        {
                            currentPosition--;
                        }

                        break;
                    case SurfaceType.StrongDown:
                        while (rand.Next(0, 2) == 0)
                        {
                            currentPosition++;
                        }

                        while (rand.Next(0, 5) == 0)
                        {
                            currentPosition--;
                        }

                        break;
                }

                // 丸め込み
                currentPosition = Math.Max(currentPosition, maxSurfaceLevel);
                currentPosition = Math.Min(currentPosition, Context.SurfaceLevel);

                // タイル設置
                for (int y = currentPosition; y < Context.SurfaceLevel; y++)
                {
                    sandbox.Tiles[x, y] = new Tile()
                    {
                        type = TileID.Dirt,
                    };

                    sandbox.Tiles[x, y].active(true);
                }
            }

            return true;
        }

        /// <summary>
        /// 地表生成に使われるコンテキスト.
        /// </summary>
        public class SurfaceContext : ActionContext
        {
            /// <summary>
            /// 地表の高さ.この高さ以上(数値的には以下)が地表.
            /// </summary>
            [Browsable(false)]
            public int SurfaceLevel => WorldGenerationRunner.CurrentRunner.GlobalContext.SurfaceLevel;

            /// <summary>
            /// 地表生成を行う最大の高さ.
            /// <see cref="SurfaceLevel"/>からこの値を減算した高さが実際に地表生成を行う最大の高さになる
            /// </summary>
            [Category("地表生成")]
            [DisplayName("地表最大高さ")]
            [Description("地表生成を行う最大の高さ.地表レベルからのブロック数.")]
            public int SurfaceMaxHeight { get; set; } = 50;
        }
    }
}
