// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder
{
    using System;

    /// <summary>
    /// タイル範囲.
    /// </summary>
    public readonly struct TileRange
    {
        /// <summary>
        /// 左上の座標と幅、高さからタイル範囲を作成する.
        /// </summary>
        /// <param name="topLeftCoordinate">タイル範囲の左上の座標.</param>
        /// <param name="width">タイル範囲の幅.</param>
        /// <param name="height">タイル範囲の高さ.</param>
        /// <exception cref="ArgumentException">幅が0未満のとき.高さが0未満のとき.</exception>
        public TileRange(Coordinate topLeftCoordinate, int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentException("width >= 0でなければなりません");
            }

            if (height < 0)
            {
                throw new ArgumentException("height >= 0でなければなりません");
            }

            this.Top = topLeftCoordinate.Y;
            this.Left = topLeftCoordinate.X;
            this.Bottom = topLeftCoordinate.Y + height;
            this.Right = topLeftCoordinate.X + width;

            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// 左上の座標と右下の座標からタイル範囲を作成する.
        /// </summary>
        /// <param name="topLeftCoordinate">タイル範囲の左上の座標.</param>
        /// <param name="bottomRightCoordinate">タイル範囲の右下の座標.</param>
        /// <exception cref="ArgumentException">右下の座標が左上の座標より、左上に存在しているとき.</exception>
        public TileRange(Coordinate topLeftCoordinate, Coordinate bottomRightCoordinate)
        {
            this.Top = topLeftCoordinate.Y;
            this.Bottom = bottomRightCoordinate.Y;
            this.Left = topLeftCoordinate.X;
            this.Right = topLeftCoordinate.X;

            this.Width = bottomRightCoordinate.X - topLeftCoordinate.X;
            this.Height = bottomRightCoordinate.Y - topLeftCoordinate.Y;

            if (this.Width < 0)
            {
                throw new ArgumentException("bottomRightCoordinate.X >= topLeftCoordinate.Xでなければなりません");
            }

            if (this.Height < 0)
            {
                throw new ArgumentException("bottomRightCoordinate.Y >= topLeftCoordinate.Yでなければなりません");
            }
        }

        /// <summary>
        /// タイル範囲の上辺.
        /// </summary>
        public int Top { get; }

        /// <summary>
        /// タイル範囲の下辺.
        /// </summary>
        public int Bottom { get; }

        /// <summary>
        /// タイル範囲の左辺.
        /// </summary>
        public int Left { get; }

        /// <summary>
        /// タイル範囲の右辺.
        /// </summary>
        public int Right { get; }

        /// <summary>
        /// タイル範囲の幅.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// タイル範囲の高さ.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// タイル範囲の左上の座標.
        /// </summary>
        public Coordinate TopLeft => new Coordinate(this.Left, this.Top);

        /// <summary>
        /// タイル範囲の右上の座標.
        /// </summary>
        public Coordinate TopRight => new Coordinate(this.Right, this.Top);

        /// <summary>
        /// タイル範囲の左下の座標.
        /// </summary>
        public Coordinate BottomLeft => new Coordinate(this.Left, this.Bottom);

        /// <summary>
        /// タイル範囲の右下の座標.
        /// </summary>
        public Coordinate BottomRight => new Coordinate(this.Right, this.Bottom);

        /// <summary>
        /// タイル範囲の中から、ランダムに座標を選択する.
        /// </summary>
        /// <param name="random">ランダムインスタンス.</param>
        /// <returns>選択された座標.</returns>
        /// <exception cref="ArgumentNullException">randomがnullのとき.</exception>
        public Coordinate SelectRandomPointInRange(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return new Coordinate(random.Next(this.Left, this.Right + 1), random.Next(this.Top, this.Bottom + 1));
        }
    }
}
