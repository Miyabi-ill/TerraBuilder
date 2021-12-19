namespace TerraBuilder.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [JsonConverter(typeof(RandomValueConverter))]
    public abstract class RandomValue
    {
        /// <summary>
        /// 値選択タイプ名
        /// </summary>
        [JsonProperty]
        public abstract string TypeName { get; }

        /// <summary>
        /// 値を取得する
        /// </summary>
        /// <param name="rand">乱数生成インスタンス</param>
        /// <returns>生成された値</returns>
        public abstract object GetValue(Random rand);
    }

    [JsonConverter(typeof(RandomValueConverter))]
    public class ConstantValue : RandomValue
    {
        private object value;

        /// <summary>
        /// 固定値を返すクラスのコンストラクタ。
        /// </summary>
        public ConstantValue()
        {
        }

        /// <summary>
        /// 固定値を返すクラスのコンストラクタ。
        /// </summary>
        /// <param name="value">返す固定値</param>
        public ConstantValue(object value)
        {
            Value = value;
        }

        /// <summary>
        /// 返す固定値
        /// </summary>
        [JsonProperty]
        public object Value
        {
            get => value;
            set
            {
                if (value is long val)
                {
                    this.value = (int)val;
                }
                else
                {
                    this.value = value;
                }
            }
        }

        /// <inheritdoc/>
        [JsonProperty]
        public override string TypeName => "Constant";

        /// <summary>
        /// 固定値を取得する
        /// </summary>
        /// <param name="rand">乱数生成インスタンス。使わない</param>
        /// <returns>Valueを帰す</returns>
        public override object GetValue(Random rand)
        {
            return Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value?.ToString();
        }
    }

    /// <summary>
    /// 範囲内の値を生成する。intかdouble以外が必要な場合、SelectValueを使う。
    /// </summary>
    [JsonConverter(typeof(RandomValueConverter))]
    public class RangeValue : RandomValue
    {
        private Type dataType;
        private object minValue;
        private object maxValue;

        /// <summary>
        /// 範囲内の値を生成するクラスのコンストラクタ。
        /// このコンストラクタを使用する場合、DataTypeを自分で指定する必要がある。
        /// </summary>
        public RangeValue()
        {
        }

        /// <summary>
        /// 範囲内の値を生成するクラスのコンストラクタ。
        /// </summary>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <exception cref="TypeInitializationException">minValueかmaxValueがintかdoubleでなければエラー</exception>
        public RangeValue(object minValue, object maxValue)
        {
            if (minValue is long)
            {
                minValue = (int)(long)minValue;
            }

            if (maxValue is long)
            {
                maxValue = (int)(long)maxValue;
            }

            if ((minValue.GetType() != typeof(int) && minValue.GetType() != typeof(double))
                || (maxValue.GetType() != typeof(int) && maxValue.GetType() != typeof(double)))
            {
                throw new TypeInitializationException(nameof(RangeValue), new ArgumentException("RangeValue value type must be `int` or `double`."));
            }

            if (minValue is double || maxValue is double)
            {
                this.minValue = (double)minValue;
                this.maxValue = (double)maxValue;
                dataType = typeof(double);
            }
            else
            {
                this.minValue = minValue;
                this.maxValue = maxValue;
                dataType = typeof(int);
            }
        }

        [JsonProperty]
        public override string TypeName => "Range";

        [JsonProperty]
        public object MinValue
        {
            get => minValue;
            set
            {
                if (value is long)
                {
                    minValue = (int)(long)value;
                }
                else
                {
                    minValue = value;
                }
            }
        }

        [JsonProperty]
        public object MaxValue
        {
            get => maxValue;
            set
            {
                if (value is long)
                {
                    maxValue = (int)(long)value;
                }
                else
                {
                    maxValue = value;
                }
            }
        }

        public override object GetValue(Random rand)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            if (dataType == null)
            {
                if (MinValue is int && MaxValue is int)
                {
                    dataType = typeof(int);
                }
                else
                {
                    dataType = typeof(double);
                }
            }

            if (dataType == typeof(int))
            {
                return rand.Next((int)MinValue, (int)MaxValue + 1);
            }
            else if (dataType == typeof(double))
            {
                return (rand.NextDouble() * ((double)MaxValue - (double)MinValue)) + (double)MinValue;
            }

            throw new ArgumentException("RangeValue DataType must be `int` or `double`");
        }

        public override string ToString()
        {
            return $"Min: {MinValue}, Max: {MaxValue}";
        }
    }

    [JsonConverter(typeof(RandomValueConverter))]
    public class SelectValue : RandomValue
    {
        [JsonProperty]
        public override string TypeName => "Select";

        [JsonProperty]
        public List<(double, object)> SelectValues { get; protected set; } = new List<(double, object)>();

        public override object GetValue(Random rand)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            if (SelectValues.Count > 0)
            {
                double sum = SelectValues.AsQueryable().Select(((double, object) value) => value.Item1).Sum();
                double select = rand.NextDouble() * sum;
                foreach (var value in SelectValues)
                {
                    if (select - value.Item1 < 0)
                    {
                        return value.Item2;
                    }

                    select -= value.Item1;
                }

                object val = SelectValues.Last().Item2;
                if (val is long)
                {
                    val = (int)(long)val;
                }

                return val;
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

    public class RandomValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(RandomValue).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            // JObjectをJsonからロード
            JObject jObject = JObject.Load(reader);

            string typeName = (string)jObject["TypeName"];
            RandomValue target;
            switch (typeName)
            {
                case "Constant":
                    target = new ConstantValue();
                    break;
                case "Range":
                    target = new RangeValue();
                    break;
                case "Select":
                    target = new SelectValue();
                    break;
                default:
                    throw new JsonReaderException($"TypeName `{typeName}` does not implemented.");
            }

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
