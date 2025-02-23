namespace FileDataReader;

internal abstract class BaseEncryptionSupportingReader : BaseReader, IEncryptionSupportingReader
{
    protected List<IEncryptableDatabaseSection> m_sections;

    protected Dictionary<ulong, int[]> m_encryptedIDs;

    List<IEncryptableDatabaseSection> IEncryptionSupportingReader.GetEncryptedSections()
    {
        return m_sections.Where((IEncryptableDatabaseSection s) => s.TactKeyLookup != 0).ToList();
    }

    Dictionary<ulong, int[]> IEncryptionSupportingReader.GetEncryptedIDs()
    {
        return m_encryptedIDs;
    }
}