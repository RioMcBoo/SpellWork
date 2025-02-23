using System.Text;

namespace FileDataReader;

internal class WDB5Reader : BaseReader
{
    private const int HeaderSize = 52;

    private const uint WDB5FmtSig = 893535319u;

    public WDB5Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDB5Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 52)
        {
            throw new InvalidDataException("WDB5 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 893535319)
        {
            throw new InvalidDataException("WDB5 file is corrupted!");
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
            int num2 = MaxIndex - MinIndex + 1;
            m_sparseEntries = new List<SparseEntry>(num2);
            m_copyData = new Dictionary<int, int>(num2);
            Dictionary<uint, int> dictionary = new Dictionary<uint, int>(num2);
            for (int j = 0; j < num2; j++)
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

        int num3 = 0;
        for (int l = 0; l < RecordsCount; l++)
        {
            BitReader bitReader = new BitReader(recordsData)
            {
                Position = 0
            };
            if (Flags.HasFlagExt(DB2Flags.Sparse))
            {
                bitReader.Position = num3;
                num3 += m_sparseEntries[l].Size * 8;
            }
            else
            {
                bitReader.Offset = l * RecordSize;
            }

            IDBRow value2 = new WDB5Row(this, bitReader, Flags.HasFlagExt(DB2Flags.Index) ? m_indexData[l] : -1, l);
            _Records.Add(l, value2);
        }
    }
}