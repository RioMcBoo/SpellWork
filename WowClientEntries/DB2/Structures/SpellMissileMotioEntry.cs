using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellMissileMotionEntry
    {
        [Index(false)]
        public int Id;
        public string Name;
        public string Script;
        public uint Flags;
        public int MissileCount;
    };
}
