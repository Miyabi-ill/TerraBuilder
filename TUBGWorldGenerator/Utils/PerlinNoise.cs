namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Linq;

    /// <summary>
    /// パーリンノイズを生成するクラス
    /// </summary>
    public static class PerlinNoise
    {
        /// <summary>
        /// 1次パーリンノイズを生成する。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="width">全体の波の長さ</param>
        /// <param name="amplifier">波の増幅量</param>
        /// <param name="seed">シード値</param>
        /// <returns>1次パーリンノイズの配列</returns>
        public static double[] Generate1D(
            int waveLength,
            int width,
            double amplifier,
            int seed = 42)
        {
            Random random = new Random(seed);
            return Generate1D(waveLength, width, amplifier, random);
        }

        /// <summary>
        /// 1次パーリンノイズを生成する。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="width">全体の波の長さ</param>
        /// <param name="amplifier">波の増幅量</param>
        /// <param name="random">ランダムインスタンス</param>
        /// <returns>1次パーリンノイズの配列</returns>
        public static double[] Generate1D(
            int waveLength,
            int width,
            double amplifier,
            Random random)
        {
            double a = 0;
            double b = random.NextDouble();
            double[] array = new double[width];
            for (int x = 0; x < width; x++)
            {
                if (x % waveLength == 0)
                {
                    a = b;
                    b = random.NextDouble();
                    array[x] = a * amplifier;
                }
                else
                {
                    array[x] = InterpolateCos(a, b, (x % waveLength) / (double)waveLength) * amplifier;
                }
            }

            return array;
        }

        /// <summary>
        /// 異なるオクターブ(粒度)の1次パーリンノイズを合成したパーリンノイズを生成する。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="width">全体の波の長さ</param>
        /// <param name="amplifier">波の増幅量</param>
        /// <param name="octaves">オクターブ。重ねる波の数</param>
        /// <param name="diviser">波の増幅量と波長を割り算する値</param>
        /// <param name="seed">シード値</param>
        /// <returns>1次パーリンノイズの配列</returns>
        public static double[] GenerateOctave1D(
            int waveLength,
            int width,
            double amplifier,
            int octaves,
            int diviser,
            int seed = 42)
        {
            var random = new Random(seed);
            return GenerateOctave1D(waveLength, width, amplifier, octaves, diviser, random);
        }

        /// <summary>
        /// 異なるオクターブ(粒度)の1次パーリンノイズを合成したパーリンノイズを生成する。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="width">全体の波の長さ</param>
        /// <param name="amplifier">波の増幅量</param>
        /// <param name="octaves">オクターブ。重ねる波の数</param>
        /// <param name="diviser">波の増幅量と波長を割り算する値</param>
        /// <param name="random">ランダムインスタンス</param>
        /// <returns>1次パーリンノイズの配列</returns>
        public static double[] GenerateOctave1D(
            int waveLength,
            int width,
            double amplifier,
            int octaves,
            int diviser,
            Random random)
        {
            double[] result = new double[width];
            for (int i = 0; i < octaves; i++)
            {
                double[] perlin = Generate1D(waveLength, width, amplifier, random);
                for (int j = 0; j < width; j++)
                {
                    result[j] += perlin[j];
                }

                amplifier /= diviser;
                waveLength /= diviser;
            }

            return result;
        }

        /// <summary>
        /// オクターブの異なるパーリンノイズを合成した後、正規化したパーリンノイズを生成する。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="width">全体の波の長さ</param>
        /// <param name="octaves">オクターブ。重ねる波の数</param>
        /// <param name="diviser">波の増幅量と波長を割り算する値</param>
        /// <param name="random">ランダムインスタンス</param>
        /// <returns>0から1に正規化された1次パーリンノイズ</returns>
        public static double[] NormalizeOctave1D(
            int waveLength,
            int width,
            int octaves,
            int diviser,
            Random random)
        {
            double[] perlin1D = GenerateOctave1D(waveLength, width, 1, octaves, diviser, random);
            double min = perlin1D.Min();
            double max = perlin1D.Max();
            double diff = max - min;
            for (int i = 0; i < width; i++)
            {
                perlin1D[i] = (perlin1D[i] - min) / diff;
            }

            return perlin1D;
        }

        private static double InterpolateCos(double a, double b, double x)
        {
            double ft = x * Math.PI;
            double f = (1 - Math.Cos(ft)) * 0.5;
            return (a * (1 - f)) + (b * f);
        }

        private static double InterpolateLinear(double a, double b, double x)
        {
            return (a * (1 - x)) + (b * x);
        }
    }
}
