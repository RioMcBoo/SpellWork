using System.Runtime.CompilerServices;
using System.Text;

namespace FileDataReader;

internal class WDB6Reader : BaseReader
{
    private const int HeaderSize = 56;

    private const uint WDB6FmtSig = 910312535u;

    private readonly Dictionary<byte, short> CommonTypeBits = new Dictionary<byte, short>
    {
        { 0, 0 },
        { 1, 16 },
        { 2, 24 },
        { 3, 0 },
        { 4, 0 }
    };

    public WDB6Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDB6Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 56)
        {
            throw new InvalidDataException("WDB6 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 910312535)
        {
            throw new InvalidDataException("WDB6 file is corrupted!");
        }

        RecordsCount = binaryReader.ReadInt32();
        FieldsCount = binaryReader.ReadInt32();
        RecordSize = binaryReader.ReadInt32();
        StringTableSize = binaryReader.ReadInt32();
        TableHash = binaryReader.ReadUInt32();
        LayoutHash = binaryReader.ReadUInt32();
        MinIndex = binaryReader.ReadInt32();
        MaxIndex = binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num = binaryReader.ReadInt32();
        Flags = (DB2Flags)binaryReader.ReadUInt16();
        IdFieldIndex = binaryReader.ReadUInt16();
        int newSize = binaryReader.ReadInt32();
        int num2 = binaryReader.ReadInt32();
        if (RecordsCount == 0)
        {
            return;
        }

        m_meta = binaryReader.ReadArray<FieldMetaData>(FieldsCount);
        if (!Flags.HasFlagExt(DB2Flags.Sparse))
        {
            recordsData = binaryReader.ReadBytes(RecordsCount * RecordSize);
            Array.Resize(ref recordsData, recordsData.Length + 8);
            m_stringsTable = new Dictionary<long, string>(StringTableSize / 32);
            long position;
            for (int i = 0; i < StringTableSize; i += (int)(binaryReader.BaseStream.Position - position))
            {
                position = binaryReader.BaseStream.Position;
                m_stringsTable[i] = binaryReader.ReadCString();
            }
        }
        else
        {
            recordsData = binaryReader.ReadBytes(StringTableSize - (int)binaryReader.BaseStream.Position);
            int num3 = MaxIndex - MinIndex + 1;
            m_sparseEntries = new List<SparseEntry>(num3);
            m_copyData = new Dictionary<int, int>(num3);
            Dictionary<uint, int> dictionary = new Dictionary<uint, int>(num3);
            for (int j = 0; j < num3; j++)
            {
                SparseEntry item = binaryReader.Read<SparseEntry>();
                if (item.Offset != 0 && item.Size != 0)
                {
                    if (dictionary.TryGetValue(item.Offset, out var value))
                    {
                        m_copyData[MinIndex + j] = value;
                        continue;
                    }

                    m_sparseEntries.Add(item);
                    dictionary.Add(item.Offset, MinIndex + j);
                }
            }
        }

        if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
        {
            m_foreignKeyData = binaryReader.ReadArray<int>(MaxIndex - MinIndex + 1);
        }

        if (Flags.HasFlagExt(DB2Flags.Index))
        {
            m_indexData = binaryReader.ReadArray<int>(RecordsCount);
        }

        if (m_copyData == null)
        {
            m_copyData = new Dictionary<int, int>(num / 8);
        }

        for (int k = 0; k < num / 8; k++)
        {
            m_copyData[binaryReader.ReadInt32()] = binaryReader.ReadInt32();
        }

        if (num2 > 0)
        {
            Array.Resize(ref m_meta, newSize);
            int num4 = binaryReader.ReadInt32();
            m_commonData = new Dictionary<int, Value32>[num4];
            bool flag = (num2 - 4 - num4 * 5) % 8 == 0;
            for (int l = 0; l < num4; l++)
            {
                int num5 = binaryReader.ReadInt32();
                byte key = binaryReader.ReadByte();
                int count = flag ? 4 : 32 - CommonTypeBits[key] >> 3;
                if (l > FieldsCount)
                {
                    m_meta[l] = new FieldMetaData
                    {
                        Bits = CommonTypeBits[key],
                        Offset = (short)(m_meta[l - 1].Offset + (32 - m_meta[l - 1].Bits >> 3))
                    };
                }

                Dictionary<int, Value32> dictionary2 = new Dictionary<int, Value32>(num5);
                for (int m = 0; m < num5; m++)
                {
                    int key2 = binaryReader.ReadInt32();
                    Value32 value2 = Unsafe.ReadUnaligned<Value32>(ref binaryReader.ReadBytes(count)[0]);
                    dictionary2.Add(key2, value2);
                }

                m_commonData[l] = dictionary2;
            }
        }

        int num6 = 0;
        for (int n = 0; n < RecordsCount; n++)
        {
            BitReader bitReader = new BitReader(recordsData)
            {
                Position = 0
            };
            if (Flags.HasFlagExt(DB2Flags.Sparse))
            {
                bitReader.Position = num6;
                num6 += m_sparseEntries[n].Size * 8;
            }
            else
            {
                bitReader.Offset = n * RecordSize;
            }

            IDBRow value3 = new WDB6Row(this, bitReader, Flags.HasFlagExt(DB2Flags.Index) ? m_indexData[n] : -1, n);
            _Records.Add(n, value3);
        }
    }
}