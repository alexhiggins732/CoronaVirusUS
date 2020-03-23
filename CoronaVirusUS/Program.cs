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
            Action action = null;
            if (args != null && args.Length > 0)
            {
                args = args.Select(x => x.ToLower()).ToArray();
                if (args[0] == "usstats")
                {
                    action = () => UsStatProcessor.Run();
                    action.Run($"{nameof(UsStatProcessor)}.log");
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
