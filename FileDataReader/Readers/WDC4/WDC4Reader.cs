using System.Text;

namespace FileDataReader;

internal class WDC4Reader : BaseEncryptionSupportingReader
{
    private const int HeaderSize = 72;

    private const uint WDC4FmtSig = 876823639u;

    public WDC4Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDC4Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 72)
        {
            throw new InvalidDataException("WDC4 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 876823639)
        {
            throw new InvalidDataException("WDC4 file is corrupted!");
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
        int num = binaryReader.ReadInt32();
        if (num == 0 || RecordsCount == 0)
        {
            return;
        }

        SectionHeaderWDC4[] array = binaryReader.ReadArray<SectionHeaderWDC4>(num);
        m_sections = array.OfType<IEncryptableDatabaseSection>().ToList();
        m_encryptedIDs = new Dictionary<ulong, int[]>();
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

        for (int l = 0; l < num; l++)
        {
            if (array[l].TactKeyLookup != 0L)
            {
                int size = binaryReader.ReadInt32();
                int[] array2 = binaryReader.ReadArray<int>(size);
                if (m_encryptedIDs.TryGetValue(array[l].TactKeyLookup, out var value))
                {
                    m_encryptedIDs[array[l].TactKeyLookup] = value.Concat(array2).ToArray();
                }
                else
                {
                    m_encryptedIDs.Add(array[l].TactKeyLookup, array2);
                }
            }
        }

        int num2 = 0;
        int num3 = 0;
        SectionHeaderWDC4[] array3 = array;
        for (int m = 0; m < array3.Length; m++)
        {
            SectionHeaderWDC4 sectionHeaderWDC = array3[m];
            binaryReader.BaseStream.Position = sectionHeaderWDC.FileOffset;
            if (!Flags.HasFlagExt(DB2Flags.Sparse))
            {
                recordsData = binaryReader.ReadBytes(sectionHeaderWDC.NumRecords * RecordSize);
                Array.Resize(ref recordsData, recordsData.Length + 8);
                if (m_stringsTable == null)
                {
                    m_stringsTable = new Dictionary<long, string>(sectionHeaderWDC.StringTableSize / 32);
                }

                long position;
                for (int n = 0; n < sectionHeaderWDC.StringTableSize; n += (int)(binaryReader.BaseStream.Position - position))
                {
                    position = binaryReader.BaseStream.Position;
                    m_stringsTable[n + num2] = binaryReader.ReadCString();
                }

                num2 += sectionHeaderWDC.StringTableSize;
            }
            else
            {
                recordsData = binaryReader.ReadBytes(sectionHeaderWDC.OffsetRecordsEndOffset - sectionHeaderWDC.FileOffset);
                if (binaryReader.BaseStream.Position != sectionHeaderWDC.OffsetRecordsEndOffset)
                {
                    throw new Exception("reader.BaseStream.Position != section.OffsetRecordsEndOffset");
                }
            }

            if (sectionHeaderWDC.TactKeyLookup != 0L && Array.TrueForAll(recordsData, (x) => x == 0))
            {
                bool flag = false;
                if (sectionHeaderWDC.IndexDataSize > 0 || sectionHeaderWDC.CopyTableCount > 0)
                {
                    flag = binaryReader.ReadInt32() == 0;
                    binaryReader.BaseStream.Position -= 4L;
                }
                else if (sectionHeaderWDC.OffsetMapIDCount > 0)
                {
                    flag = binaryReader.Read<SparseEntry>().Size == 0;
                    binaryReader.BaseStream.Position -= 6L;
                }
                else
                {
                    flag = true;
                }

                if (flag)
                {
                    num3 += sectionHeaderWDC.NumRecords;
                    continue;
                }
            }

            m_indexData = binaryReader.ReadArray<int>(sectionHeaderWDC.IndexDataSize / 4);
            if (m_indexData.Length != 0 && m_indexData.All((x) => x == 0))
            {
                m_indexData = Enumerable.Range(MinIndex + num3, sectionHeaderWDC.NumRecords).ToArray();
            }

            if (sectionHeaderWDC.CopyTableCount > 0)
            {
                if (m_copyData == null)
                {
                    m_copyData = new Dictionary<int, int>();
                }

                for (int num4 = 0; num4 < sectionHeaderWDC.CopyTableCount; num4++)
                {
                    int num5 = binaryReader.ReadInt32();
                    int num6 = binaryReader.ReadInt32();
                    if (num5 != num6)
                    {
                        m_copyData[num5] = num6;
                    }
                }
            }

            if (sectionHeaderWDC.OffsetMapIDCount > 0)
            {
                if (TableHash == 145293629)
                {
                    binaryReader.BaseStream.Position += 4 * sectionHeaderWDC.OffsetMapIDCount;
                }

                m_sparseEntries = binaryReader.ReadArray<SparseEntry>(sectionHeaderWDC.OffsetMapIDCount).ToList();
            }

            if (sectionHeaderWDC.OffsetMapIDCount > 0 && Flags.HasFlag(DB2Flags.SecondaryKey))
            {
                int[] array4 = binaryReader.ReadArray<int>(sectionHeaderWDC.OffsetMapIDCount);
                if (sectionHeaderWDC.IndexDataSize > 0 && m_indexData.Length != array4.Length)
                {
                    throw new Exception("m_indexData.Length != sparseIndexData.Length");
                }

                m_indexData = array4;
            }

            ReferenceData referenceData = new ReferenceData();
            if (sectionHeaderWDC.ParentLookupDataSize > 0)
            {
                referenceData.NumRecords = binaryReader.ReadInt32();
                referenceData.MinId = binaryReader.ReadInt32();
                referenceData.MaxId = binaryReader.ReadInt32();
                ReferenceEntry[] array5 = binaryReader.ReadArray<ReferenceEntry>(referenceData.NumRecords);
                for (int num7 = 0; num7 < array5.Length; num7++)
                {
                    referenceData.Entries[array5[num7].Index] = array5[num7].Id;
                }
            }

            if (sectionHeaderWDC.OffsetMapIDCount > 0 && !Flags.HasFlag(DB2Flags.SecondaryKey))
            {
                int[] array6 = binaryReader.ReadArray<int>(sectionHeaderWDC.OffsetMapIDCount);
                if (sectionHeaderWDC.IndexDataSize > 0 && m_indexData.Length != array6.Length)
                {
                    throw new Exception("m_indexData.Length != sparseIndexData.Length");
                }

                m_indexData = array6;
            }

            int num8 = 0;
            for (int num9 = 0; num9 < sectionHeaderWDC.NumRecords; num9++)
            {
                BitReader bitReader = new BitReader(recordsData)
                {
                    Position = 0
                };
                if (Flags.HasFlagExt(DB2Flags.Sparse))
                {
                    bitReader.Position = num8;
                    num8 += m_sparseEntries[num9].Size * 8;
                }
                else
                {
                    bitReader.Offset = num9 * RecordSize;
                }

                int value2;
                if (Flags.HasFlag(DB2Flags.SecondaryKey))
                {
                    referenceData.Entries.TryGetValue(m_indexData[num9], out value2);
                }
                else
                {
                    referenceData.Entries.TryGetValue(num9, out value2);
                }

                IDBRow value3 = new WDC4Row(this, bitReader, sectionHeaderWDC.IndexDataSize != 0 ? m_indexData[num9] : -1, value2, num9 + num3);
                _Records.Add(_Records.Count, value3);
            }

            num3 += sectionHeaderWDC.NumRecords;
        }
    }
}