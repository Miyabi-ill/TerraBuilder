namespace TUBGWorldGenerator.Utils
{
    using Terraria;
    using TUBGWorldGenerator.WorldGeneration;

    public class Liquids
    {
        public static void Settle(WorldSandbox sandbox)
        {
            Liquid.worldGenTilesIgnoreWater(ignoreSolids: true);
            Liquid.QuickWater(3);
            WorldGen.WaterCheck();
            int counter = 0;
            Liquid.quickSettle = true;
            while (counter < 10)
            {
                counter++;
                while (Liquid.numLiquid > 0)
                {
                    Liquid.UpdateLiquid();
                }

                WorldGen.WaterCheck();
            }

            Liquid.quickSettle = false;
            Liquid.worldGenTilesIgnoreWater(ignoreSolids: false);
            Main.tileSolid[484] = false;
        }
    }
}
