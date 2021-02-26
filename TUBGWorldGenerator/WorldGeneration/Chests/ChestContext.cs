﻿namespace TUBGWorldGenerator.WorldGeneration.Chests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using Terraria.ID;

    /// <summary>
    /// 設置するチェストの設定を行うクラス。
    /// </summary>
    public class ChestContext : ActionContext
    {
        /// <summary>
        /// チェスト名
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// 設置するチェストのタイルID
        /// </summary>
        public int TileType { get; set; } = TileID.Containers;

        /// <summary>
        /// 設置するチェストのスタイル
        /// </summary>
        public int TileStyle { get; set; } = 0;

        /// <summary>
        /// チェストに入れるアイテムのスロット。
        /// </summary>
        public List<ItemSlotProbablyAndStack> ItemSlots { get; private set; } = new List<ItemSlotProbablyAndStack>();
    }
}
