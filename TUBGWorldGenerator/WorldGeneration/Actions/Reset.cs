namespace TUBGWorldGenerator.WorldGeneration.Actions
{
    public class Reset : IWorldGenerationAction<ActionContext>
    {
        public string Name => nameof(Reset);

        public string Description => "Reset all world parameters. Should be run first.";

        public ActionContext Context { get; }

        public bool Run()
        {
            return true;
        }
    }
}
