namespace TUBGWorldGenerator.WorldGeneration
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// 各アクション毎の設定項目の基底クラス。
    /// </summary>
    public abstract class ActionContext
    {
        private readonly Dictionary<string, PropertyInfo> propertyDictionary = new Dictionary<string, PropertyInfo>();

        private readonly Dictionary<string, object> outerPropertyDictionary = new Dictionary<string, object>();

        /// <summary>
        /// コンストラクタ。
        /// プロパティを全て辞書に追加する。
        /// </summary>
        public ActionContext()
        {
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                propertyDictionary.Add(property.Name, property);
            }
        }

        /// <summary>
        /// プロパティの値を名前から取得する。
        /// 必要性は不明。
        /// </summary>
        /// <param name="name">プロパティ名</param>
        /// <returns>プロパティの値</returns>
        public object this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new KeyNotFoundException(name);
                }

                if (propertyDictionary.ContainsKey(name))
                {
                    return propertyDictionary[name].GetValue(this);
                }
                else if (outerPropertyDictionary.ContainsKey(name))
                {
                    return outerPropertyDictionary[name];
                }

                throw new KeyNotFoundException(name);
            }

            set
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new KeyNotFoundException(name);
                }

                if (propertyDictionary.ContainsKey(name))
                {
                    propertyDictionary[name].SetValue(this, value);
                }
                else if (outerPropertyDictionary.ContainsKey(name))
                {
                    outerPropertyDictionary[name] = value;
                }
                else
                {
                    // キーがない場合、辞書に追加する。Pythonの辞書の挙動の模倣
                    outerPropertyDictionary.Add(name, value);
                }
            }
        }
    }
}
