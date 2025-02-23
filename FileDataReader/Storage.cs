namespace FileDataReader;

public class Storage<T> : SortedDictionary<int, T> where T : class, new()
{
    public Storage(string fileName)
        : this((Stream)File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public Storage(Stream stream)
        : this(new DBReader(stream))
    {
    }

    public Storage(DBReader dbReader)
    {
        dbReader.PopulateRecords(this);
    }
}