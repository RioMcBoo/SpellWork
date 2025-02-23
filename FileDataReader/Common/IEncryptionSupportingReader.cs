namespace FileDataReader;

public interface IEncryptionSupportingReader
{
    List<IEncryptableDatabaseSection> GetEncryptedSections();
    Dictionary<ulong, int[]> GetEncryptedIDs();
}