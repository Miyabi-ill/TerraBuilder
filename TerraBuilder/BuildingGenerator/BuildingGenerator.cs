namespace TerraBuilder.BuildingGenerator
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Terraria;
    using TerraBuilder.BuildingGenerator.Parts;

    /// <summary>
    /// 建築ジェネレータ
    /// </summary>
    public class BuildingGenerator
    {
        public BuildingGenerator()
        {
            CurrentGenerator = this;
        }

        public static BuildingGenerator CurrentGenerator { get; private set; }

        /// <summary>
        /// 建築ツリーのルート要素
        /// </summary>
        public BuildRoot Root { get; set; }

        /// <summary>
        /// <see cref="Root"/>から生成された建築物。
        /// </summary>
        public Tile[,] Result { get; private set; }

        /// <summary>
        /// 建築物の設定ファイルが入ったディレクトリのパス
        /// </summary>
        public string BuildingsRootPath
        {
            get => BuildingsDict.BuildingsDirectory;
            set
            {
                BuildingsDict.BuildingsDirectory = value;
                BuildingsDict.Update();
            }
        }

        public BuildingsDict BuildingsDict { get; } = new BuildingsDict();

        /// <summary>
        /// 建築を現在のツリーから生成し、<see cref="Result"/>に格納する。
        /// </summary>
        public void Build()
        {
            Tile[,] tiles = Root?.Build();
            if (tiles == null || tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0)
            {
                Result = null;
                return;
            }

            Result = tiles;
        }

        public BuildRoot TilesToBuilding(Tile[,] tiles)
        {
            throw new NotImplementedException();

            BuildRoot root = new BuildRoot();

            bool[,] registeredMap = new bool[tiles.GetLength(0), tiles.GetLength(1)];
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            // 壁を全て登録
            // 全てのタイルをループしつつ、登録済みのタイルはスキップ
            // 左下からX軸、Y軸を確認していき、最大長の軸を登録する
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 壁情報が空でもスキップ
                    if (registeredMap[x, y]
                        || tiles[x, height - y - 1].wall == 0)
                    {
                        registeredMap[x, y] = true;
                        continue;
                    }

                    int lastSameY = y;
                    int wallId = tiles[x, height - y - 1].wall;

                    // 上に向かって(yがプラスの方向に)検索していく
                    for (int y_search = y + 1; y_search < height; y_search++)
                    {
                        if (tiles[x, height - y_search - 1].wall != wallId)
                        {
                            break;
                        }
                        else
                        {
                            lastSameY = y_search;
                        }
                    }

                    int lastSameX = x;
                    for (int x_search = x + 1; x_search < width; x_search++)
                    {
                        if (tiles[x_search, height - y - 1].wall != wallId)
                        {
                            break;
                        }
                        else
                        {
                            lastSameX = x_search;
                        }
                    }

                    // Xを優先で登録する。
                    if (lastSameX >= lastSameY)
                    {
                        lastSameY = y;
                        bool fail = false;

                        // Yを増加させながら、矩形が取れる最大のYを探す
                        for (int y_search = y + 1; y_search < height; y_search++)
                        {
                            for (int x_search = x + 1; x_search < width; x_search++)
                            {
                                if (tiles[x_search, height - y_search - 1].wall != wallId)
                                {
                                    fail = true;
                                    break;
                                }
                            }

                            if (fail)
                            {
                                break;
                            }

                            lastSameY = y_search;
                        }
                    }

                    // Y優先で登録する。
                    else
                    {
                        bool fail = false;
                        lastSameX = x;

                        // Xを増加させながら、矩形が取れる最大のXを探す
                        for (int x_search = x + 1; x_search < width; x_search++)
                        {
                            for (int y_search = y + 1; y_search < height; y_search++)
                            {
                                if (tiles[x_search, height - y_search - 1].wall != wallId)
                                {
                                    fail = true;
                                    break;
                                }
                            }

                            if (fail)
                            {
                                break;
                            }

                            lastSameX = x_search;
                        }
                    }

                    // 登録済みにマークする
                    for (int x_search = x; x_search < lastSameX; x_search++)
                    {
                        for (int y_search = y; y_search < lastSameY; y_search++)
                        {
                            registeredMap[x_search, y_search] = true;
                        }
                    }

                    int wallWidth = lastSameX - x + 1;
                    int wallHeight = lastSameY - y + 1;

                    Rectangle rectangle = new Rectangle()
                    {
                        FillWall = TerrariaNameDict.TileNameToID.First(p => p.Value == wallId).Key,
                        Size = new Size() { Width = wallWidth, Height = wallHeight },
                        X = x + 1,
                        Y = y + 1,
                    };

                    root.Childs.Add(rectangle);
                }
            }

            // 登録済みマップをクリアする
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    registeredMap[x, y] = false;
                }
            }

            // 複数タイルオブジェクトを全て検出、登録

        }

        /// <summary>
        /// Jsonからツリーを生成する。
        /// タイルを取得するためには、<see cref="Build"/>を呼び出す。
        /// </summary>
        /// <param name="text">Jsonテキスト</param>
        public void ImportJson(string text)
        {
            try
            {
                Root = JsonConvert.DeserializeObject<BuildRoot>(text, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch (Exception e)
            {
                Root = null;
                Result = null;
            }
        }
    }
}
