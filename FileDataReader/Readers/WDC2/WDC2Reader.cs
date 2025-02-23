using System.Text;

namespace FileDataReader;

internal class WDC2Reader : BaseEncryptionSupportingReader
{
    private const int HeaderSize = 72;

    private const uint WDC2FmtSig = 843269207u;

    private const uint CLS1FmtSig = 1129075505u;

    public WDC2Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDC2Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 72)
        {
            throw new InvalidDataException("WDC2 file is corrupted!");
        }

        uint num = binaryReader.ReadUInt32();
        if (num != 843269207 && num != 1129075505)
        {
            throw new InvalidDataException("WDC2 file is corrupted!");
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
        Flags = (DB2Flags)binaryReader.ReadUInt16();
        IdFieldIndex = binaryReader.ReadUInt16();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num2 = binaryReader.ReadInt32();
        if (num2 > 1)
        {
            throw new Exception("WDC2 only supports 1 section");
        }

        if (num2 == 0 || RecordsCount == 0)
        {
            return;
        }

        List<SectionHeader> list = binaryReader.ReadArray<SectionHeader>(num2).ToList();
        m_sections = list.OfType<IEncryptableDatabaseSection>().ToList();
        m_meta = binaryReader.ReadArray<FieldMetaData>(FieldsCount);
        m_columnMeta = binaryReader.ReadArray<ColumnMetaData>(FieldsCount);
        m_palletData = new Value32[m_columnMeta.Length][];
        for (int i = 0; i < m_columnMeta.Length; i++)
        {
            if (m_columnMeta[i].CompressionType == CompressionType.Pallet || m_columnMeta[i].CompressionType == CompressionType.PalletArray)
            {
                m_palletData[i] = binaryReader.ReadArray<Value32>((int)m_columnMeta[i].AdditionalDataSize / 4);
            }
        }

        m_commonData = new Dictionary<int, Value32>[m_columnMeta.Length];
        for (int j = 0; j < m_columnMeta.Length; j++)
        {
            if (m_columnMeta[j].CompressionType == CompressionType.Common)
            {
                Dictionary<int, Value32> dictionary = new Dictionary<int, Value32>((int)m_columnMeta[j].AdditionalDataSize / 8);
                m_commonData[j] = dictionary;
                for (int k = 0; k < m_columnMeta[j].AdditionalDataSize / 8; k++)
                {
                    dictionary[binaryReader.ReadInt32()] = binaryReader.Read<Value32>();
                }
            }
        }

        for (int l = 0; l < num2; l++)
        {
            binaryReader.BaseStream.Position = list[l].FileOffset;
            if (!Flags.HasFlagExt(DB2Flags.Sparse))
            {
                recordsData = binaryReader.ReadBytes(list[l].NumRecords * RecordSize);
                Array.Resize(ref recordsData, recordsData.Length + 8);
                m_stringsTable = new Dictionary<long, string>(list[l].StringTableSize / 32);
                long position;
                for (int m = 0; m < list[l].StringTableSize; m += (int)(binaryReader.BaseStream.Position - position))
                {
                    position = binaryReader.BaseStream.Position;
                    m_stringsTable[position] = binaryReader.ReadCString();
                }
            }
            else
            {
                recordsData = binaryReader.ReadBytes(list[l].SparseTableOffset - list[l].FileOffset);
                if (binaryReader.BaseStream.Position != list[l].SparseTableOffset)
                {
                    throw new Exception("reader.BaseStream.Position != sections[sectionIndex].SparseTableOffset");
                }

                int num3 = MaxIndex - MinIndex + 1;
                m_sparseEntries = new List<SparseEntry>(num3);
                m_copyData = new Dictionary<int, int>(num3);
                Dictionary<uint, int> dictionary2 = new Dictionary<uint, int>(num3);
                for (int n = 0; n < num3; n++)
                {
                    SparseEntry item = binaryReader.Read<SparseEntry>();
                    if (item.Offset != 0 && item.Size != 0)
                    {
                        if (dictionary2.TryGetValue(item.Offset, out var value))
                        {
                            m_copyData[MinIndex + n] = value;
                            continue;
                        }

                        m_sparseEntries.Add(item);
                        dictionary2.Add(item.Offset, MinIndex + n);
                    }
                }
            }

            m_indexData = binaryReader.ReadArray<int>(list[l].IndexDataSize / 4);
            if (m_indexData.Length != 0 && m_indexData.All((x) => x == 0))
            {
                m_indexData = Enumerable.Range(MinIndex, MaxIndex - MinIndex + 1).ToArray();
            }

            if (m_copyData == null)
            {
                m_copyData = new Dictionary<int, int>(list[l].CopyTableSize / 8);
            }

            for (int num4 = 0; num4 < list[l].CopyTableSize / 8; num4++)
            {
                m_copyData[binaryReader.ReadInt32()] = binaryReader.ReadInt32();
            }

            ReferenceData referenceData = new ReferenceData();
            if (list[l].ParentLookupDataSize > 0)
            {
                referenceData.NumRecords = binaryReader.ReadInt32();
                referenceData.MinId = binaryReader.ReadInt32();
                referenceData.MaxId = binaryReader.ReadInt32();
                ReferenceEntry[] array = binaryReader.ReadArray<ReferenceEntry>(referenceData.NumRecords);
                for (int num5 = 0; num5 < array.Length; num5++)
                {
                    referenceData.Entries[array[num5].Index] = array[num5].Id;
                }
            }

            int num6 = 0;
            for (int num7 = 0; num7 < RecordsCount; num7++)
            {
                BitReader bitReader = new BitReader(recordsData)
                {
                    Position = 0
                };
                if (Flags.HasFlagExt(DB2Flags.Sparse))
                {
                    bitReader.Position = num6;
                    num6 += m_sparseEntries[num7].Size * 8;
                }
                else
                {
                    bitReader.Offset = num7 * RecordSize;
                }

                referenceData.Entries.TryGetValue(num7, out var value2);
                IDBRow value3 = new WDC2Row(this, bitReader, list[l].FileOffset, list[l].IndexDataSize != 0 ? m_indexData[num7] : -1, value2, num7);
                _Records.Add(num7, value3);
            }
        }
    }
}