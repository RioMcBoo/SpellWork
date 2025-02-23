namespace FileDataReader;

public interface IEncryptableDatabaseSection
{
    ulong TactKeyLookup { get; }
    int NumRecords { get; }
}