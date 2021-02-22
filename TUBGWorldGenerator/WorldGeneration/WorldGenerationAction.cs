namespace TUBGWorldGenerator.WorldGeneration
{
    public interface IWorldGenerationAction<out T>
        where T : ActionContext
    {
        string Name { get; }

        string Description { get; }

        T Context { get; }

        bool Run(WorldSandbox sandbox);
    }
}
