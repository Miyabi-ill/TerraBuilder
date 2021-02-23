namespace TUBGWorldGenerator.WorldGeneration
{
    using System;
    using System.IO;
    using OTAPI.Tile;
    using Terraria;
    using Terraria.IO;
    using Terraria.Localization;
    using Terraria.Map;

    /// <summary>
    /// ワールド生成に必要なテラリアのプロパティをひとまとめにしたクラス。
    /// </summary>
    public class WorldSandbox
    {
        private static bool isInitializedTerrariaInstance = false;

        private int tileCountX;
        private int tileCountY;

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
        public int TileCountX
        {
            get => tileCountX;
            set
            {
                tileCountX = value;
                Main.maxTilesX = value;
            }
        }

        /// <summary>
        /// ワールドの縦。
        /// </summary>
        public int TileCountY
        {
            get => tileCountY;
            set
            {
                tileCountY = value;
                Main.maxTilesY = value;
            }
        }

        /// <summary>
        /// プロパティをリセットして読み込みなおす
        /// </summary>
        public void Reset()
        {
            TileCountX = 4200;
            TileCountY = 1200;

            Tiles = Main.tile;
            Chests = Main.chest;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                Main.npc[i] = new NPC();
            }
        }

        /// <summary>
        /// ワールドを保存する。
        /// </summary>
        /// <param name="path">保存先のパス。指定しない場合はテラリアのワールドフォルダに保存される</param>
        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                string fileName = DateTime.Now.ToString("yy-MM-dd HH-mm-ss") + ".wld";
                path = Path.Combine(Main.SavePath, "Worlds", fileName);
            }

            for (int x = 0; x < TileCountX; x++)
            {
                for (int y = 0; y < TileCountY; y++)
                {
                    if (Main.tile[x, y] == null)
                    {
                        Main.tile[x, y] = new Tile();
                    }
                }
            }

            Main.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
            Main.ActiveWorldFileData = new WorldFileData(path, false)
            {
                CreationTime = DateTime.Now,
                UniqueId = Guid.NewGuid(),
                WorldGeneratorVersion = 0UL,
                Metadata = Main.WorldFileMetadata,
            };

            Main.ActiveWorldFileData.SetSeed("42");
            Main.worldID = 42;

            Main.treeX[0] = TileCountX;
            Main.treeX[1] = TileCountX;
            Main.treeX[2] = TileCountX;

            Main.caveBackX[0] = TileCountX;
            Main.caveBackX[1] = TileCountX;
            Main.caveBackX[2] = TileCountX;

            Main.worldSurface = WorldGenerationRunner.CurrentRunner.GlobalContext.SurfaceLevel;
            Main.rockLayer = 800;

            Main.worldName = "TUBG";

            Main.spawnTileX = TileCountX / 2;
            Main.spawnTileY = TileCountY / 2;

            using (FileStream stream = File.OpenWrite(path))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    WorldFile.SaveWorld_Version2(binaryWriter);
                }
            }
        }
    }
}
