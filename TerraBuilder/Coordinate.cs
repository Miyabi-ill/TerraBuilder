// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder
{
    using System;

    /// <summary>
    /// テラリアワールドの座標用クラス。
    /// ワールドの左上をx, y = 0, 0とし、xは横方向、yは縦方向の軸を表す.
    /// </summary>
    public struct Coordinate
    {
        /// <summary>
        /// ワールドの横方向の座標。右に行くほど大.
        /// </summary>
        public int X;

        /// <summary>
        /// ワールドの縦方向の座標。下に行くほど大.
        /// </summary>
        public int Y;

        /// <summary>
        /// テラリアワールドの座標コンストラクタ.
        /// yは0で設定される.
        /// </summary>
        /// <param name="x">ワールドの横方向の座標.</param>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="x"/>がマイナス値のとき.</exception>
        public Coordinate(int x = 0)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            this.X = x;
            this.Y = 0;
        }

        /// <summary>
        /// テラリアワールドの座標コンストラクタ.
        /// </summary>
        /// <param name="x">ワールドの横方向の座標.</param>
        /// <param name="y">ワールドの縦方向の座標.</param>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="x"/>か<see cref="y"/>がマイナス値のとき.</exception>
        public Coordinate(int x, int y)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), nameof(x) + "は負の値になることはできません。");
            }

            if (y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(y), nameof(y) + "は負の値になることはできません。");
            }

            this.X = x;
            this.Y = y;
        }

        public static Coordinate operator +(Coordinate right, Coordinate left)
        {
            return new Coordinate(right.X + left.X, right.Y + left.Y);
        }

        public static Coordinate operator -(Coordinate coordinate)
        {
            return new Coordinate(-coordinate.X, -coordinate.Y);
        }

        public static Coordinate operator -(Coordinate right, Coordinate left)
        {
            return new Coordinate(right.X - left.X, right.Y - left.Y);
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Coordinate))
            {
                return false;
            }

            var other = (Coordinate)obj;
            return this.X == other.X && this.Y == other.Y;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int left = this.X << 16;
            int right = this.X >> 16;
            return left ^ right ^ this.Y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X: {this.X}, Y: {this.Y}";
        }
    }
}
