using Spectre.Console.Cli;

namespace LZW.Commands;

internal sealed class CompressCommand : Command<CompressCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[FilePath]")] public string FilePath { get; set; } = null!;

        [CommandOption("-o|--output")] public string? OutputFilePath { get; set; }

        [CommandOption("-d|--dictionarySize")] public int? DictionarySize { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var data = File.ReadAllBytes(settings.FilePath);
        var dictionarySize = settings.DictionarySize ?? 4096;
        var compressed = Compressor.Compress(data, dictionarySize);
        File.WriteAllBytes(settings.OutputFilePath ?? settings.FilePath + ".compessed", BitConverter.GetBytes(dictionarySize).Concat(compressed).ToArray());
        Console.WriteLine($"'{settings.FilePath}' {data.Length:n0} B to {compressed.Length:n0} B");

        return 0;
    }
}