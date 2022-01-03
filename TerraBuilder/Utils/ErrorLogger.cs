namespace TerraBuilder.Utils
{
    using System;
    using System.IO;

    public static class ErrorLogger
    {
        private static readonly string LogFilePath = "log.txt";

        public static void Log(Exception e)
        {
            try
            {
                using (var sw = new StreamWriter(LogFilePath, append: true))
                {
                    sw.WriteLine(e.Message);
                    sw.WriteLine(e.StackTrace);
                }
            }
            catch { }
        }
    }
}
