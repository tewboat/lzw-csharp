namespace LZW;

internal sealed class CompressionTrie
{
    public SortedDictionary<byte, CompressionTrie> Next { get; }
    public int Code;

    private CompressionTrie(int code)
    {
        Next = new SortedDictionary<byte, CompressionTrie>();
        Code = code;
    }

    public static CompressionTrie InitRoot()
    {
        var trie = new CompressionTrie(-1);
        for (var i = 0; i < 256; i++)
            trie.Next[(byte)i] = new CompressionTrie(i);
        return trie;
    }

    public static CompressionTrie InitTree(int code)
    {
        return new CompressionTrie(code);
    }
}

internal sealed class DecompressionTrie
{
    public SortedDictionary<byte, DecompressionTrie> Next { get; }
    public byte Byte { get; }
    public  DecompressionTrie? Parent { get; }

    private DecompressionTrie(DecompressionTrie? parent, byte @byte)
    {
        Next = new SortedDictionary<byte, DecompressionTrie>();
        Byte = @byte;
        Parent = parent;
    }

    public static DecompressionTrie InitRoot()
    {
        var trie = new DecompressionTrie(null, 0);
        for (var i = 0; i < 256; i++)
            trie.Next[(byte)i] = new DecompressionTrie(trie, (byte)i);
        return trie;
    }

    public static DecompressionTrie InitTree(DecompressionTrie parent, byte @byte)
    {
        return new DecompressionTrie(parent, @byte);
    }
}