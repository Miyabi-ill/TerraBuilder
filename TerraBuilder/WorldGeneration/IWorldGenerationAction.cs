// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration
{
    using TerraBuilder.WorldEdit;

    /// <summary>
    /// ワールド生成アクションの基底インターフェース.
    /// </summary>
    /// <typeparam name="T">使用するコンテキストのクラス.</typeparam>
    public interface IWorldGenerationAction<out T>
        where T : ActionConfig
    {
        /// <summary>
        /// アクション名.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// アクションの説明.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// アクションのコンテキスト.
        /// </summary>
        T Context { get; }

        /// <summary>
        /// アクションを実行する.
        /// </summary>
        /// <param name="sandbox">ワールドサンドボックス.</param>
        /// <returns>実行の成否.成功した場合true.失敗した場合false.</returns>
        bool Run(WorldSandbox sandbox);
    }
}
