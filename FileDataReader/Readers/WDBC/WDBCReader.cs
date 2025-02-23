using System.Text;

namespace FileDataReader;

internal class WDBCReader : BaseReader
{
    private const int HeaderSize = 20;

    private const uint WDBCFmtSig = 1128416343u;

    public WDBCReader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDBCReader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 20)
        {
            throw new InvalidDataException("WDBC file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 1128416343)
        {
            throw new InvalidDataException("WDBC file is corrupted!");
        }

        RecordsCount = binaryReader.ReadInt32();
        FieldsCount = binaryReader.ReadInt32();
        RecordSize = binaryReader.ReadInt32();
        StringTableSize = binaryReader.ReadInt32();
        if (RecordsCount != 0)
        {
            recordsData = binaryReader.ReadBytes(RecordsCount * RecordSize);
            Array.Resize(ref recordsData, recordsData.Length + 8);
            for (int i = 0; i < RecordsCount; i++)
            {
                BitReader data = new BitReader(recordsData)
                {
                    Position = i * RecordSize * 8
                };
                IDBRow value = new WDBCRow(this, data, i);
                _Records.Add(i, value);
            }

            m_stringsTable = new Dictionary<long, string>(StringTableSize / 32);
            long position;
            for (int j = 0; j < StringTableSize; j += (int)(binaryReader.BaseStream.Position - position))
            {
                position = binaryReader.BaseStream.Position;
                m_stringsTable[j] = binaryReader.ReadCString();
            }
        }
    }
}