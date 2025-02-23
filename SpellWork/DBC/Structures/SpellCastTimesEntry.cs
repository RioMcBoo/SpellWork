using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public class SpellCastTimesEntry
    {
        [Index(true)]
        public int ID;
        public int Base;
        public short PerLevel;
        public int Minimum;
    }
}
