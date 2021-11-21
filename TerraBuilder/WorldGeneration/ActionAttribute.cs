namespace TerraBuilder.WorldGeneration
{
    using System;

    /// <summary>
    /// ワールド生成アクションにこの属性を付与することで、ジェネレータから利用可能になる
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : Attribute
    {
    }
}
