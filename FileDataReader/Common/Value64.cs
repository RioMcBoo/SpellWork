using System.Runtime.CompilerServices;

namespace FileDataReader;

internal struct Value64
{
    private unsafe fixed byte Value[8];

    public unsafe T GetValue<T>() where T : struct
    {
        fixed (byte* source = Value)
        {
            return Unsafe.ReadUnaligned<T>(source);
        }
    }
}