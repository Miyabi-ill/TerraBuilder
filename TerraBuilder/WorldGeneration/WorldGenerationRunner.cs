// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Newtonsoft.Json;
    using TerraBuilder.WorldEdit;

    /// <summary>
    /// ワールド生成を行うクラス.
    /// </summary>
    public class WorldGenerationRunner
    {
        static WorldGenerationRunner()
        {
            foreach (Type type in typeof(WorldGenerationRunner).Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ActionAttribute), false).Length > 0)
                {
                    AvailableActions.Add(type.Name, () => type.GetConstructor(Type.EmptyTypes).Invoke(null) as IWorldGenerationLayer<LayerConfig>);
                }
            }
        }

        /// <summary>
        /// コンストラクタ.
        /// 自動的にコンテキストを読み込む.
        /// </summary>
        public WorldGenerationRunner()
        {
            CurrentRunner = this;

            // TODO: テンプレートクラスを作り切り分け？
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Biomes.Caverns)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Biomes.Surface)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.Wells)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Biomes.Tunnel)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.RandomSizeBlocks)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.RandomSizeBlockWithArea)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.RandomCavernChests)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Biomes.CavernWater)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.RandomRope)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Buildings.LiquidsInAir)].Invoke());
            this.WorldGenerationLayers.Add(AvailableActions[nameof(Layers.Biomes.SpawnArea)].Invoke());

            // TODO: jsonから読み込み？
            this.GlobalConfig = new GlobalConfig();
        }

        /// <summary>
        /// 利用可能なアクションの名前と生成用関数の辞書.
        /// </summary>
        public static Dictionary<string, Func<IWorldGenerationLayer<LayerConfig>>> AvailableActions { get; } = new Dictionary<string, Func<IWorldGenerationLayer<LayerConfig>>>();

        /// <summary>
        /// 現在のインスタンス.
        /// TODO: 並列化にネガティブなため、構成を要検討.
        /// </summary>
        public static WorldGenerationRunner CurrentRunner { get; private set; }

        /// <summary>
        /// ワールド生成アクションリスト.このリスト順で生成が実行される.
        /// </summary>
        public ObservableCollection<IWorldGenerationLayer<LayerConfig>> WorldGenerationLayers { get; } = new ObservableCollection<IWorldGenerationLayer<LayerConfig>>();

        /// <summary>
        /// サンドボックス全体に関わるコンフィグ項目.
        /// </summary>
        public GlobalConfig GlobalConfig { get; private set; }

        /// <summary>
        /// 登録されている全てのレイヤーを実行し、ワールド生成を行う.
        /// </summary>
        /// <param name="sandbox">ワールド生成を行うサンドボックス.</param>
        /// <returns>レイヤーの適用が全て成功すればtrue.1つでも失敗すれば、その時点で失敗しfalseを返す.</returns>
        public bool Run(WorldSandbox sandbox)
        {
            throw new NotImplementedException();
            _ = sandbox.Reset();
            foreach (IWorldGenerationLayer<LayerConfig> action in this.WorldGenerationLayers)
            {
                lock (sandbox)
                {
                    try
                    {
                        bool success = action.Apply(sandbox);
                        if (!success)
                        {
                            MainWindow.Window.ShowErrorMessage($"アクション`{action.Name}`で生成に失敗しました.");
                            return false;
                        }
                    }
                    catch
                    {
                        MainWindow.Window.ShowErrorMessage($"アクション`{action.Name}`で生成にエラーが発生しました.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 現在のワールド生成レイヤー群とその設定を保存する.
        /// </summary>
        /// <param name="path">保存先のパス.</param>
        public void Save(string path)
        {
            throw new NotImplementedException();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(
                    new ValueTuple<GlobalConfig, ObservableCollection<IWorldGenerationLayer<LayerConfig>>>(this.GlobalConfig, this.WorldGenerationLayers),
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented,
                    }));
            }
        }

        /// <summary>
        /// ワールド生成アクションを読み込む.
        /// </summary>
        /// <param name="path">読み込むパス.</param>
        public void Load(string path)
        {
            throw new NotImplementedException();
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    (GlobalConfig, ObservableCollection<IWorldGenerationLayer<LayerConfig>>) tuple = JsonConvert.DeserializeObject<ValueTuple<GlobalConfig, ObservableCollection<IWorldGenerationLayer<LayerConfig>>>>(
                        sr.ReadToEnd(),
                        new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                        });
                    this.GlobalConfig = tuple.Item1;
                    this.WorldGenerationLayers.Clear();
                    foreach (IWorldGenerationLayer<LayerConfig> item in tuple.Item2)
                    {
                        this.WorldGenerationLayers.Add(item);
                    }
                }

                MainWindow.Window.ShowMessage($"アクションを{path}から読み込みました.");
            }
            else
            {
                MainWindow.Window.ShowMessage($"生成コンフィグファイル`{path}`が存在しません.");
            }
        }
    }
}
