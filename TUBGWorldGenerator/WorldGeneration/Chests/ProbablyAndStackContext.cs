namespace TUBGWorldGenerator.WorldGeneration.Chests
{
    public class ProbablyAndStackContext<T>
        where T : ActionContext
    {
        public ProbablyAndStackContext(T innerContext)
        {
            Context = innerContext;
        }

        public T Context { get; }

        public int Min { get; }

        public int Max { get; }

        public double Probably { get; set; }
    }
}
