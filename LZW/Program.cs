using LZW.Commands;
using Spectre.Console.Cli;

namespace LZW
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.AddCommand<StatisticsCommand>("stat");
                config.AddCommand<CompressCommand>("compress");
                config.AddCommand<DecompressCommand>("decompress");
            });

            await app.RunAsync(args);
        }
    }
}