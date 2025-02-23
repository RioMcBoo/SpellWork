using System.Runtime.CompilerServices;

namespace FileDataReader;

internal class HTFXRow : IDBRow, IHotfixEntry, IEquatable<HTFXRow>
{
    private BitReader m_data;

    private readonly IHotfixEntry m_hotfixEntry;

    private static Dictionary<Type, Func<BitReader, object>> simpleReaders = new Dictionary<Type, Func<BitReader, object>>
    {
        [typeof(ulong)] = (data) => GetFieldValue<ulong>(data),
        [typeof(long)] = (data) => GetFieldValue<long>(data),
        [typeof(float)] = (data) => GetFieldValue<float>(data),
        [typeof(int)] = (data) => GetFieldValue<int>(data),
        [typeof(uint)] = (data) => GetFieldValue<uint>(data),
        [typeof(short)] = (data) => GetFieldValue<short>(data),
        [typeof(ushort)] = (data) => GetFieldValue<ushort>(data),
        [typeof(sbyte)] = (data) => GetFieldValue<sbyte>(data),
        [typeof(byte)] = (data) => GetFieldValue<byte>(data),
        [typeof(string)] = (data) => data.ReadCString()
    };

    private static Dictionary<Type, Func<BitReader, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, int, object>>
    {
        [typeof(ulong[])] = (data, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
        [typeof(long[])] = (data, cardinality) => GetFieldValueArray<long>(data, cardinality),
        [typeof(float[])] = (data, cardinality) => GetFieldValueArray<float>(data, cardinality),
        [typeof(int[])] = (data, cardinality) => GetFieldValueArray<int>(data, cardinality),
        [typeof(uint[])] = (data, cardinality) => GetFieldValueArray<uint>(data, cardinality),
        [typeof(ulong[])] = (data, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
        [typeof(ushort[])] = (data, cardinality) => GetFieldValueArray<ushort>(data, cardinality),
        [typeof(short[])] = (data, cardinality) => GetFieldValueArray<short>(data, cardinality),
        [typeof(byte[])] = (data, cardinality) => GetFieldValueArray<byte>(data, cardinality),
        [typeof(sbyte[])] = (data, cardinality) => GetFieldValueArray<sbyte>(data, cardinality),
        [typeof(string[])] = (data, cardinality) => (from i in Enumerable.Range(0, cardinality)
                                                                   select data.ReadCString()).ToArray()
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

    public int PushId => m_hotfixEntry.PushId;

    public uint TableHash => m_hotfixEntry.TableHash;

    public int RecordId => m_hotfixEntry.RecordId;

    public bool IsValid => m_hotfixEntry.IsValid;

    public int DataSize => m_hotfixEntry.DataSize;

    public HTFXRow(BitReader data, IHotfixEntry hotfixEntry)
    {
        m_data = data;
        m_hotfixEntry = hotfixEntry;
        Id = hotfixEntry.RecordId;
    }

    public void GetFields<T>(FieldCache<T>[] fields, T entry)
    {
        Data.Position = 0;
        foreach (FieldCache<T> fieldCache in fields)
        {
            if (fieldCache.IndexMapField)
            {
                fieldCache.Setter(entry, Convert.ChangeType(Id, fieldCache.FieldType));
                continue;
            }

            object obj = null;
            if (fieldCache.IsArray)
            {
                if (!arrayReaders.TryGetValue(fieldCache.MetaDataFieldType, out var value))
                {
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
                }

                obj = value(m_data, fieldCache.Cardinality);
            }
            else
            {
                if (!simpleReaders.TryGetValue(fieldCache.MetaDataFieldType, out var value2))
                {
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                obj = value2(m_data);
            }

            if (fieldCache.IsNonInlineRelation)
            {
                object arg = Convert.ChangeType(obj, fieldCache.FieldType);
                fieldCache.Setter(entry, arg);
            }
            else
            {
                fieldCache.Setter(entry, obj);
            }
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

    public override int GetHashCode()
    {
        return (((((unchecked(17 * 486187739) + PushId) * 486187739 + TableHash.GetHashCode()) * 486187739 + RecordId) * 486187739 + (IsValid ? 1 : 0)) * 486187739 + DataSize) * 486187739 + m_data.GetHashCode();
    }

    public bool Equals(HTFXRow other)
    {
        if (PushId == other.PushId && TableHash == other.TableHash && RecordId == other.RecordId && IsValid == other.IsValid)
        {
            return DataSize == other.DataSize;
        }

        return false;
    }
}