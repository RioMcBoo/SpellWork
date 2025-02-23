using System.Runtime.InteropServices;

namespace FileDataReader;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct SectionHeader : IEncryptableDatabaseSection
{
    public ulong TactKeyLookup;

    public int FileOffset;

    public int NumRecords;

    public int StringTableSize;

    public int CopyTableSize;

    public int SparseTableOffset;

    public int IndexDataSize;

    public int ParentLookupDataSize;

    ulong IEncryptableDatabaseSection.TactKeyLookup => TactKeyLookup;

    int IEncryptableDatabaseSection.NumRecords => NumRecords;
}