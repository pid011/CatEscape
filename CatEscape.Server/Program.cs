using System;
using System.Threading.Tasks;
using CatEscape.Server;
using Serilog;

const int port = 10200;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var server = new GameServer(port);

var stopCommandTask = Task.Run(() =>
{
    while (true)
    {
        Console.WriteLine("If you want to stop, enter 'stop'");
        var answer = Console.ReadLine();
        if (answer is "stop")
        {
            return;
        }
    }
});

var runTask = server.RunAsync();
await Task.WhenAny(runTask, stopCommandTask);
