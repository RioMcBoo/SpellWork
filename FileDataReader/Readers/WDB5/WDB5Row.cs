namespace FileDataReader;

internal class WDB5Row : IDBRow
{
    private BitReader m_data;

    private BaseReader m_reader;

    private readonly int m_dataOffset;

    private readonly int m_dataPosition;

    private readonly int m_recordIndex;

    private readonly FieldMetaData[] m_fieldMeta;

    private static Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, BaseReader, object>>
    {
        [typeof(long)] = (data, fieldMeta, stringTable, header) => GetFieldValue<long>(data, fieldMeta),
        [typeof(float)] = (data, fieldMeta, stringTable, header) => GetFieldValue<float>(data, fieldMeta),
        [typeof(int)] = (data, fieldMeta, stringTable, header) => GetFieldValue<int>(data, fieldMeta),
        [typeof(uint)] = (data, fieldMeta, stringTable, header) => GetFieldValue<uint>(data, fieldMeta),
        [typeof(short)] = (data, fieldMeta, stringTable, header) => GetFieldValue<short>(data, fieldMeta),
        [typeof(ushort)] = (data, fieldMeta, stringTable, header) => GetFieldValue<ushort>(data, fieldMeta),
        [typeof(sbyte)] = (data, fieldMeta, stringTable, header) => GetFieldValue<sbyte>(data, fieldMeta),
        [typeof(byte)] = (data, fieldMeta, stringTable, header) => GetFieldValue<byte>(data, fieldMeta),
        [typeof(string)] = (data, fieldMeta, stringTable, header) => !header.Flags.HasFlagExt(DB2Flags.Sparse) ? stringTable[GetFieldValue<int>(data, fieldMeta)] : data.ReadCString()
    };

    private static Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, int, object>>
    {
        [typeof(ulong[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ulong>(data, fieldMeta, cardinality),
        [typeof(long[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<long>(data, fieldMeta, cardinality),
        [typeof(float[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<float>(data, fieldMeta, cardinality),
        [typeof(int[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<int>(data, fieldMeta, cardinality),
        [typeof(uint[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<uint>(data, fieldMeta, cardinality),
        [typeof(ulong[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ulong>(data, fieldMeta, cardinality),
        [typeof(ushort[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ushort>(data, fieldMeta, cardinality),
        [typeof(short[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<short>(data, fieldMeta, cardinality),
        [typeof(byte[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<byte>(data, fieldMeta, cardinality),
        [typeof(sbyte[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<sbyte>(data, fieldMeta, cardinality),
        [typeof(string[])] = (data, fieldMeta, stringTable, cardinality) => (from i in GetFieldValueArray<int>(data, fieldMeta, cardinality)
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

    public WDB5Row(BaseReader reader, BitReader data, int id, int recordIndex)
    {
        m_reader = reader;
        m_data = data;
        m_recordIndex = recordIndex;
        Id = id;
        m_dataOffset = m_data.Offset;
        m_dataPosition = m_data.Position;
        m_fieldMeta = reader.Meta;
    }

    public void GetFields<T>(FieldCache<T>[] fields, T entry)
    {
        int num = 0;
        m_data.Position = m_dataPosition;
        m_data.Offset = m_dataOffset;
        for (int i = 0; i < fields.Length; i++)
        {
            FieldCache<T> fieldCache = fields[i];
            if (fieldCache.IndexMapField)
            {
                if (Id != -1)
                {
                    num++;
                }
                else
                {
                    Id = GetFieldValue<int>(m_data, m_fieldMeta[i]);
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

                obj = value(m_data, m_fieldMeta[num2], m_reader.StringTable, fieldCache.Cardinality);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.FieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                obj = value2(m_data, m_fieldMeta[num2], m_reader.StringTable, m_reader);
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

    private static T GetFieldValue<T>(BitReader r, FieldMetaData fieldMeta) where T : struct
    {
        return r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
    }

    private static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, int cardinality) where T : struct
    {
        T[] array = new T[cardinality];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
        }

        return array;
    }

    public IDBRow Clone()
    {
        return (IDBRow)MemberwiseClone();
    }
}