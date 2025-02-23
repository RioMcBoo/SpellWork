using System.Runtime.InteropServices;
using System.Text;

namespace FileDataReader;

internal class WDC1Reader : BaseReader
{
    private const int HeaderSize = 84;

    private const uint WDC1FmtSig = 826491991u;

    public WDC1Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDC1Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 84)
        {
            throw new InvalidDataException("WDC1 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 826491991)
        {
            throw new InvalidDataException("WDC1 file is corrupted!");
        }

        base.RecordsCount = binaryReader.ReadInt32();
        base.FieldsCount = binaryReader.ReadInt32();
        base.RecordSize = binaryReader.ReadInt32();
        base.StringTableSize = binaryReader.ReadInt32();
        base.TableHash = binaryReader.ReadUInt32();
        base.LayoutHash = binaryReader.ReadUInt32();
        base.MinIndex = binaryReader.ReadInt32();
        base.MaxIndex = binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num = binaryReader.ReadInt32();
        base.Flags = (DB2Flags)binaryReader.ReadUInt16();
        base.IdFieldIndex = binaryReader.ReadUInt16();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num2 = binaryReader.ReadInt32();
        int num3 = binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num4 = binaryReader.ReadInt32();
        if (base.RecordsCount == 0)
        {
            return;
        }

        m_meta = binaryReader.ReadArray<FieldMetaData>(base.FieldsCount);
        if (!base.Flags.HasFlagExt(DB2Flags.Sparse))
        {
            recordsData = binaryReader.ReadBytes(base.RecordsCount * base.RecordSize);
            Array.Resize(ref recordsData, recordsData.Length + 8);
            m_stringsTable = new Dictionary<long, string>(base.StringTableSize / 32);
            long position;
            for (int i = 0; i < base.StringTableSize; i += (int)(binaryReader.BaseStream.Position - position))
            {
                position = binaryReader.BaseStream.Position;
                m_stringsTable[i] = binaryReader.ReadCString();
            }
        }
        else
        {
            recordsData = binaryReader.ReadBytes(num2 - 84 - Marshal.SizeOf<FieldMetaData>() * base.FieldsCount);
            if (binaryReader.BaseStream.Position != num2)
            {
                throw new Exception("r.BaseStream.Position != sparseTableOffset");
            }

            int num5 = base.MaxIndex - base.MinIndex + 1;
            m_sparseEntries = new List<SparseEntry>(num5);
            m_copyData = new Dictionary<int, int>(num5);
            Dictionary<uint, int> dictionary = new Dictionary<uint, int>(num5);
            for (int j = 0; j < num5; j++)
            {
                SparseEntry item = binaryReader.Read<SparseEntry>();
                if (item.Offset != 0 && item.Size != 0)
                {
                    if (dictionary.TryGetValue(item.Offset, out var value))
                    {
                        m_copyData[base.MinIndex + j] = value;
                        continue;
                    }

                    m_sparseEntries.Add(item);
                    dictionary.Add(item.Offset, base.MinIndex + j);
                }
            }
        }

        m_indexData = binaryReader.ReadArray<int>(num3 / 4);
        if (m_copyData == null)
        {
            m_copyData = new Dictionary<int, int>(num / 8);
        }

        for (int k = 0; k < num / 8; k++)
        {
            m_copyData[binaryReader.ReadInt32()] = binaryReader.ReadInt32();
        }

        m_columnMeta = binaryReader.ReadArray<ColumnMetaData>(base.FieldsCount);
        m_palletData = new Value32[m_columnMeta.Length][];
        for (int l = 0; l < m_columnMeta.Length; l++)
        {
            if (m_columnMeta[l].CompressionType == CompressionType.Pallet || m_columnMeta[l].CompressionType == CompressionType.PalletArray)
            {
                m_palletData[l] = binaryReader.ReadArray<Value32>((int)m_columnMeta[l].AdditionalDataSize / 4);
            }
        }

        m_commonData = new Dictionary<int, Value32>[m_columnMeta.Length];
        for (int m = 0; m < m_columnMeta.Length; m++)
        {
            if (m_columnMeta[m].CompressionType == CompressionType.Common)
            {
                Dictionary<int, Value32> dictionary2 = new Dictionary<int, Value32>((int)m_columnMeta[m].AdditionalDataSize / 8);
                m_commonData[m] = dictionary2;
                for (int n = 0; n < m_columnMeta[m].AdditionalDataSize / 8; n++)
                {
                    dictionary2[binaryReader.ReadInt32()] = binaryReader.Read<Value32>();
                }
            }
        }

        ReferenceData referenceData = new ReferenceData();
        if (num4 > 0)
        {
            referenceData.NumRecords = binaryReader.ReadInt32();
            referenceData.MinId = binaryReader.ReadInt32();
            referenceData.MaxId = binaryReader.ReadInt32();
            ReferenceEntry[] array = binaryReader.ReadArray<ReferenceEntry>(referenceData.NumRecords);
            for (int num6 = 0; num6 < array.Length; num6++)
            {
                referenceData.Entries[array[num6].Index] = array[num6].Id;
            }
        }

        int num7 = 0;
        for (int num8 = 0; num8 < base.RecordsCount; num8++)
        {
            BitReader bitReader = new BitReader(recordsData)
            {
                Position = 0
            };
            if (base.Flags.HasFlagExt(DB2Flags.Sparse))
            {
                bitReader.Position = num7;
                num7 += m_sparseEntries[num8].Size * 8;
            }
            else
            {
                bitReader.Offset = num8 * base.RecordSize;
            }

            referenceData.Entries.TryGetValue(num8, out var value2);
            IDBRow value3 = new WDC1Row(this, bitReader, (num3 != 0) ? m_indexData[num8] : (-1), value2, num8);
            _Records.Add(num8, value3);
        }
    }
}