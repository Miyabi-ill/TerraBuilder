namespace TUBGWorldGenerator.WorldGeneration
{
    using OTAPI.Tile;
    using Terraria;
    using Terraria.Localization;
    using Terraria.Map;

    public class WorldSandbox
    {
        private static bool isInitializedTerrariaInstance = false;

        public ITileCollection Tiles { get; }

        public Chest[] Chests { get; }

        public int TileCountX { get; }

        public int TileCountY { get; }

        public WorldSandbox()
        {
            if (!isInitializedTerrariaInstance)
            {
                // Initialize Terraria Instance.
                MapHelper.Initialize();
                Lang.InitializeLegacyLocalization();
                LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);

                _ = new Main();
                Main.instance.Initialize();

                isInitializedTerrariaInstance = true;
            }

            // Setup Properties
            Tiles = Main.tile;
            Chests = Main.chest;

            TileCountX = Main.maxTilesX;
            TileCountY = Main.maxTilesY;
        }
    }
}
