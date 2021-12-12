namespace TerraBuilder.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public abstract class RandomValue<T>
    {
        /// <summary>
        /// 値を取得する
        /// </summary>
        /// <param name="rand">乱数生成インスタンス</param>
        /// <returns>生成された値</returns>
        public abstract T GetValue(Random rand);
    }

    public class ConstantValue<T> : RandomValue<T>
    {
        /// <summary>
        /// 帰す固定値
        /// </summary>
        [JsonProperty]
        public T Value { get; set; }

        /// <summary>
        /// 固定値を取得する
        /// </summary>
        /// <param name="rand">乱数生成インスタンス。使わない</param>
        /// <returns>Valueを帰す</returns>
        public override T GetValue(Random rand)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// 範囲内の値を生成する。intかdouble以外が必要な場合、SelectValueを使う。
    /// </summary>
    /// <typeparam name="T">intかdouble</typeparam>
    public class RangeValue<T> : RandomValue<T>
    {
        public RangeValue()
        {
            if (typeof(T) != typeof(int) && typeof(T) != typeof(double))
            {
                throw new TypeInitializationException(nameof(RangeValue<T>), new ArgumentException($"RangeValue generics type must be `int` or `double` but it is `{typeof(T).Name}`"));
            }
        }

        public T MinValue { get; set; }

        public T MaxValue { get; set; }

        public override T GetValue(Random rand)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            if (MinValue is int)
            {
                return (T)(object)rand.Next(Convert.ToInt32(MinValue), Convert.ToInt32(MaxValue) + 1);
            }
            else if (MinValue is double minValue && MaxValue is double maxValue)
            {
                return (T)(object)((rand.NextDouble() * (maxValue - minValue)) + minValue);
            }

            throw new ArgumentException($"RangeValue generics type must be `int` or `double` but it is `{typeof(T).Name}`");
        }

        public override string ToString()
        {
            return $"Min: {MinValue}, Max: {MaxValue}";
        }
    }

    public class SelectValue<T> : RandomValue<T>
    {
        public List<(double, T)> SelectValues { get; private set; } = new List<(double, T)>();

        public override T GetValue(Random rand)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            if (SelectValues.Count > 0)
            {
                double sum = SelectValues.AsQueryable().Select(((double, T) value) => value.Item1).Sum();
                double select = rand.NextDouble() * sum;
                foreach (var value in SelectValues)
                {
                    if (select - value.Item1 < 0)
                    {
                        return value.Item2;
                    }

                    select -= value.Item1;
                }

                return SelectValues.Last().Item2;
            }
            else
            {
                return default;
            }
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", SelectValues)}]";
        }
    }
}
