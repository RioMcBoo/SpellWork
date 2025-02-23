using System.Text;

namespace FileDataReader;

internal class WDB2Reader : BaseReader
{
    private const int HeaderSize = 28;

    private const int ExtendedHeaderSize = 48;

    private const uint WDB2FmtSig = 843203671u;

    public WDB2Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDB2Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 28)
        {
            throw new InvalidDataException("WDB2 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 843203671)
        {
            throw new InvalidDataException("WDB2 file is corrupted!");
        }

        RecordsCount = binaryReader.ReadInt32();
        FieldsCount = binaryReader.ReadInt32();
        RecordSize = binaryReader.ReadInt32();
        StringTableSize = binaryReader.ReadInt32();
        TableHash = binaryReader.ReadUInt32();
        uint num = binaryReader.ReadUInt32();
        binaryReader.ReadUInt32();
        if (RecordsCount == 0)
        {
            return;
        }

        if (num > 12880)
        {
            if (binaryReader.BaseStream.Length < 48)
            {
                throw new InvalidDataException("WDB2 file is corrupted!");
            }

            MinIndex = binaryReader.ReadInt32();
            MaxIndex = binaryReader.ReadInt32();
            binaryReader.ReadInt32();
            binaryReader.ReadInt32();
            if (MaxIndex > 0)
            {
                int num2 = MaxIndex - MinIndex + 1;
                binaryReader.BaseStream.Position += num2 * 4;
                binaryReader.BaseStream.Position += num2 * 2;
            }
        }

        recordsData = binaryReader.ReadBytes(RecordsCount * RecordSize);
        Array.Resize(ref recordsData, recordsData.Length + 8);
        for (int i = 0; i < RecordsCount; i++)
        {
            BitReader data = new BitReader(recordsData)
            {
                Position = i * RecordSize * 8
            };
            IDBRow value = new WDB2Row(this, data, i);
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