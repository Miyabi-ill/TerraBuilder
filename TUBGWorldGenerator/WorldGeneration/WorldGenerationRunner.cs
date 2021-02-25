namespace TUBGWorldGenerator.WorldGeneration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ワールド生成を行うクラス。
    /// </summary>
    public class WorldGenerationRunner
    {
        /// <summary>
        /// コンストラクタ。
        /// 自動的にコンテキストを読み込む。
        /// </summary>
        public WorldGenerationRunner()
        {
            CurrentRunner = this;

            WorldGenerationActions.Add(new Actions.Biomes.Caverns());
            WorldGenerationActions.Add(new Actions.Biomes.Surface());
            WorldGenerationActions.Add(new Actions.Biomes.Tunnel());
            WorldGenerationActions.Add(new Actions.Biomes.SpawnArea());

            // TODO: Load from json
            GlobalContext = new GlobalContext();
        }

        /// <summary>
        /// 現在のインスタンス
        /// </summary>
        public static WorldGenerationRunner CurrentRunner { get; private set; }

        /// <summary>
        /// ワールド生成アクションリスト。このリスト順で生成が実行される。
        /// </summary>
        public ObservableCollection<IWorldGenerationAction<ActionContext>> WorldGenerationActions { get; } = new ObservableCollection<IWorldGenerationAction<ActionContext>>();

        /// <summary>
        /// 全体から参照されるコンテキスト。
        /// 通常は`WorldGenerationRunner.CurrentRunner.GlobalContext`でアクセスされる。
        /// </summary>
        public GlobalContext GlobalContext { get; }

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
                    bool success = action.Run(sandbox);
                    if (!success)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
