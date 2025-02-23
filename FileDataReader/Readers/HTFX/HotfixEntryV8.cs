using System.Runtime.InteropServices;

namespace FileDataReader;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HotfixEntryV8 : IHotfixEntry
{
    private readonly byte op;

    private readonly byte pad1;

    private readonly byte pad2;

    private readonly byte pad3;

    public int PushId { get; }

    public int UniqueId { get; }

    public uint TableHash { get; }

    public int RecordId { get; }

    public int DataSize { get; }

    public bool IsValid => op == 1;
}