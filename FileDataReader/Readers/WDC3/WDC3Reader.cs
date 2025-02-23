using System.Text;

namespace FileDataReader;

internal class WDC3Reader : BaseEncryptionSupportingReader
{
    private const int HeaderSize = 72;

    private const uint WDC3FmtSig = 860046423u;

    public WDC3Reader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public WDC3Reader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 72)
        {
            throw new InvalidDataException("WDC3 file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 860046423)
        {
            throw new InvalidDataException("WDC3 file is corrupted!");
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
        base.Flags = (DB2Flags)binaryReader.ReadUInt16();
        base.IdFieldIndex = binaryReader.ReadUInt16();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        binaryReader.ReadInt32();
        int num = binaryReader.ReadInt32();
        if (num == 0 || base.RecordsCount == 0)
        {
            return;
        }

        List<SectionHeaderWDC3> list = binaryReader.ReadArray<SectionHeaderWDC3>(num).ToList();
        m_sections = list.OfType<IEncryptableDatabaseSection>().ToList();
        m_meta = binaryReader.ReadArray<FieldMetaData>(base.FieldsCount);
        m_columnMeta = binaryReader.ReadArray<ColumnMetaData>(base.FieldsCount);
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

        int num2 = 0;
        int num3 = 0;
        foreach (SectionHeaderWDC3 item in list)
        {
            binaryReader.BaseStream.Position = item.FileOffset;
            if (!base.Flags.HasFlagExt(DB2Flags.Sparse))
            {
                recordsData = binaryReader.ReadBytes(item.NumRecords * base.RecordSize);
                Array.Resize(ref recordsData, recordsData.Length + 8);
                if (m_stringsTable == null)
                {
                    m_stringsTable = new Dictionary<long, string>(item.StringTableSize / 32);
                }

                long position;
                for (int l = 0; l < item.StringTableSize; l += (int)(binaryReader.BaseStream.Position - position))
                {
                    position = binaryReader.BaseStream.Position;
                    m_stringsTable[l + num2] = binaryReader.ReadCString();
                }

                num2 += item.StringTableSize;
            }
            else
            {
                recordsData = binaryReader.ReadBytes(item.OffsetRecordsEndOffset - item.FileOffset);
                if (binaryReader.BaseStream.Position != item.OffsetRecordsEndOffset)
                {
                    throw new Exception("reader.BaseStream.Position != section.OffsetRecordsEndOffset");
                }
            }

            if (item.TactKeyLookup != 0L && Array.TrueForAll(recordsData, (byte x) => x == 0))
            {
                bool flag = false;
                if (item.IndexDataSize > 0 || item.CopyTableCount > 0)
                {
                    flag = binaryReader.ReadInt32() == 0;
                    binaryReader.BaseStream.Position -= 4L;
                }
                else if (item.OffsetMapIDCount > 0)
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
                    num3 += item.NumRecords;
                    continue;
                }
            }

            m_indexData = binaryReader.ReadArray<int>(item.IndexDataSize / 4);
            if (m_indexData.Length != 0 && m_indexData.All((int x) => x == 0))
            {
                m_indexData = Enumerable.Range(base.MinIndex + num3, item.NumRecords).ToArray();
            }

            if (item.CopyTableCount > 0)
            {
                if (m_copyData == null)
                {
                    m_copyData = new Dictionary<int, int>();
                }

                for (int m = 0; m < item.CopyTableCount; m++)
                {
                    int num4 = binaryReader.ReadInt32();
                    int num5 = binaryReader.ReadInt32();
                    if (num4 != num5)
                    {
                        m_copyData[num4] = num5;
                    }
                }
            }

            if (item.OffsetMapIDCount > 0)
            {
                if (base.TableHash == 145293629)
                {
                    binaryReader.BaseStream.Position += 4 * item.OffsetMapIDCount;
                }

                m_sparseEntries = binaryReader.ReadArray<SparseEntry>(item.OffsetMapIDCount).ToList();
            }

            ReferenceData referenceData = new ReferenceData();
            if (item.ParentLookupDataSize > 0)
            {
                referenceData.NumRecords = binaryReader.ReadInt32();
                referenceData.MinId = binaryReader.ReadInt32();
                referenceData.MaxId = binaryReader.ReadInt32();
                ReferenceEntry[] array = binaryReader.ReadArray<ReferenceEntry>(referenceData.NumRecords);
                for (int n = 0; n < array.Length; n++)
                {
                    referenceData.Entries[array[n].Index] = array[n].Id;
                }
            }

            if (item.OffsetMapIDCount > 0)
            {
                int[] array2 = binaryReader.ReadArray<int>(item.OffsetMapIDCount);
                if (item.IndexDataSize > 0 && m_indexData.Length != array2.Length)
                {
                    throw new Exception("m_indexData.Length != sparseIndexData.Length");
                }

                m_indexData = array2;
            }

            int num6 = 0;
            for (int num7 = 0; num7 < item.NumRecords; num7++)
            {
                BitReader bitReader = new BitReader(recordsData)
                {
                    Position = 0
                };
                if (base.Flags.HasFlagExt(DB2Flags.Sparse))
                {
                    bitReader.Position = num6;
                    num6 += m_sparseEntries[num7].Size * 8;
                }
                else
                {
                    bitReader.Offset = num7 * base.RecordSize;
                }

                referenceData.Entries.TryGetValue(num7, out var value);
                IDBRow value2 = new WDC3Row(this, bitReader, (item.IndexDataSize != 0) ? m_indexData[num7] : (-1), value, num7 + num3);
                _Records.Add(_Records.Count, value2);
            }

            num3 += item.NumRecords;
        }
    }
}