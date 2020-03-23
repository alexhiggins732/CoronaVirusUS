using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoronaVirusUS
{
    static class ActionExtensions
    {
        public static void Run(this Action action, string logFile)
        {
            while (true)
            {
                try
                {
                    var t = Task.Run(() => action());
                    t.Wait();

                }
                catch (ThreadAbortException threadEx)
                {
                    string message = $"[{DateTime.Now}] {threadEx.Message}: {threadEx.ToString()}\r\n";
                    System.IO.File.AppendAllText(logFile, message);
                }
                catch (Exception ex)
                {
                    string message = $"[{DateTime.Now}] {ex.Message}: {ex.ToString()}\r\n";
                    File.AppendAllText(logFile, message);
                }
            }
        }
    }
}
