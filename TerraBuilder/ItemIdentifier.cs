// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// アイテム識別子.
    /// </summary>
    public readonly struct ItemIdentifier
    {
        public ItemIdentifier(int id)
        {
            this.ID = id;
        }

        /// <summary>
        /// アイテムID.
        /// </summary>
        public int ID { get; }

 

        public static bool operator ==(ItemIdentifier itemIdentifier, ItemIdentifier other)
        {
            return itemIdentifier.ID == other.ID;
        }

        public static bool operator !=(ItemIdentifier itemIdentifier, ItemIdentifier other)
        {
            return itemIdentifier.ID != other.ID;
        }

        /// <summary>
        /// アイテム識別子が別のオブジェクトと同一（同じ値を持っている）か判定する.
        /// </summary>
        /// <param name="obj">判定するオブジェクト.</param>
        /// <returns>オブジェクトが同一ならtrue.違うならfalse.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ItemIdentifier other)
            {
                return this.ID == other.ID;
            }

            return false;
        }
    }
}
