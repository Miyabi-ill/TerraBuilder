namespace TUBGWorldGenerator.WorldGeneration
{
    using OTAPI.Tile;
    using Terraria;
    using Terraria.Localization;
    using Terraria.Map;

    /// <summary>
    /// ワールド生成に必要なテラリアのプロパティをひとまとめにしたクラス。
    /// </summary>
    public class WorldSandbox
    {
        private static bool isInitializedTerrariaInstance = false;

        /// <summary>
        /// コンストラクタ。
        /// もしテラリアのクラスが初期化されていなければ初期化し、
        /// プロパティを設定する。
        /// </summary>
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

        /// <summary>
        /// ワールドに存在するタイル
        /// </summary>
        public ITileCollection Tiles { get; }

        /// <summary>
        /// ワールドにあるチェスト
        /// </summary>
        public Chest[] Chests { get; }

        /// <summary>
        /// ワールドの横幅。
        /// </summary>
        public int TileCountX { get; }

        /// <summary>
        /// ワールドの縦。
        /// </summary>
        public int TileCountY { get; }
    }
}
