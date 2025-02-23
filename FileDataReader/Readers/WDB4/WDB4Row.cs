using System.Runtime.CompilerServices;

namespace FileDataReader;

internal class WDB4Row : IDBRow
{
    private BitReader m_data;

    private BaseReader m_reader;

    private readonly int m_dataOffset;

    private readonly int m_dataPosition;

    private readonly int m_recordIndex;

    private static Dictionary<Type, Func<BitReader, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<BitReader, Dictionary<long, string>, BaseReader, object>>
    {
        [typeof(long)] = (data, stringTable, header) => GetFieldValue<long>(data),
        [typeof(float)] = (data, stringTable, header) => GetFieldValue<float>(data),
        [typeof(int)] = (data, stringTable, header) => GetFieldValue<int>(data),
        [typeof(uint)] = (data, stringTable, header) => GetFieldValue<uint>(data),
        [typeof(short)] = (data, stringTable, header) => GetFieldValue<short>(data),
        [typeof(ushort)] = (data, stringTable, header) => GetFieldValue<ushort>(data),
        [typeof(sbyte)] = (data, stringTable, header) => GetFieldValue<sbyte>(data),
        [typeof(byte)] = (data, stringTable, header) => GetFieldValue<byte>(data),
        [typeof(string)] = (data, stringTable, header) => !header.Flags.HasFlagExt(DB2Flags.Sparse) ? stringTable[GetFieldValue<int>(data)] : data.ReadCString()
    };

    private static Dictionary<Type, Func<BitReader, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, Dictionary<long, string>, int, object>>
    {
        [typeof(ulong[])] = (data, stringTable, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
        [typeof(long[])] = (data, stringTable, cardinality) => GetFieldValueArray<long>(data, cardinality),
        [typeof(float[])] = (data, stringTable, cardinality) => GetFieldValueArray<float>(data, cardinality),
        [typeof(int[])] = (data, stringTable, cardinality) => GetFieldValueArray<int>(data, cardinality),
        [typeof(uint[])] = (data, stringTable, cardinality) => GetFieldValueArray<uint>(data, cardinality),
        [typeof(ulong[])] = (data, stringTable, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
        [typeof(ushort[])] = (data, stringTable, cardinality) => GetFieldValueArray<ushort>(data, cardinality),
        [typeof(short[])] = (data, stringTable, cardinality) => GetFieldValueArray<short>(data, cardinality),
        [typeof(byte[])] = (data, stringTable, cardinality) => GetFieldValueArray<byte>(data, cardinality),
        [typeof(sbyte[])] = (data, stringTable, cardinality) => GetFieldValueArray<sbyte>(data, cardinality),
        [typeof(string[])] = (data, stringTable, cardinality) => (from i in GetFieldValueArray<int>(data, cardinality)
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

    public WDB4Row(BaseReader reader, BitReader data, int id, int recordIndex)
    {
        m_reader = reader;
        m_data = data;
        m_recordIndex = recordIndex;
        Id = id;
        m_dataOffset = m_data.Offset;
        m_dataPosition = m_data.Position;
    }

    public void GetFields<T>(FieldCache<T>[] fields, T entry)
    {
        int num = 0;
        m_data.Position = m_dataPosition;
        m_data.Offset = m_dataOffset;
        for (int i = 0; i < fields.Length; i++)
        {
            FieldCache<T> fieldCache = fields[i];
            if (fields[i].IndexMapField)
            {
                if (Id != -1)
                {
                    num++;
                }
                else
                {
                    Id = GetFieldValue<int>(m_data);
                }

                fieldCache.Setter(entry, Convert.ChangeType(Id, fieldCache.FieldType));
                continue;
            }

            object obj = null;
            if (i - num >= m_reader.FieldsCount)
            {
                fieldCache.Setter(entry, Convert.ChangeType(m_reader.ForeignKeyData[Id - m_reader.MinIndex], fieldCache.FieldType));
                continue;
            }

            if (fieldCache.IsArray)
            {
                if (!arrayReaders.TryGetValue(fieldCache.FieldType, out var value))
                {
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
                }

                obj = value(m_data, m_reader.StringTable, fieldCache.Cardinality);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.FieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                obj = value2(m_data, m_reader.StringTable, m_reader);
            }

            fieldCache.Setter(entry, obj);
        }
    }

    private static T GetFieldValue<T>(BitReader r) where T : struct
    {
        return r.ReadValue64(Unsafe.SizeOf<T>() * 8).GetValue<T>();
    }

    private static T[] GetFieldValueArray<T>(BitReader r, int cardinality) where T : struct
    {
        T[] array = new T[cardinality];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = r.ReadValue64(Unsafe.SizeOf<T>() * 8).GetValue<T>();
        }

        return array;
    }

    public IDBRow Clone()
    {
        return (IDBRow)MemberwiseClone();
    }
}