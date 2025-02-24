using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountGen.Utils
{
    internal static class LoggingHelper
    {
        public enum LogType
        {
            Info,
            Success,
            Error
        }

        public static void Log(string message, LogType type = LogType.Info)
        {
            switch (type)
            {
                case LogType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;                
                    break;
                case LogType.Info:
                    break;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
