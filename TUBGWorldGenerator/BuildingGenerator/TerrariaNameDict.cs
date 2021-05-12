namespace TUBGWorldGenerator.BuildingGenerator
{
    using System.Collections.Generic;
    using Terraria;
    using Terraria.ID;

    /// <summary>
    /// 文字列からテラリアIDを取得するクラス。
    /// </summary>
    public static class TerrariaNameDict
    {
        static TerrariaNameDict()
        {
            foreach (var field in typeof(TileID).GetFields())
            {
                if (field.FieldType == typeof(ushort))
                {
                    TileNameToID.Add(field.Name, (ushort)field.GetValue(null));
                }
            }

            foreach (var field in typeof(WallID).GetFields())
            {
                if (field.FieldType == typeof(ushort))
                {
                    WallNameToID.Add(field.Name, (ushort)field.GetValue(null));
                }
            }

            foreach (var field in typeof(ItemID).GetFields())
            {
                if (field.FieldType == typeof(short))
                {
                    int id = (short)field.GetValue(null);
                    Item item = new Item();
                    item.SetDefaults(id);
                    if (item.type == id)
                    {
                        ItemNameToItem.Add(field.Name, item);
                    }
                }
            }
        }

        /// <summary>
        /// タイル内部名からIDを取得する
        /// </summary>
        public static Dictionary<string, ushort> TileNameToID { get; } = new Dictionary<string, ushort>();

        /// <summary>
        /// 壁内部名からIDを取得する
        /// </summary>
        public static Dictionary<string, ushort> WallNameToID { get; } = new Dictionary<string, ushort>();

        public static Dictionary<string, Item> ItemNameToItem { get; } = new Dictionary<string, Item>();
    }
}
