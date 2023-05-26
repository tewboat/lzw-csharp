using System.Diagnostics;
using Spectre.Console.Cli;

namespace LZW.Commands;

internal sealed class StatisticsCommand : Command<StatisticsCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[FilePath]")] public string FilePath { get; set; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (settings.FilePath == null)
            throw new ArgumentNullException(nameof(settings.FilePath));

        var data = File.ReadAllBytes(settings.FilePath);

        for (var i = 512; i < 1 << 20; i *= 2)
            Compressor.Compress(data, i);

        Console.WriteLine($"Data length before compressing: {data.Length}");
        Console.WriteLine("MaxDictionarySize | CompressionRatio | Time, ms");
        Console.WriteLine("-----------------------------------------------");

        for (var i = 512; i < 1 << 20; i *= 2)
        {
            const int repetitionsCount = 50;
            var totalTime = 0.0;
            var compressedSize = 0;
            for (var j = 0; j < repetitionsCount; j++)
            {
                GC.TryStartNoGCRegion(100000000);
                var stopwatch = Stopwatch.StartNew();
                var compressed = Compressor.Compress(data, i);
                compressedSize = compressed.Length;
                stopwatch.Stop();
                totalTime += stopwatch.ElapsedMilliseconds;
                GC.EndNoGCRegion();
                GC.Collect();
            }
            
            Console.WriteLine(
                $"{i,17} | {(float)data.Length / compressedSize,16} | {totalTime / repetitionsCount,8}");
        }

        return 0;
    }
}