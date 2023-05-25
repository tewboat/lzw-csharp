namespace LZW;

using System.IO;

public class BitReader
{
    private Stream inputStream;
    private int bitsLeftInCurrentByte;
    private int currentByte;
    
    public BitReader(Stream inputStream)
    {
        this.inputStream = inputStream;
        this.BitsInChunk = 8; // default 8 bits in the chunk
    }
    
    public int BitsInChunk { get; set; }
    
    public Stream Stream
    {
        get { return inputStream; }
    }
    
    public int Read()
    {
        var retval = 0;
        for (var i = 0; i < BitsInChunk && retval != -1; i++)
        {
            if (bitsLeftInCurrentByte == 0)
            {
                currentByte = inputStream.ReadByte();
                bitsLeftInCurrentByte = 8;
            }

            if (currentByte == -1)
            {
                retval = -1;
            }
            else
            {
                retval <<= 1;
                retval |= ((currentByte >> (bitsLeftInCurrentByte - 1)) & 0x1);
                bitsLeftInCurrentByte--;
            }
        }

        return retval;
    }
}