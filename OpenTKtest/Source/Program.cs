using OpenTKTest;
using System;
using System.Diagnostics;

class Program
{
    [STAThread]
    static void Main()
    {
        TextWriterTraceListener debugLog = new TextWriterTraceListener(Console.Out);
        Debug.Listeners.Add(debugLog);

        using (Game game = new Game())
        {
            game.Run(60.0);
        }

        debugLog.Flush();
        debugLog.Close();
    }
}