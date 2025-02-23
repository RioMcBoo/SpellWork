namespace FileDataReader;

internal class WDB6Row : IDBRow
{
    private BitReader m_data;

    private BaseReader m_reader;

    private readonly int m_dataOffset;

    private readonly int m_dataPosition;

    private readonly int m_recordIndex;

    private readonly FieldMetaData[] m_fieldMeta;

    private readonly Dictionary<int, Value32>[] m_commonData;

    private static Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>>
    {
        [typeof(long)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<long>(id, data, fieldMeta, commonData),
        [typeof(float)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<float>(id, data, fieldMeta, commonData),
        [typeof(int)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<int>(id, data, fieldMeta, commonData),
        [typeof(uint)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<uint>(id, data, fieldMeta, commonData),
        [typeof(short)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<short>(id, data, fieldMeta, commonData),
        [typeof(ushort)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<ushort>(id, data, fieldMeta, commonData),
        [typeof(sbyte)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<sbyte>(id, data, fieldMeta, commonData),
        [typeof(byte)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<byte>(id, data, fieldMeta, commonData),
        [typeof(string)] = (id, data, fieldMeta, commonData, stringTable, header) => !header.Flags.HasFlagExt(DB2Flags.Sparse) ? stringTable[GetFieldValue<int>(id, data, fieldMeta, commonData)] : data.ReadCString()
    };

    private static Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, int, object>>
    {
        [typeof(ulong[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ulong>(id, data, fieldMeta, commonData, cardinality),
        [typeof(long[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<long>(id, data, fieldMeta, commonData, cardinality),
        [typeof(float[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<float>(id, data, fieldMeta, commonData, cardinality),
        [typeof(int[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<int>(id, data, fieldMeta, commonData, cardinality),
        [typeof(uint[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<uint>(id, data, fieldMeta, commonData, cardinality),
        [typeof(ulong[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ulong>(id, data, fieldMeta, commonData, cardinality),
        [typeof(ushort[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ushort>(id, data, fieldMeta, commonData, cardinality),
        [typeof(short[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<short>(id, data, fieldMeta, commonData, cardinality),
        [typeof(byte[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<byte>(id, data, fieldMeta, commonData, cardinality),
        [typeof(sbyte[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<sbyte>(id, data, fieldMeta, commonData, cardinality),
        [typeof(string[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => (from i in GetFieldValueArray<int>(id, data, fieldMeta, commonData, cardinality)
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

    public WDB6Row(BaseReader reader, BitReader data, int id, int recordIndex)
    {
        m_reader = reader;
        m_data = data;
        m_recordIndex = recordIndex;
        m_dataOffset = m_data.Offset;
        m_dataPosition = m_data.Position;
        m_fieldMeta = reader.Meta;
        m_commonData = reader.CommonData;
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
                    BitReader data = m_data;
                    FieldMetaData fieldMeta = m_fieldMeta[i];
                    Dictionary<int, Value32>[] commonData = m_commonData;
                    Id = GetFieldValue<int>(0, data, fieldMeta, commonData != null ? commonData[i] : null);
                }

                fieldCache.Setter(entry, Convert.ChangeType(Id, fieldCache.FieldType));
                continue;
            }

            object obj = null;
            int num2 = i - num;
            if (num2 >= m_reader.Meta.Length)
            {
                fieldCache.Setter(entry, Convert.ChangeType(m_reader.ForeignKeyData[Id - m_reader.MinIndex], fieldCache.FieldType));
                continue;
            }

            if (fieldCache.IsArray)
            {
                if (fieldCache.Cardinality <= 1)
                {
                    SetCardinality(fieldCache, num2);
                }

                if (!arrayReaders.TryGetValue(fieldCache.FieldType, out var value))
                {
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
                }

                Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, int, object> func = value;
                int id = Id;
                BitReader data2 = m_data;
                FieldMetaData arg = m_fieldMeta[num2];
                Dictionary<int, Value32>[] commonData2 = m_commonData;
                obj = func(id, data2, arg, commonData2 != null ? commonData2[num2] : null, m_reader.StringTable, fieldCache.Cardinality);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.FieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object> func2 = value2;
                int id2 = Id;
                BitReader data3 = m_data;
                FieldMetaData arg2 = m_fieldMeta[num2];
                Dictionary<int, Value32>[] commonData3 = m_commonData;
                obj = func2(id2, data3, arg2, commonData3 != null ? commonData3[num2] : null, m_reader.StringTable, m_reader);
            }

            fieldCache.Setter(entry, obj);
        }
    }

    private void SetCardinality<T>(FieldCache<T> info, int fieldIndex)
    {
        int offset = m_fieldMeta[fieldIndex].Offset;
        int num = 32 - m_fieldMeta[fieldIndex].Bits >> 3;
        int num2 = fieldIndex + 1 < m_fieldMeta.Length ? m_fieldMeta[fieldIndex + 1].Offset : m_reader.RecordSize;
        info.Cardinality = (num2 - offset) / num;
    }

    private static T GetFieldValue<T>(int Id, BitReader r, FieldMetaData fieldMeta, Dictionary<int, Value32> commonData) where T : struct
    {
        if (commonData != null && commonData.TryGetValue(Id, out var value))
        {
            return value.GetValue<T>();
        }

        return r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
    }

    private static T[] GetFieldValueArray<T>(int Id, BitReader r, FieldMetaData fieldMeta, Dictionary<int, Value32> commonData, int cardinality) where T : struct
    {
        T[] array = new T[cardinality];
        for (int i = 0; i < array.Length; i++)
        {
            if (commonData != null && commonData.TryGetValue(Id, out var value))
            {
                array[1] = value.GetValue<T>();
            }
            else
            {
                array[i] = r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
            }
        }

        return array;
    }

    public IDBRow Clone()
    {
        return (IDBRow)MemberwiseClone();
    }
}