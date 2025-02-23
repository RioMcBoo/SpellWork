namespace FileDataReader;

internal abstract class BaseReader
{
    protected FieldMetaData[] m_meta;

    protected int[] m_indexData;

    protected ColumnMetaData[] m_columnMeta;

    protected Value32[][] m_palletData;

    protected Dictionary<int, Value32>[] m_commonData;

    protected Dictionary<long, string> m_stringsTable;

    protected Dictionary<int, int> m_copyData;

    protected byte[] recordsData;

    protected Dictionary<int, IDBRow> _Records = new Dictionary<int, IDBRow>();

    protected List<SparseEntry> m_sparseEntries;

    protected int[] m_foreignKeyData;

    public int RecordsCount { get; protected set; }

    public int FieldsCount { get; protected set; }

    public int RecordSize { get; protected set; }

    public int StringTableSize { get; protected set; }

    public uint TableHash { get; protected set; }

    public uint LayoutHash { get; protected set; }

    public int MinIndex { get; protected set; }

    public int MaxIndex { get; protected set; }

    public int IdFieldIndex { get; protected set; }

    public DB2Flags Flags { get; protected set; }

    public FieldMetaData[] Meta => m_meta;

    public int[] IndexData => m_indexData;

    public ColumnMetaData[] ColumnMeta => m_columnMeta;
    public Value32[][] PalletData => m_palletData;

    public Dictionary<int, Value32>[] CommonData => m_commonData;

    public Dictionary<long, string> StringTable => m_stringsTable;

    public int[] ForeignKeyData => m_foreignKeyData;

    public void Enumerate(Action<IDBRow> action)
    {
        Parallel.ForEach(_Records.Values, action);
        Parallel.ForEach(GetCopyRows(), action);
    }

    private IEnumerable<IDBRow> GetCopyRows()
    {
        if (m_copyData == null || m_copyData.Count == 0)
        {
            yield break;
        }

        _Records = _Records.ToDictionary((KeyValuePair<int, IDBRow> x) => x.Value.Id, (KeyValuePair<int, IDBRow> x) => x.Value);
        foreach (KeyValuePair<int, int> copyDatum in m_copyData)
        {
            IDBRow iDBRow = _Records[copyDatum.Value].Clone();
            iDBRow.Data = iDBRow.Data.Clone();
            iDBRow.Id = copyDatum.Key;
            _Records[iDBRow.Id] = iDBRow;
            yield return iDBRow;
        }

        m_copyData.Clear();
    }
}
