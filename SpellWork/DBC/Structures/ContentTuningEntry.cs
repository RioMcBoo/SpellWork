using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public sealed class ContentTuningEntry
    {
        [Index(false)]
        public int Id;
        public int OrderIndex;
        public int RedirectContentTuningID;
        public int RedirectFlag;
        public int ParentContentTuningID;
    }
}
