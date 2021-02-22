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

            Reset();
        }

        /// <summary>
        /// ワールドに存在するタイル
        /// </summary>
        public ITileCollection Tiles { get; private set; }

        /// <summary>
        /// ワールドにあるチェスト
        /// </summary>
        public Chest[] Chests { get; private set; }

        /// <summary>
        /// ワールドの横幅。
        /// </summary>
        public int TileCountX { get; private set; }

        /// <summary>
        /// ワールドの縦。
        /// </summary>
        public int TileCountY { get; private set; }

        /// <summary>
        /// プロパティをリセットして読み込みなおす
        /// </summary>
        public void Reset()
        {
            TileCountX = 4200;
            TileCountY = 1200;
            Main.treeX[0] = TileCountX;
            Main.treeX[1] = TileCountX;
            Main.treeX[2] = TileCountX;

            Tiles = Main.tile;
            Chests = Main.chest;
        }
    }
}
