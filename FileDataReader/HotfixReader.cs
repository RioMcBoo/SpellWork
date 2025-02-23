namespace FileDataReader;

public class HotfixReader
{
    public delegate RowOp RowProcessor(IHotfixEntry row, bool shouldDelete);

    private readonly HTFXReader _reader;

    public int Version => _reader.Version;

    public int BuildId => _reader.BuildId;

    public HotfixReader(string fileName)
        : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public HotfixReader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream);
        string text = new string(binaryReader.ReadChars(4));
        stream.Position = 0L;
        if (text == "XFTH")
        {
            _reader = new HTFXReader(stream);
            return;
        }

        throw new Exception("Hotfix type " + text + " is not supported!");
    }

    public void ApplyHotfixes<T>(IDictionary<int, T> storage, DBReader dbReader) where T : class, new()
    {
        ReadHotfixes(storage, dbReader);
    }

    public void ApplyHotfixes<T>(IDictionary<int, T> storage, DBReader dbReader, RowProcessor processor) where T : class, new()
    {
        ReadHotfixes(storage, dbReader, processor);
    }

    public void CombineCaches(params string[] files)
    {
        foreach (string file in files)
        {
            CombineCache(file);
        }
    }

    public void CombineCache(string file)
    {
        if (File.Exists(file))
        {
            HTFXReader hTFXReader = new HTFXReader(file);
            if (hTFXReader.BuildId == BuildId)
            {
                _reader.Combine(hTFXReader);
            }
        }
    }

    protected virtual void ReadHotfixes<T>(IDictionary<int, T> storage, DBReader dbReader, RowProcessor processor = null) where T : class, new()
    {
        FieldCache<T>[] array = (from x in typeof(T).GetFields()
                                 select new FieldCache<T>(x)).ToArray();
        if (processor == null)
        {
            processor = DefaultProcessor;
        }

        if (dbReader.Flags.HasFlagExt(DB2Flags.Index))
        {
            array[dbReader.IdFieldIndex].IndexMapField = true;
        }

        IOrderedEnumerable<HTFXRow> orderedEnumerable = from x in _reader.GetRecords(dbReader.TableHash)
                                                        orderby x.PushId
                                                        select x;
        bool shouldDelete = (dbReader.TableHash != 3744420815u && dbReader.TableHash != 35137211) || !orderedEnumerable.Any((HTFXRow r) => r.IsValid && r.PushId == -1 && r.DataSize > 0);
        foreach (HTFXRow item in orderedEnumerable)
        {
            switch (processor(item, shouldDelete))
            {
                case RowOp.Add:
                {
                    T val = new T();
                    item.GetFields(array, val);
                    storage[item.RecordId] = val;
                    break;
                }
                case RowOp.Delete:
                    storage.Remove(item.RecordId);
                    break;
            }
        }
    }

    public static RowOp DefaultProcessor(IHotfixEntry row, bool shouldDelete)
    {
        if (row.IsValid & (row.DataSize > 0))
        {
            return RowOp.Add;
        }

        if (shouldDelete)
        {
            return RowOp.Delete;
        }

        return RowOp.Ignore;
    }
}