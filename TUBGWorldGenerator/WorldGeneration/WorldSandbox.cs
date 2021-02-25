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

        private readonly string locker = string.Empty;

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
            get => Main.maxTilesX;
            set => Main.maxTilesX = value;
        }

        /// <summary>
        /// ワールドの縦。
        /// </summary>
        public int TileCountY
        {
            get => Main.maxTilesY;
            set => Main.maxTilesY = value;
        }

        /// <summary>
        /// リスポーン地点X
        /// </summary>
        public int SpawnTileX
        {
            get => Main.spawnTileX;
            set => Main.spawnTileX = value;
        }

        /// <summary>
        /// リスポーン地点Y
        /// </summary>
        public int SpawnTileY
        {
            get => Main.spawnTileY;
            set => Main.spawnTileY = value;
        }

        /// <summary>
        /// プロパティをリセットして読み込みなおす
        /// </summary>
        /// <returns>リセットに成功したらtrue</returns>
        public bool Reset()
        {
            lock (locker)
            {
                TileCountX = 4200;
                TileCountY = 1200;

                Tiles = Main.tile;
                Chests = Main.chest;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Main.npc[i] = new NPC();
                }

                for (int x = 0; x < TileCountX; x++)
                {
                    for (int y = 0; y < TileCountY; y++)
                    {
                        Main.tile[x, y] = null;
                    }
                }

                // Seedの再設定により、Randomインスタンスを生成しなおす。
                // 追加のコンテキストもクリアしておく
                if (WorldGenerationRunner.CurrentRunner != null)
                {
                    WorldGenerationRunner.CurrentRunner.GlobalContext.Seed = WorldGenerationRunner.CurrentRunner.GlobalContext.Seed;
                    WorldGenerationRunner.CurrentRunner.GlobalContext.ClearAdditionalContext();
                }

                return true;
            }
        }

        /// <summary>
        /// ワールドを保存する。
        /// </summary>
        /// <param name="path">保存先のパス。指定しない場合はテラリアのワールドフォルダに保存される</param>
        /// <returns>保存したパス</returns>
        public string Save(string path)
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

            using (FileStream stream = File.OpenWrite(path))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    WorldFile.SaveWorld_Version2(binaryWriter);
                }
            }

            return path;
        }
    }
}
