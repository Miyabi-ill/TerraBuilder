// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration
{
    using TerraBuilder.WorldEdit;

    /// <summary>
    /// ワールド生成レイヤーの基底インターフェース.
    /// </summary>
    /// <typeparam name="T">使用するコンフィグクラス.</typeparam>
    public interface IWorldGenerationLayer<out T>
        where T : LayerConfig
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
        /// このレイヤーをサンドボックスに適用する.
        /// </summary>
        /// <param name="sandbox">ワールドサンドボックス.</param>
        /// <returns>実行の成否.成功した場合true.失敗した場合false.</returns>
        bool Apply(WorldSandbox sandbox);
    }
}
