namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class WeightedRandom
    {
        public static int SelectIndex(Random random, IEnumerable<double> weights)
        {
            double sum = weights.Sum();
            if (sum == 0)
            {
                return -1;
            }

            double select = random.NextDouble();

            double current = 0;
            int i = 0;
            foreach (double weight in weights)
            {
                current += weight < 0 ? 0 : weight / sum;
                if (select < current)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}
