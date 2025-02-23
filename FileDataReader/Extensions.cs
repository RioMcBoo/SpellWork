using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileDataReader;

internal static class Extensions
{
    public static Action<T, object> GetSetter<T>(this FieldInfo fieldInfo)
    {
        ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
        MemberExpression left = Expression.Field(parameterExpression, fieldInfo);
        ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
        UnaryExpression right = Expression.Convert(parameterExpression2, fieldInfo.FieldType);
        return Expression.Lambda<Action<T, object>>(Expression.Assign(left, right), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
    }

    public static T GetAttribute<T>(this FieldInfo fieldInfo) where T : Attribute
    {
        return Attribute.GetCustomAttribute(fieldInfo, typeof(T)) as T;
    }

    public static T Read<T>(this BinaryReader reader) where T : struct
    {
        return Unsafe.ReadUnaligned<T>(ref reader.ReadBytes(Unsafe.SizeOf<T>())[0]);
    }

    public static T[] ReadArray<T>(this BinaryReader reader) where T : struct
    {
        int num = (int)reader.ReadInt64();
        byte[] src = reader.ReadBytes(num);
        reader.BaseStream.Position += -num & 7;
        return src.CopyTo<T>();
    }

    public static T[] ReadArray<T>(this BinaryReader reader, int size) where T : struct
    {
        int count = Marshal.SizeOf<T>() * size;
        return reader.ReadBytes(count).CopyTo<T>();
    }

    public unsafe static T[] CopyTo<T>(this byte[] src) where T : struct
    {
        T[] array = new T[src.Length / Unsafe.SizeOf<T>()];
        if (src.Length != 0)
        {
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref array[0]), Unsafe.AsPointer(ref src[0]), (uint)src.Length);
        }

        return array;
    }

    public static bool HasFlagExt(this DB2Flags flag, DB2Flags valueToCheck)
    {
        return (flag & valueToCheck) == valueToCheck;
    }
}