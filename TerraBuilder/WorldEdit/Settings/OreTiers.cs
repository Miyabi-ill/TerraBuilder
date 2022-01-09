// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldEdit.Settings
{
    /// <summary>
    /// ワールドの鉱石ティア毎に選択した鉱石の種類.
    /// </summary>
    public class OreTiers
    {
        /// <summary>
        /// 銅ティア鉱石のタイルID.
        /// </summary>
        public int CopperTierTileId { get; set; }

        /// <summary>
        /// 銅ティアインゴットのアイテムID.
        /// </summary>
        public int CopperTierBarItemId { get; set; }

        /// <summary>
        /// 鉄ティア鉱石のタイルID.
        /// </summary>
        public int IronTierTileId { get; set; }

        /// <summary>
        /// 鉄ティアインゴットのアイテムID.
        /// </summary>
        public int IronTierBarItemId { get; set; }

        /// <summary>
        /// 銀ティア鉱石のタイルID.
        /// </summary>
        public int SilverTierTileId { get; set; }

        /// <summary>
        /// 銀ティアインゴットのアイテムID.
        /// </summary>
        public int SilverTierBarItemId { get; set; }

        /// <summary>
        /// 金ティア鉱石のタイルID.
        /// </summary>
        public int GoldTierTileId { get; set; }

        /// <summary>
        /// 金ティアインゴットのアイテムID.
        /// </summary>
        public int GoldTierBarItemId { get; set; }
    }
}
