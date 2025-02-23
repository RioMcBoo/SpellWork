using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public sealed class SpellLabelEntry
    {
        [Index(true)]
        public int ID;
        public int LabelID;
        public int SpellID;
    }
}
