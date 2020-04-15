using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaVirusUS
{
    class Program
    {
        static void Main(string[] args)
        {
            var probable = UsDataProcessor.GetNyProbable();
            Action action = null;
            if (args != null && args.Length > 0)
            {
                args = args.Select(x => x.ToLower()).ToArray();
                if (args[0] == "usstats")
                {
                    action = () => UsStatProcessor.Run();
                    action.Run($"{nameof(UsStatProcessor)}.log");
                }
                else if (args[0] == "nyc")
                {
                    //action = () => Console.Write(UsDataProcessor.GetNyProbable());
                    //action.Run($"{nameof(UsDataProcessor.GetNyProbable)}.log");
                    Console.Write(UsDataProcessor.GetNyProbable());
                    return;
                }
            }
            var ex = new Exception("Invalid arguments", new Exception(string.Join(" ", args)));
            string message = $"[{DateTime.Now}] {ex.Message}: {ex.ToString()}\r\n";
            var logFile = nameof(CoronaVirusUS) + ".log";
            File.AppendAllText(logFile, message);
            return;
        }

    }
}
