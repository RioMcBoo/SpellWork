using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellLabelEntry
    {
        [Index(true)]
        public int ID;
        public int LabelID;
        public int SpellID;
    }
}
