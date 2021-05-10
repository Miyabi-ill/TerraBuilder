namespace TUBGWorldGenerator.BuildingGenerator
{
    using System.Xml.Serialization;
    using Terraria;

    public class BuildBase
    {
        /// <summary>
        /// 建築の基準点X。左下を0とする。
        /// </summary>
        public virtual int X { get; set; }

        /// <summary>
        /// 建築の基準点Y。左下を0とする。
        /// </summary>
        public virtual int Y { get; set; }

        /// <summary>
        /// タイル配列を生成する
        /// </summary>
        /// <returns>生成したタイル配列</returns>
        public virtual Tile[,] Build()
        {
            return new Tile[0, 0];
        }
    }
}
