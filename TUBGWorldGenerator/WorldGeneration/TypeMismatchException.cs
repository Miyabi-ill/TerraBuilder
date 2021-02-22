using System;

namespace TUBGWorldGenerator.WorldGeneration
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException() : base()
        {
        }

        public TypeMismatchException(string message) : base(message)
        {
        }

        public TypeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
