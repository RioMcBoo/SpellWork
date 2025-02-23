using System.Runtime.CompilerServices;

namespace FileDataReader;

internal class WDC1Row : IDBRow
{
    private BitReader m_data;

    private BaseReader m_reader;

    private readonly int m_dataOffset;

    private readonly int m_dataPosition;

    private readonly int m_recordIndex;

    private readonly FieldMetaData[] m_fieldMeta;

    private readonly ColumnMetaData[] m_columnMeta;

    private readonly Value32[][] m_palletData;

    private readonly Dictionary<int, Value32>[] m_commonData;

    private readonly int m_refID;

    private static Dictionary<Type, Func<int, BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>>
    {
        [typeof(long)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<long>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(float)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<float>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(int)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(uint)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<uint>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(short)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<short>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ushort)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<ushort>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(sbyte)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<sbyte>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(byte)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => GetFieldValue<byte>(id, data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(string)] = (int id, BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable, BaseReader header) => (!header.Flags.HasFlagExt(DB2Flags.Sparse)) ? stringTable[GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData)] : data.ReadCString()
    };

    private static Dictionary<Type, Func<BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>> arrayReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>>
    {
        [typeof(ulong[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(long[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<long>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(float[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<float>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(int[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(uint[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<uint>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ulong[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(ushort[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<ushort>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(short[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<short>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(byte[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<byte>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(sbyte[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => GetFieldValueArray<sbyte>(data, fieldMeta, columnMeta, palletData, commonData),
        [typeof(string[])] = (BitReader data, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringTable) => (from i in GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData)
                                                                                                                                                                                                       select stringTable[i]).ToArray()
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

    public WDC1Row(BaseReader reader, BitReader data, int id, int refID, int recordIndex)
    {
        m_reader = reader;
        m_data = data;
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

                obj = value(m_data, m_fieldMeta[num2], m_columnMeta[num2], m_palletData[num2], m_commonData[num2], m_reader.StringTable);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.FieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                obj = value2(Id, m_data, m_fieldMeta[num2], m_columnMeta[num2], m_palletData[num2], m_commonData[num2], m_reader.StringTable, m_reader);
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
                int num2 = 32 - fieldMeta.Bits;
                if (num2 <= 0)
                {
                    num2 = columnMeta.Immediate.BitWidth;
                }

                return r.ReadValue64(num2).GetValue<T>();
            }
            case CompressionType.Immediate:
                if ((columnMeta.Immediate.Flags & 1) == 1)
                {
                    return r.ReadValue64Signed(columnMeta.Immediate.BitWidth).GetValue<T>();
                }

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
                uint num3 = r.ReadUInt32(columnMeta.Pallet.BitWidth);
                return palletData[num3].GetValue<T>();
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

    public IDBRow Clone()
    {
        return (IDBRow)MemberwiseClone();
    }
}