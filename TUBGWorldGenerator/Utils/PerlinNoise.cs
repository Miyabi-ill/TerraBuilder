namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class PerlinNoise
    {
        private static readonly int[] LookupTable;

        static PerlinNoise()
        {
            int[] noLoopLookupTable = new int[256];
            for (int i = 0; i < 256; i++)
            {
                noLoopLookupTable[i] = i;
            }

            Random rand = new Random(42);
            noLoopLookupTable = noLoopLookupTable.OrderBy(x => rand.NextDouble()).ToArray();
            LookupTable = noLoopLookupTable.Concat(noLoopLookupTable).ToArray();
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

        public static double[] Generate1D(
            int waveLength,
            int width,
            double amplifier,
            int seed = 42)
        {
            var rand = new Random(seed);
            double a = 0;
            double b = rand.NextDouble();
            double[] array = new double[width];
            for (int x = 0; x < width; x++)
            {
                if (x % waveLength == 0)
                {
                    a = b;
                    b = rand.NextDouble();
                    array[x] = a * amplifier;
                }
                else
                {
                    array[x] = InterpolateCos(a, b, (x % waveLength) / (double)waveLength) * amplifier;
                }
            }

            return array;
        }

        public static double[] GenerateOctave1D(
            int waveLength,
            int width,
            double amplifier,
            int octaves,
            int diviser,
            int seed = 42)
        {
            var rand = new Random(seed);
            double[] result = new double[width];
            for (int i = 0; i < octaves; i++)
            {
                double[] perlin = Generate1D(waveLength, width, amplifier, rand.Next());
                for (int j = 0; j < width; j++)
                {
                    result[j] += perlin[j];
                }

                amplifier /= diviser;
                waveLength /= diviser;
            }

            return result;
        }
    }
}
