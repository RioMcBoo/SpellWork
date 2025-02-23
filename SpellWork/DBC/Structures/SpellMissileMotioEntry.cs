using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public class SpellMissileMotionEntry
    {
        [Index(true)]
        public int Id;
        public string Name;
        public string Script;
        public uint Flags;
        public uint MissileCount;
    };
}
