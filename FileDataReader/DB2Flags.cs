namespace FileDataReader;

[Flags]
public enum DB2Flags
{
    None = 0,
    Sparse = 1,
    SecondaryKey = 2,
    Index = 4,
    Unknown1 = 8,
    BitPacked = 0x10
}