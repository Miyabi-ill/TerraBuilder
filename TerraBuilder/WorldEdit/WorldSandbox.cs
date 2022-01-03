﻿// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit
{
    using System;
    using System.IO;
    using OTAPI.Tile;
    using Terraria;
    using Terraria.IO;
    using Terraria.Localization;
    using Terraria.Map;

    /// <summary>
    /// テラリアのワールドの干渉に必要な情報をまとめたクラス.
    /// </summary>
    public class WorldSandbox
    {
        private static bool isInitializedTerrariaInstance = false;

        private readonly string locker = string.Empty;

        /// <summary>
        /// コンストラクタ.
        /// もしテラリアのクラスが初期化されていなければ初期化し、
        /// プロパティを設定する.
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

            this.Sync();
        }

        /// <summary>
        /// タイル保護マップ.
        /// </summary>
        public TileProtectionMap TileProtectionMap { get; private set; }

        /// <summary>
        /// ワールドにあるチェスト.
        /// </summary>
        public Chest[] Chests { get; private set; }

        /// <summary>
        /// ワールドの横幅.
        /// </summary>
        public int TileCountX
        {
            get => Main.maxTilesX;
            set => Main.maxTilesX = value;
        }

        /// <summary>
        /// ワールドの縦幅.
        /// </summary>
        public int TileCountY
        {
            get => Main.maxTilesY;
            set => Main.maxTilesY = value;
        }

        /// <summary>
        /// リスポーン地点X.
        /// </summary>
        public int SpawnTileX
        {
            get => Main.spawnTileX;
            set => Main.spawnTileX = value;
        }

        /// <summary>
        /// リスポーン地点Y.
        /// </summary>
        public int SpawnTileY
        {
            get => Main.spawnTileY;
            set => Main.spawnTileY = value;
        }

        /// <summary>
        /// ワールドのシード値.
        /// </summary>
        public int Seed { get; set; } = 42;

        /// <summary>
        /// ワールドに存在するタイル.
        /// </summary>
        private ITileCollection Tiles { get; set; }

        /// <summary>
        /// テラリアと同期を取る.
        /// </summary>
        /// <returns>同期に成功したらtrue.失敗したらfalse.</returns>
        public bool Sync()
        {
            lock (this.locker)
            {
                this.TileCountX = 4200;
                this.TileCountY = 1200;

                this.Tiles = Main.tile;
                this.Chests = Main.chest;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Main.npc[i] = new NPC();
                }

                for (int x = 0; x < this.TileCountX; x++)
                {
                    for (int y = 0; y < this.TileCountY; y++)
                    {
                        Main.tile[x, y] = new Tile();
                    }
                }

                for (int i = 0; i < Main.chest.Length; i++)
                {
                    Main.chest[i] = null;
                }

                this.TileProtectionMap = new TileProtectionMap(this);

                return true;
            }
        }

        /// <summary>
        /// ワールドを保存する.
        /// </summary>
        /// <param name="path">保存先のパス.指定しない場合はテラリアのワールドフォルダに保存される.</param>
        /// <returns>保存したパス.</returns>
        public string Save(string path)
        {
            lock (locker)
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

                Main.ActiveWorldFileData.SetSeed(Seed.ToString());
                Main.worldID = 42;

                Main.treeX[0] = TileCountX;
                Main.treeX[1] = TileCountX;
                Main.treeX[2] = TileCountX;

                Main.caveBackX[0] = TileCountX;
                Main.caveBackX[1] = TileCountX;
                Main.caveBackX[2] = TileCountX;

                Main.worldSurface = 400;
                Main.rockLayer = 800;

                Main.worldName = string.IsNullOrEmpty(Main.worldName) ? "TerraBuild" : Main.worldName;

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

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                lock (locker)
                {
                    using (FileStream stream = File.OpenRead(path))
                    {
                        using (BinaryReader binaryWriter = new BinaryReader(stream))
                        {
                            WorldFile.LoadWorld_Version2(binaryWriter);
                        }
                    }

                    TileCountX = Main.maxTilesX;
                    TileCountY = Main.maxTilesY;

                    Tiles = Main.tile;
                    Chests = Main.chest;

                    // Seedの再設定により、Randomインスタンスを生成しなおす.
                    // 追加のコンテキストもクリアしておく
                    if (WorldGenerationRunner.CurrentRunner != null)
                    {
                        WorldGenerationRunner.CurrentRunner.GlobalContext.Seed = Main.ActiveWorldFileData.Seed;
                        WorldGenerationRunner.CurrentRunner.GlobalContext.ClearAdditionalContext();
                    }

                    TileProtectionMap = new TileProtectionMap(this);
                }
            }
        }
    }
}