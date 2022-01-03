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
                    AvailableActions.Add(type.Name, () => type.GetConstructor(Type.EmptyTypes).Invoke(null) as IWorldGenerationAction<ActionConfig>);
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

            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Caverns)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Surface)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.Wells)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Tunnel)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomSizeBlocks)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomSizeBlockWithArea)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomCavernChests)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.CavernWater)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomRope)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.LiquidsInAir)].Invoke());
            this.WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.SpawnArea)].Invoke());

            // TODO: jsonから読み込み？
            this.GlobalContext = new GlobalContext();
        }

        /// <summary>
        /// 利用可能なアクションの名前と生成用関数の辞書.
        /// </summary>
        public static Dictionary<string, Func<IWorldGenerationAction<ActionConfig>>> AvailableActions { get; } = new Dictionary<string, Func<IWorldGenerationAction<ActionConfig>>>();

        /// <summary>
        /// 現在のインスタンス.
        /// TODO: 並列化にネガティブなため、構成を要検討.
        /// </summary>
        public static WorldGenerationRunner CurrentRunner { get; private set; }

        /// <summary>
        /// ワールド生成アクションリスト.このリスト順で生成が実行される.
        /// </summary>
        public ObservableCollection<IWorldGenerationAction<ActionConfig>> WorldGenerationActions { get; } = new ObservableCollection<IWorldGenerationAction<ActionConfig>>();

        /// <summary>
        /// 全体から参照されるコンテキスト.
        /// 通常は`WorldGenerationRunner.CurrentRunner.GlobalContext`でアクセスされる.
        /// </summary>
        public GlobalContext GlobalContext { get; private set; }

        /// <summary>
        /// 登録されている全てのアクションを実行し、ワールド生成を行う.
        /// </summary>
        /// <param name="sandbox">ワールド生成を行うサンドボックス.</param>
        /// <returns>アクションが全て成功すればtrue.1つでも失敗すれば、その時点で失敗しfalseを返す.</returns>
        public bool Run(WorldSandbox sandbox)
        {
            _ = sandbox.Reset();
            foreach (IWorldGenerationAction<ActionConfig> action in this.WorldGenerationActions)
            {
                lock (sandbox)
                {
                    try
                    {
                        bool success = action.Run(sandbox);
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
        /// 現在のワールド生成アクションを保存する.
        /// </summary>
        /// <param name="path">保存先のパス.</param>
        public void Save(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(
                    new ValueTuple<GlobalContext, ObservableCollection<IWorldGenerationAction<ActionConfig>>>(this.GlobalContext, this.WorldGenerationActions),
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
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    (GlobalContext, ObservableCollection<IWorldGenerationAction<ActionConfig>>) tuple = JsonConvert.DeserializeObject<ValueTuple<GlobalContext, ObservableCollection<IWorldGenerationAction<ActionConfig>>>>(
                        sr.ReadToEnd(),
                        new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                        });
                    this.GlobalContext = tuple.Item1;
                    this.WorldGenerationActions.Clear();
                    foreach (IWorldGenerationAction<ActionConfig> item in tuple.Item2)
                    {
                        this.WorldGenerationActions.Add(item);
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
