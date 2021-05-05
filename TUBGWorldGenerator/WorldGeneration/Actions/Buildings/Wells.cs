namespace TUBGWorldGenerator.WorldGeneration.Actions.Buildings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Terraria;
    using Terraria.ID;

    public class Wells : IWorldGenerationAction<Wells.WellContext>
    {
        public string Name => nameof(Wells);

        public string Description => "井戸を生成する。";

        public WellContext Context { get; private set; } = new WellContext();

        public bool Run(WorldSandbox sandbox)
        {
            var globalContext = WorldGenerationRunner.CurrentRunner.GlobalContext;

            double[] cavernTop = (double[])globalContext["CavernTop"];
            List<int> placedWellX = new List<int>();
            for (int count = 0; count < Context.Count; count++)
            {
                for (int retry = 0; retry < Context.MaxRetryPerWell; retry++)
                {
                    int x = globalContext.Random.Next(100, sandbox.TileCountX - 100);
                    bool check = true;
                    bool placeSuccess = false;
                    foreach (int wx in placedWellX)
                    {
                        if (wx - Context.MinDistanceFromNearbyWells <= x && x <= wx + Context.MinDistanceFromNearbyWells)
                        {
                            check = false;
                            break;
                        }
                    }

                    if (check)
                    {
                        for (int y = 0; y < sandbox.TileCountY; y++)
                        {
                            if (sandbox.Tiles[x, y]?.active() == true)
                            {
                                placeSuccess = PlaceWell(sandbox, x, y, (int)cavernTop[x], placeRopeToMaximum: true);
                                break;
                            }
                        }
                    }

                    if (placeSuccess)
                    {
                        placedWellX.Add(x);
                        break;
                    }
                }
            }

            return true;
        }

        public bool PlaceWell(WorldSandbox sandbox, int x, int y, int wellEndY, bool placeRopeToMaximum = true)
        {
            if (x < 5 || x > sandbox.TileCountX - 5)
            {
                return false;
            }

            int currentY = y;
            while (true)
            {
                if (currentY <= wellEndY)
                {
                    sandbox.Tiles[x - 4, currentY] = new Tile() { type = TileID.GrayBrick };
                    sandbox.Tiles[x - 4, currentY].active(true);
                    sandbox.Tiles[x - 3, currentY] = new Tile() { type = TileID.GrayBrick };
                    sandbox.Tiles[x - 3, currentY].active(true);
                    sandbox.Tiles[x - 2, currentY]?.active(false);
                    sandbox.Tiles[x - 1, currentY]?.active(false);
                    sandbox.Tiles[x, currentY] = new Tile() { type = TileID.Rope };
                    sandbox.Tiles[x, currentY].active(true);
                    sandbox.Tiles[x + 1, currentY]?.active(false);
                    sandbox.Tiles[x + 2, currentY]?.active(false);
                    sandbox.Tiles[x + 3, currentY] = new Tile() { type = TileID.GrayBrick };
                    sandbox.Tiles[x + 3, currentY].active(true);
                    sandbox.Tiles[x + 4, currentY] = new Tile() { type = TileID.GrayBrick };
                    sandbox.Tiles[x + 4, currentY].active(true);

                    for (int i = -4; i < 5; i++)
                    {
                        if (sandbox.Tiles[x + i, currentY] == null)
                        {
                            sandbox.Tiles[x + i, currentY] = new Tile();
                        }

                        sandbox.Tiles[x + i, currentY].wall = WallID.GrayBrick;
                    }
                }
                else
                {
                    if (sandbox.Tiles[x, currentY]?.active() == true)
                    {
                        break;
                    }
                    else
                    {
                        if (sandbox.Tiles[x, currentY] == null)
                        {
                            sandbox.Tiles[x, currentY] = new Tile();
                        }

                        sandbox.Tiles[x, currentY].type = TileID.Rope;
                        sandbox.Tiles[x, currentY].active(true);
                    }
                }

                currentY++;
            }

            return true;
        }

        public class WellContext : ActionContext
        {
            [Category("井戸設置")]
            [DisplayName("井戸設置数")]
            [Description("井戸を設置する個数")]
            public int Count { get; set; } = 20;

            [Category("井戸設置")]
            [DisplayName("井戸設置リトライ数")]
            [Description("井戸を設置する際にリトライする回数。設置に失敗するケース=近くの井戸からの最小距離が大きすぎるなど")]
            public int MaxRetryPerWell { get; set; } = 100;

            [Category("井戸設置")]
            [DisplayName("近くの井戸からの最小距離")]
            [Description("近くの井戸から最小でも離れている距離。この距離以内には井戸は生成されない。")]
            public int MinDistanceFromNearbyWells { get; set; } = 30;
        }
    }
}
