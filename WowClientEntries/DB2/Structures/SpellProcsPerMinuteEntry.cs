using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellProcsPerMinuteEntry
    {
        [Index(true)]
        public int ID;
        public float BaseProcRate;
        public byte Flags;
    }
}
