namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using Newtonsoft.Json;
    using Terraria;

    /// <summary>
    /// 建築ジェネレータ
    /// </summary>
    public class BuildingGenerator
    {
        /// <summary>
        /// 建築ツリーのルート要素
        /// </summary>
        public BuildBase Root { get; set; }

        /// <summary>
        /// <see cref="Root"/>から生成された建築物。
        /// </summary>
        public Tile[,] Result { get; private set; }

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
        /// Jsonからツリーを生成する
        /// </summary>
        /// <param name="text">Jsonテキスト</param>
        public void ImportJson(string text)
        {
            try
            {
                Root = JsonConvert.DeserializeObject<BuildBase>(text, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch (Exception e)
            {
                Root = null;
                Result = null;
            }
        }
    }
}
