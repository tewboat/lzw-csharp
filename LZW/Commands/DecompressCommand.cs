using Spectre.Console.Cli;

namespace LZW.Commands;

internal sealed class DecompressCommand : Command<DecompressCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[FilePath]")] public string FilePath { get; set; } = null!;

        [CommandArgument(1, "[OutputFilePath]")]
        public string OutputFilePath { get; set; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var data = File.ReadAllBytes(settings.FilePath);
        var decompessed = Compressor.Decompress(data);
        File.WriteAllBytes(settings.OutputFilePath, decompessed);
        Console.WriteLine($"'{settings.FilePath}' successfully decompressed!");

        return 0;
    }
}