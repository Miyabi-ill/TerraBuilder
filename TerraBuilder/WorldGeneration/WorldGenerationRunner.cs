namespace TerraBuilder.WorldGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// ワールド生成を行うクラス。
    /// </summary>
    public class WorldGenerationRunner
    {
        static WorldGenerationRunner()
        {
            foreach (Type type in typeof(WorldGenerationRunner).Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ActionAttribute), false).Length > 0)
                {
                    AvailableActions.Add(type.Name, () => type.GetConstructor(Type.EmptyTypes).Invoke(null) as IWorldGenerationAction<ActionContext>);
                }
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// 自動的にコンテキストを読み込む。
        /// </summary>
        public WorldGenerationRunner()
        {
            CurrentRunner = this;

            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Caverns)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Surface)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.Wells)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.Tunnel)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomSizeBlocks)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomSizeBlockWithArea)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomCavernChests)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.CavernWater)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.RandomRope)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Buildings.LiquidsInAir)].Invoke());
            WorldGenerationActions.Add(AvailableActions[nameof(Actions.Biomes.SpawnArea)].Invoke());

            // TODO: Load from json
            GlobalContext = new GlobalContext();
        }

        /// <summary>
        /// 利用可能なアクションの名前と生成用関数の辞書。
        /// </summary>
        public static Dictionary<string, Func<IWorldGenerationAction<ActionContext>>> AvailableActions { get; } = new Dictionary<string, Func<IWorldGenerationAction<ActionContext>>>();

        /// <summary>
        /// 現在のインスタンス
        /// </summary>
        public static WorldGenerationRunner CurrentRunner { get; private set; }

        /// <summary>
        /// ワールド生成アクションリスト。このリスト順で生成が実行される。
        /// </summary>
        public ObservableCollection<IWorldGenerationAction<ActionContext>> WorldGenerationActions { get; private set; } = new ObservableCollection<IWorldGenerationAction<ActionContext>>();

        /// <summary>
        /// 全体から参照されるコンテキスト。
        /// 通常は`WorldGenerationRunner.CurrentRunner.GlobalContext`でアクセスされる。
        /// </summary>
        public GlobalContext GlobalContext { get; private set; }

        /// <summary>
        /// 登録されている全てのアクションを実行し、ワールド生成を行う。
        /// </summary>
        /// <param name="sandbox">ワールド生成を行うサンドボックス</param>
        /// <returns>アクションが全て成功すればtrue</returns>
        public bool Run(WorldSandbox sandbox)
        {
            sandbox.Reset();
            foreach (var action in WorldGenerationActions)
            {
                lock (sandbox)
                {
                    try
                    {
                        bool success = action.Run(sandbox);
                        if (!success)
                        {
                            MainWindow.Window.ShowErrorMessage($"アクション`{action.Name}`で生成に失敗しました。");
                            return false;
                        }
                    }
                    catch
                    {
                        MainWindow.Window.ShowErrorMessage($"アクション`{action.Name}`で生成にエラーが発生しました。");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 現在のワールド生成アクションを保存する
        /// </summary>
        /// <param name="path">保存先のパス</param>
        public void Save(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(
                    new ValueTuple<GlobalContext, ObservableCollection<IWorldGenerationAction<ActionContext>>>(GlobalContext, WorldGenerationActions),
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented,
                    }));
            }
        }

        /// <summary>
        /// ワールド生成アクションを読み込む
        /// </summary>
        /// <param name="path">読み込むパス</param>
        public void Load(string path)
        {
            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path))
                {
                    var tuple = JsonConvert.DeserializeObject<ValueTuple<GlobalContext, ObservableCollection<IWorldGenerationAction<ActionContext>>>>(
                        sr.ReadToEnd(),
                        new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                        });
                    GlobalContext = tuple.Item1;
                    WorldGenerationActions.Clear();
                    foreach (var item in tuple.Item2)
                    {
                        WorldGenerationActions.Add(item);
                    }
                }

                MainWindow.Window.ShowMessage($"アクションを{path}から読み込みました。");
            }
            else
            {
                MainWindow.Window.ShowMessage($"生成コンフィグファイル`{path}`が存在しません。");
            }
        }
    }
}
