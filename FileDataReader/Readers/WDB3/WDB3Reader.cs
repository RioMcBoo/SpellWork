using System.Text;

namespace FileDataReader;

internal class WDB3Reader : BaseReader
{
    private const int HeaderSize = 48;

    private const uint WDB3FmtSig = 859980887u;

    private readonly Dictionary<uint, DB2Flags> FlagTable = new Dictionary<uint, DB2Flags>
    {
        {
            3348406326u,
            DB2Flags.Sparse
        },
        {
            2442913102u,
            DB2Flags.Sparse
        },
        {
            2982519032u,
            DB2Flags.Sparse | DB2Flags.SecondaryKey
        }
    };

    public WDB3Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDB3Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 48)
        {
            throw new InvalidDataException("WDB3 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 859980887)
        {
            throw new InvalidDataException("WDB3 file is corrupted!");
        }

        RecordsCount = binaryReader.ReadInt32();
        FieldsCount = binaryReader.ReadInt32();
        RecordSize = binaryReader.ReadInt32();
        StringTableSize = binaryReader.ReadInt32();
        TableHash = binaryReader.ReadUInt32();
        binaryReader.ReadUInt32();
        binaryReader.ReadUInt32();
        MinIndex = binaryReader.ReadInt32();
        MaxIndex = binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num = binaryReader.ReadInt32();
        if (RecordsCount == 0)
        {
            return;
        }

        if (FlagTable.TryGetValue(TableHash, out var value))
        {
            Flags |= value;
        }

        if (Flags.HasFlagExt(DB2Flags.Sparse))
        {
            int num2 = MaxIndex - MinIndex + 1;
            m_sparseEntries = new List<SparseEntry>(num2);
            m_copyData = new Dictionary<int, int>(num2);
            Dictionary<uint, int> dictionary = new Dictionary<uint, int>(num2);
            for (int i = 0; i < num2; i++)
            {
                SparseEntry item = binaryReader.Read<SparseEntry>();
                if (item.Offset != 0 && item.Size != 0)
                {
                    if (dictionary.TryGetValue(item.Offset, out var value2))
                    {
                        m_copyData[MinIndex + i] = value2;
                        continue;
                    }

                    m_sparseEntries.Add(item);
                    dictionary.Add(item.Offset, MinIndex + i);
                }
            }

            if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
            {
                m_foreignKeyData = binaryReader.ReadArray<int>(MaxIndex - MinIndex + 1);
            }

            recordsData = binaryReader.ReadBytes(m_sparseEntries.Sum((SparseEntry x) => x.Size));
        }
        else
        {
            if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
            {
                m_foreignKeyData = binaryReader.ReadArray<int>(MaxIndex - MinIndex + 1);
            }

            recordsData = binaryReader.ReadBytes(RecordsCount * RecordSize);
            Array.Resize(ref recordsData, recordsData.Length + 8);
        }

        m_stringsTable = new Dictionary<long, string>(StringTableSize / 32);
        long position;
        for (int j = 0; j < StringTableSize; j += (int)(binaryReader.BaseStream.Position - position))
        {
            position = binaryReader.BaseStream.Position;
            m_stringsTable[j] = binaryReader.ReadCString();
        }

        if (binaryReader.BaseStream.Position + num < binaryReader.BaseStream.Length)
        {
            m_indexData = binaryReader.ReadArray<int>(RecordsCount);
            Flags |= DB2Flags.Index;
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

            IDBRow value3 = new WDB3Row(this, bitReader, Flags.HasFlagExt(DB2Flags.Index) ? m_indexData[l] : -1, l);
            _Records.Add(l, value3);
        }
    }
}