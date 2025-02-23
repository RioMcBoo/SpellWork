using System.Runtime.InteropServices;

namespace FileDataReader;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct SectionHeaderWDC3 : IEncryptableDatabaseSection
{
    public ulong TactKeyLookup;

    public int FileOffset;

    public int NumRecords;

    public int StringTableSize;

    public int OffsetRecordsEndOffset;

    public int IndexDataSize;

    public int ParentLookupDataSize;

    public int OffsetMapIDCount;

    public int CopyTableCount;

    ulong IEncryptableDatabaseSection.TactKeyLookup => TactKeyLookup;

    int IEncryptableDatabaseSection.NumRecords => NumRecords;
}