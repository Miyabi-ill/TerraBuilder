namespace TerraBuilder.Utils
{
    using System;

    public static class RandGen
    {
        /// <summary>
        /// 範囲から指定の長さが含まれる始点を生成する.
        /// </summary>
        /// <param name="random">ランダムインスタンス</param>
        /// <param name="rangeMin">範囲の最小</param>
        /// <param name="rangeMax">範囲の最大</param>
        /// <param name="length">長さ</param>
        /// <returns>範囲の最小を0とした相対位置.失敗した場合は-1</returns>
        public static int SelectPositionInRange(Random random, int rangeMin, int rangeMax, int length)
        {
            if (rangeMax - length <= rangeMin)
            {
                return -1;
            }

            return random.Next(rangeMin, rangeMax - length);
        }
    }
}
