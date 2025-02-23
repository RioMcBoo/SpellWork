using System.Runtime.CompilerServices;

namespace FileDataReader;

internal struct Value32
{
    private unsafe fixed byte Value[4];

    public unsafe T GetValue<T>() where T : struct
    {
        fixed (byte* source = Value)
        {
            return Unsafe.ReadUnaligned<T>(source);
        }
    }
}