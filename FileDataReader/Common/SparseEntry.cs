using System.Runtime.InteropServices;

namespace FileDataReader;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct SparseEntry
{
    public uint Offset;
    public ushort Size;
}