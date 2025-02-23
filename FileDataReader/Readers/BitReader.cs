using System.Runtime.CompilerServices;
using System.Text;

namespace FileDataReader;

internal class BitReader
{
    private readonly byte[] m_array;

    private int m_readPos;

    private int m_readOffset;

    public int Position
    {
        get
        {
            return m_readPos;
        }
        set
        {
            m_readPos = value;
        }
    }

    public int Offset
    {
        get
        {
            return m_readOffset;
        }
        set
        {
            m_readOffset = value;
        }
    }

    public BitReader(byte[] data)
    {
        m_array = data;
    }

    public BitReader(byte[] data, int offset)
    {
        m_array = data;
        m_readOffset = offset;
    }

    public uint ReadUInt32(int numBits)
    {
        uint result = Unsafe.As<byte, uint>(ref m_array[m_readOffset + (m_readPos >> 3)]) << 32 - numBits - (m_readPos & 7) >> 32 - numBits;
        m_readPos += numBits;
        return result;
    }

    public ulong ReadUInt64(int numBits)
    {
        ulong result = Unsafe.As<byte, ulong>(ref m_array[m_readOffset + (m_readPos >> 3)]) << 64 - numBits - (m_readPos & 7) >> 64 - numBits;
        m_readPos += numBits;
        return result;
    }

    public unsafe Value32 ReadValue32(int numBits)
    {
        ulong num = ReadUInt32(numBits);
        return *(Value32*)&num;
    }

    public unsafe Value64 ReadValue64(int numBits)
    {
        ulong num = ReadUInt64(numBits);
        return *(Value64*)&num;
    }

    public unsafe Value64 ReadValue64Signed(int numBits)
    {
        ulong num = ReadUInt64(numBits);
        ulong num2 = (ulong)(1L << numBits - 1);
        num = (num2 ^ num) - num2;
        return *(Value64*)&num;
    }

    public string ReadCString()
    {
        List<byte> list = new List<byte>(32);
        uint num;
        while ((num = ReadUInt32(8)) != 0)
        {
            list.Add((byte)num);
        }

        return Encoding.UTF8.GetString(list.ToArray());
    }

    public override int GetHashCode()
    {
        int num = 0;
        for (int i = 0; i < m_array.Length; i++)
        {
            num += m_array[i];
            num += num << 10;
            num ^= num >> 6;
        }

        num += num << 3;
        num ^= num >> 11;
        return num + (num << 15);
    }

    public BitReader Clone()
    {
        return new BitReader(m_array);
    }
}