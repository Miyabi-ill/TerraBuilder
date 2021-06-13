namespace TerraBuilder.WorldGeneration
{
    /// <summary>
    /// ワールド生成アクションの基底インターフェース。
    /// </summary>
    /// <typeparam name="T">使用するコンテキストのクラス</typeparam>
    public interface IWorldGenerationAction<out T>
        where T : ActionContext
    {
        /// <summary>
        /// アクション名。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// アクションの説明。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// アクションのコンテキスト。
        /// </summary>
        T Context { get; }

        /// <summary>
        /// アクションを実行する。
        /// </summary>
        /// <param name="sandbox">ワールドのサンドボックス</param>
        /// <returns>実行の成否。成功した場合true</returns>
        bool Run(WorldSandbox sandbox);
    }
}
