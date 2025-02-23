using System.Text;
using System.Linq.Expressions;

namespace FileDataReader;

internal class HTFXReader : BaseReader
{
    public readonly int Version;

    public readonly int BuildId;

    private const int HeaderSize = 12;

    private const int ExtendedHeaderSize = 44;

    private const uint HTFXFmtSig = 1213482584u;

    public HTFXReader(string dbcFile)
        : this(new FileStream(dbcFile, FileMode.Open))
    {
    }

    public HTFXReader(Stream stream)
    {
        using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
        if (binaryReader.BaseStream.Length < 12)
        {
            throw new InvalidDataException("Hotfix file is corrupted!");
        }

        if (binaryReader.ReadUInt32() != 1213482584)
        {
            throw new InvalidDataException("Hotfix file is corrupted!");
        }

        Version = binaryReader.ReadInt32();
        BuildId = binaryReader.ReadInt32();
        if (Version >= 5)
        {
            if (binaryReader.BaseStream.Length < 44)
            {
                throw new InvalidDataException("Hotfix file is corrupted!");
            }

            binaryReader.BaseStream.Position += 32L;
        }

        long length = binaryReader.BaseStream.Length;
        if (Version == 8 && length > binaryReader.BaseStream.Position + 24)
        {
            long position = binaryReader.BaseStream.Position;
            if (binaryReader.ReadUInt32() != 1213482584)
            {
                throw new Exception("Invalid hotfix entry magic!");
            }

            binaryReader.BaseStream.Position += 16L;
            int num = binaryReader.ReadInt32();
            if (binaryReader.BaseStream.Length > binaryReader.BaseStream.Position + 4 + num)
            {
                binaryReader.BaseStream.Position += 4 + num;
            }

            if (binaryReader.BaseStream.Length > binaryReader.BaseStream.Position + 4 && binaryReader.ReadUInt32() != 1213482584)
            {
                Version = 7;
            }

            binaryReader.BaseStream.Position = position;
        }

        Func<BinaryReader, IHotfixEntry> readerFunc = GetReaderFunc();
        while (binaryReader.BaseStream.Position < length)
        {
            if (binaryReader.ReadUInt32() != 1213482584)
            {
                throw new InvalidDataException("Hotfix file is corrupted!");
            }

            IHotfixEntry hotfixEntry = readerFunc(binaryReader);
            HTFXRow value = new HTFXRow(new BitReader(binaryReader.ReadBytes(hotfixEntry.DataSize)), hotfixEntry);
            _Records.Add(_Records.Count, value);
        }
    }

    public IEnumerable<HTFXRow> GetRecords(uint tablehash)
    {
        foreach (HTFXRow value in _Records.Values)
        {
            if (value.TableHash == tablehash)
            {
                yield return value;
            }
        }
    }

    public void Combine(HTFXReader reader)
    {
        HashSet<HTFXRow> hashSet = new HashSet<HTFXRow>(_Records.Values.Cast<HTFXRow>());
        foreach (HTFXRow value in reader._Records.Values)
        {
            if (!hashSet.Contains(value))
            {
                _Records.Add(_Records.Count, value);
                hashSet.Add(value);
            }
        }
    }

    private Func<BinaryReader, IHotfixEntry> GetReaderFunc()
    {
        Type typeFromHandle;
        if (Version == 1)
        {
            typeFromHandle = typeof(HotfixEntryV1);
        }
        else if (Version >= 2 && Version <= 6)
        {
            typeFromHandle = typeof(HotfixEntryV2);
        }
        else if (Version == 7)
        {
            typeFromHandle = typeof(HotfixEntryV7);
        }
        else if (Version == 8)
        {
            typeFromHandle = typeof(HotfixEntryV8);
        }
        else
        {
            if (Version != 9)
            {
                throw new NotSupportedException($"Hotfix version {Version} is not supported");
            }

            typeFromHandle = typeof(HotfixEntryV9);
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeof(BinaryReader), "reader");
        return Expression.Lambda<Func<BinaryReader, IHotfixEntry>>(Expression.Convert(Expression.Call(typeof(Extensions).GetMethod("Read").MakeGenericMethod(typeFromHandle), parameterExpression), typeof(IHotfixEntry)), new ParameterExpression[1] { parameterExpression }).Compile();
    }
}