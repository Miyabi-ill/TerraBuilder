// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration
{
    using System.Collections.Generic;
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
        T Config { get; }

        /// <summary>
        /// このレイヤーをサンドボックスに適用する.
        /// </summary>
        /// <param name="runner">ワールド生成ランナー.</param>
        /// <param name="sandbox">ワールドサンドボックス.</param>
        /// <param name="generatedValueDict">レイヤーを適用した時に生成された値で、
        /// 後のレイヤーが<see cref="WorldGenerationRunner.GetGeneratedValue{TLayer, TValue}(string)"/>から利用できるようにする値.</param>
        /// <returns>実行の成否.成功した場合true.失敗した場合false.</returns>
        bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict);
    }
}
