namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using Newtonsoft.Json;
    using Terraria;
    using TUBGWorldGenerator.BuildingGenerator.Parts;

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
