// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// TODO: 外部アクションとの通信方法の提供という目的に、この方法が最適か検討.
namespace TerraBuilder.WorldGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// 各アクション毎の設定項目の基底クラス.
    /// </summary>
    public abstract class LayerConfig
    {
        private readonly Dictionary<string, PropertyInfo> propertyDictionary = new Dictionary<string, PropertyInfo>();

        private readonly Dictionary<string, object> outerPropertyDictionary = new Dictionary<string, object>();

        /// <summary>
        /// コンストラクタ.
        /// プロパティを全て辞書に追加する.
        /// </summary>
        protected LayerConfig()
        {
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                this.propertyDictionary.Add(property.Name, property);
            }
        }

        /// <summary>
        /// コンフィグの値を名前から取得する.
        /// </summary>
        /// <typeparam name="T">取得したい値の型.厳密に（基底クラスでの取得不可）一致している必要あり.</typeparam>
        /// <param name="name">取得したい値の名前.`アクション名:コンフィグ名`のフォーマットにすること.</param>
        /// <returns>コンフィグに入っている値.</returns>
        /// <exception cref="KeyNotFoundException">取得したい値の名前が見つからない場合.</exception>
        public T GetValue<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new KeyNotFoundException(name);
            }

            if (this.propertyDictionary.ContainsKey(name))
            {
                return (T)this.propertyDictionary[name].GetValue(this);
            }
            else if (this.outerPropertyDictionary.ContainsKey(name))
            {
                return (T)this.outerPropertyDictionary[name];
            }

            throw new KeyNotFoundException(name);
        }

        /// <summary>
        /// コンフィグに値を設定する.
        /// </summary>
        /// <typeparam name="T">設定する値の型.</typeparam>
        /// <param name="name">`アクション名:コンフィグ名`形式の名前.</param>
        /// <param name="value">設定する値.</param>
        /// <exception cref="ArgumentNullException"><see cref="name"/>がnullか空の時.</exception>
        /// <exception cref="ArgumentException"><see cref="name"/>が`アクション名:コンフィグ名`形式でなかった時.</exception>
        public void SetValue<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(name);
            }

            string[] splitted = name.Split(new char[] { ':' });
            if (splitted.Length != 2)
            {
                throw new ArgumentException($"{nameof(name)}は`アクション名:コンフィグ名`形式の名前でなければいけません", nameof(name));
            }

            if (this.propertyDictionary.ContainsKey(name))
            {
                this.propertyDictionary[name].SetValue(this, value);
            }
            else if (this.outerPropertyDictionary.ContainsKey(name))
            {
                this.outerPropertyDictionary[name] = value;
            }
            else
            {
                // キーがない場合、辞書に追加する.Pythonの辞書の挙動の模倣
                this.outerPropertyDictionary.Add(name, value);
            }
        }

        /// <summary>
        /// クラスに元から存在しないプロパティ情報をクリアする.
        /// </summary>
        public void ClearAdditionalConfig() => this.outerPropertyDictionary.Clear();
    }
}
