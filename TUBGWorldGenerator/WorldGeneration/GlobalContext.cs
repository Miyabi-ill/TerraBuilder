namespace TUBGWorldGenerator.WorldGeneration
{
    /// <summary>
    /// ワールド生成の全体を通じて使われる設定。
    /// </summary>
    public class GlobalContext : ActionContext
    {
        public int SurfaceLevel { get; } = 250;

        public int RespawnLevel { get; } = 100;
    }
}
