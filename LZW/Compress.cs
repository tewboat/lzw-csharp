namespace LZW;

using System.Collections.Generic;

public static class Compressor
{

    public static byte[] Compress(byte[] data, int maxDictionarySize = 4096)
    {
        var output = new List<byte> { 0 };
        int idBits = 8, currBit = 0;

        var root = CompressionTrie.InitRoot();
        var currentNode = root;
        var code = 256;

        foreach (var @byte in data)
        {
            if (currentNode.Next.TryGetValue(@byte, out var nextNode))
            {
                currentNode = nextNode;
                continue;
            }

            if (code < maxDictionarySize)
                currentNode.Next[@byte] = CompressionTrie.InitTree(code++);
            if (code >= 1 << idBits)
                idBits++;
            for (var i = idBits - 1; i >= 0; i--)
            {
                var bit = ((currentNode.Code & 1 << i) == 0 ? 0 : 1);
                output[^1] |= (byte)(bit << (7 - currBit));
                currBit += 1;
                if (currBit >= 8)
                {
                    output.Add(0);
                    currBit %= 8;
                }
            }

            currentNode = root.Next[@byte];
        }

        if (code >= 1 << idBits)
            idBits++;
        for (var i = idBits - 1; i >= 0; i--)
        {
            var bit = (currentNode.Code & 1 << i) == 0 ? 0 : 1;
            output[^1] |= (byte)(bit << (7 - currBit));
            currBit += 1;
            if (currBit >= 8)
            {
                output.Add(0);
                currBit %= 8;
            }
        }

        return output.ToArray();
    }

    public static byte[] Decompress(byte[] compressedData, int dictionarySize = 4096)
    {
        var bitReader = new BitReader(new MemoryStream(compressedData));
        bitReader.BitsInChunk = 9;

        var data = new List<byte>();

        var root = DecompressionTrie.InitRoot();
        var dict = new List<DecompressionTrie>(256);
        var code = 256;

        foreach (var (b, node) in root.Next)
            dict.Add(node);

        var currentNode = root;
        var id = bitReader.Read();
        while (id != -1)
        {
            if (id >= dict.Count)
            {
                var node = currentNode;
                while (node.Parent != root)
                    node = node.Parent;
                var extensionNode = DecompressionTrie.InitTree(currentNode, node.Byte);
                currentNode.Next[node.Byte] = extensionNode;
                while (dict.Count <= id)
                    dict.Add(null!);
                dict[id] = extensionNode;
            }

            var str = new List<byte>();
            var pointer = dict[id];
            while (pointer != root)
            {
                str.Add(pointer!.Byte);
                pointer = pointer.Parent;
            }

            str.Reverse();
            data.AddRange(str);

            if (currentNode != root && code < dictionarySize)
            {
                var nextNode = DecompressionTrie.InitTree(currentNode, str[0]);
                currentNode.Next[str[0]] = nextNode;
                while (dict.Count <= code)
                    dict.Add(null!);
                dict[code] = nextNode;
                code++;
            }

            currentNode = dict[id];

            if (code + 2 >= 1 << bitReader.BitsInChunk)
                bitReader.BitsInChunk++;

            id = bitReader.Read();
        }

        return data.ToArray();
    }
}