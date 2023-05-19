namespace LZW;

public static class Compressor
{
    private static int[] DataToIds(byte[] data, int maxDictionarySize)
    {
        var root = CompressionTrie.InitRoot();
        var currentNode = root;
        var code = 256;
        var ids = new List<int>();
        foreach (var b in data)
        {
            if (currentNode.Next.TryGetValue(b, out var nextNode))
            {
                currentNode = nextNode;
                continue;
            }

            if (code < maxDictionarySize)
                currentNode.Next[b] = CompressionTrie.InitTree(code++);
            ids.Add(currentNode.Code);
            currentNode = root.Next[b];
        }

        ids.Add(currentNode.Code);

        return ids.ToArray();
    }

    private static byte[] IdsToData(int[] ids)
    {
        var data = new List<byte>();
        var root = DecompressionTrie.InitRoot();
        var dict = new List<DecompressionTrie>(256);
        var code = 256;

        foreach (var (b, node) in root.Next)
            dict.Add(node);

        var currentNode = root;
        foreach (var id in ids)
        {
            if (id >= dict.Count)
            {
                var node = currentNode;
                while (node?.Parent != root)
                    node = node?.Parent;
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

            if (currentNode != root)
            {
                var nextNode = DecompressionTrie.InitTree(currentNode, str[0]);
                currentNode.Next[str[0]] = nextNode;
                while (dict.Count <= code)
                    dict.Add(null!);
                dict[code] = nextNode;
                code++;
            }

            currentNode = dict[id];
        }

        return data.ToArray();
    }

    public static byte[] Compress(byte[] data, int maxDictionarySize = 4096)
    {
        var ids = DataToIds(data, maxDictionarySize);
        var output = new List<byte>();
        int idBits = 8, currBit = 0;
        for (var id = 0; id < ids.Length; id++)
        {
            var newSize = (currBit + idBits + 7) / 8 + 8;
            output.AddRange(new byte[newSize - output.Count]);
            var bytes = BitConverter.GetBytes((ulong) ids[id] << currBit % 8);
            for (var k = 0; k < 2 + idBits / 8; k++)
                output[currBit / 8 + k] |= bytes[k];
            currBit += idBits;
            if (256 + id >= 1 << idBits)
                idBits++;
        }

        output.RemoveRange((currBit + 7) / 8, 8);
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        var ids = new List<int>();
        int idBits = 8, currBit = 0, lastBit = data.Length * 8;
        data = data.Concat(new byte[8]).ToArray();
        for (var id = 0; currBit + idBits <= lastBit; id++)
        {
            var x = BitConverter.ToUInt64(data, currBit / 8);
            ids.Add((int) (x >> currBit % 8) & (1 << idBits) - 1);
            currBit += idBits;
            if (256 + id >= 1 << idBits)
                idBits++;
        }

        return IdsToData(ids.ToArray());
    }
}