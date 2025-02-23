using System.Runtime.InteropServices;

namespace FileDataReader;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HotfixEntryV2 : IHotfixEntry
{
    private readonly byte pad1;

    private readonly byte pad2;

    private readonly byte pad3;

    public uint Version { get; }

    public int PushId { get; }

    public int DataSize { get; }

    public uint TableHash { get; }

    public int RecordId { get; }

    public bool IsValid { get; }
}