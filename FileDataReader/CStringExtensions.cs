using System.Text;

namespace FileDataReader;

internal static class CStringExtensions
{
    public static string ReadCString(this BinaryReader reader)
    {
        return reader.ReadCString(Encoding.UTF8);
    }

    public static string ReadCString(this BinaryReader reader, Encoding encoding)
    {
        List<byte> list = new List<byte>(32);
        byte item;
        while ((item = reader.ReadByte()) != 0)
        {
            list.Add(item);
        }

        return encoding.GetString(list.ToArray());
    }

    public static void WriteCString(this BinaryWriter writer, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    public static byte[] ToByteArray(this string str)
    {
        str = str.Replace(" ", string.Empty);
        byte[] array = new byte[str.Length / 2];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
        }

        return array;
    }
}