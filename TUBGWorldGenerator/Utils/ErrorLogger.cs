namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
