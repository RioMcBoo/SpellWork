using System.Runtime.CompilerServices;

namespace FileDataReader;

internal class WDC4Row : IDBRow
{
    private BitReader m_data;

    private BaseReader m_reader;

    private readonly int m_dataOffset;

    private readonly int m_dataPosition;

    private readonly int m_recordOffset;

    private readonly int m_recordIndex;

    private readonly FieldMetaData[] m_fieldMeta;

    private readonly ColumnMetaData[] m_columnMeta;

    private readonly Value32[][] m_palletData;

    private readonly Dictionary<int, Value32>[] m_commonData;

    private readonly int m_refID;

    private static Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>>
    {
        [typeof(ulong)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<ulong>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(long)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<long>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(float)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<float>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(int)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(uint)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<uint>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(short)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<short>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ushort)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<ushort>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(sbyte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<sbyte>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(byte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => GetFieldValue<byte>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(string)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, header) => !header.Flags.HasFlagExt(DB2Flags.Sparse) ? getStringTableRecord(stringTable, recordOffset, id, data, fieldMeta, columnMeta, palletData, commonData) : data.ReadCString()
    };

    private static Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>> arrayReaders = new Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>>
    {
        [typeof(ulong[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(long[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<long>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(float[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<float>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(int[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(uint[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<uint>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ulong[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(short[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<short>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ushort[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<ushort>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(byte[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<byte>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(sbyte[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueArray<sbyte>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(string[])] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValueStringArray(data, fieldMeta, columnMeta, recordOffset, stringTable)
    };

    public int Id { get; set; }

    public BitReader Data
    {
        get
        {
            return m_data;
        }
        set
        {
            m_data = value;
        }
    }

    public WDC4Row(BaseReader reader, BitReader data, int id, int refID, int recordIndex)
    {
        m_reader = reader;
        m_data = data;
        m_recordOffset = recordIndex * reader.RecordSize - reader.RecordsCount * reader.RecordSize;
        m_recordIndex = recordIndex;
        m_dataOffset = m_data.Offset;
        m_dataPosition = m_data.Position;
        m_fieldMeta = reader.Meta;
        m_columnMeta = reader.ColumnMeta;
        m_palletData = reader.PalletData;
        m_commonData = reader.CommonData;
        m_refID = refID;
        Id = id;
    }

    private static string getStringTableRecord(Dictionary<long, string> stringTable, int recordOffset, int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData)
    {
        int num = recordOffset + (data.Position >> 3) + GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData);
        if (!stringTable.TryGetValue(num, out var value))
        {
            return "";
        }

        return value;
    }

    public void GetFields<T>(FieldCache<T>[] fields, T entry)
    {
        int num = 0;
        m_data.Position = m_dataPosition;
        m_data.Offset = m_dataOffset;
        for (int i = 0; i < fields.Length; i++)
        {
            FieldCache<T> fieldCache = fields[i];
            if (i == m_reader.IdFieldIndex)
            {
                if (Id != -1)
                {
                    num++;
                }
                else
                {
                    Id = GetFieldValue<int>(0, m_data, m_fieldMeta[i], m_columnMeta[i], m_palletData[i], m_commonData[i]);
                }

                fieldCache.Setter(entry, Convert.ChangeType(Id, fieldCache.FieldType));
                continue;
            }

            object obj = null;
            int num2 = i - num;
            if (num2 >= m_reader.Meta.Length)
            {
                fieldCache.Setter(entry, Convert.ChangeType(m_refID, fieldCache.FieldType));
                continue;
            }

            if (fieldCache.IsArray)
            {
                if (!arrayReaders.TryGetValue(fieldCache.FieldType, out var value))
                {
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
                }

                obj = value(m_data, m_recordOffset, m_fieldMeta[num2], m_columnMeta[num2], m_palletData[num2], m_commonData[num2], m_reader.StringTable);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.FieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                obj = value2(Id, m_data, m_recordOffset, m_fieldMeta[num2], m_columnMeta[num2], m_palletData[num2], m_commonData[num2], m_reader.StringTable, m_reader);
            }

            fieldCache.Setter(entry, obj);
        }
    }

    private static T GetFieldValue<T>(int Id, BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData) where T : struct
    {
        switch (columnMeta.CompressionType)
        {
            case CompressionType.None:
            {
                int num3 = 32 - fieldMeta.Bits;
                if (num3 <= 0)
                {
                    num3 = columnMeta.Immediate.BitWidth;
                }

                return r.ReadValue64(num3).GetValue<T>();
            }
            case CompressionType.SignedImmediate:
                return r.ReadValue64Signed(columnMeta.Immediate.BitWidth).GetValue<T>();
            case CompressionType.Immediate:
                return r.ReadValue64(columnMeta.Immediate.BitWidth).GetValue<T>();
            case CompressionType.Common:
            {
                if (commonData.TryGetValue(Id, out var value))
                {
                    return value.GetValue<T>();
                }

                return columnMeta.Common.DefaultValue.GetValue<T>();
            }
            case CompressionType.Pallet:
            {
                uint num2 = r.ReadUInt32(columnMeta.Pallet.BitWidth);
                return palletData[num2].GetValue<T>();
            }
            case CompressionType.PalletArray:
                if (columnMeta.Pallet.Cardinality == 1)
                {
                    uint num = r.ReadUInt32(columnMeta.Pallet.BitWidth);
                    return palletData[num].GetValue<T>();
                }

                break;
        }

        throw new Exception($"Unexpected compression type {columnMeta.CompressionType}");
    }

    private static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData) where T : struct
    {
        switch (columnMeta.CompressionType)
        {
            case CompressionType.None:
            {
                int num2 = 32 - fieldMeta.Bits;
                if (num2 <= 0)
                {
                    num2 = columnMeta.Immediate.BitWidth;
                }

                T[] array = new T[columnMeta.Size / (Unsafe.SizeOf<T>() * 8)];
                for (int j = 0; j < array.Length; j++)
                {
                    array[j] = r.ReadValue64(num2).GetValue<T>();
                }

                return array;
            }
            case CompressionType.PalletArray:
            {
                int cardinality = columnMeta.Pallet.Cardinality;
                uint num = r.ReadUInt32(columnMeta.Pallet.BitWidth);
                T[] array = new T[cardinality];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = palletData[i + cardinality * (int)num].GetValue<T>();
                }

                return array;
            }
            default:
                throw new Exception($"Unexpected compression type {columnMeta.CompressionType}");
        }
    }

    private static string[] GetFieldValueStringArray(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, int recordOffset, Dictionary<long, string> stringTable)
    {
        if (columnMeta.CompressionType == CompressionType.None)
        {
            int num = 32 - fieldMeta.Bits;
            if (num <= 0)
            {
                num = columnMeta.Immediate.BitWidth;
            }

            string[] array = new string[columnMeta.Size / 32];
            for (int i = 0; i < array.Length; i++)
            {
                int num2 = (r.Position >> 3) + recordOffset + r.ReadValue64(num).GetValue<int>();
                if (stringTable.TryGetValue(num2, out var value))
                {
                    array[i] = value;
                }
                else
                {
                    array[i] = "";
                }
            }

            return array;
        }

        throw new Exception($"Unexpected compression type {columnMeta.CompressionType}");
    }

    public IDBRow Clone()
    {
        return (IDBRow)MemberwiseClone();
    }
}