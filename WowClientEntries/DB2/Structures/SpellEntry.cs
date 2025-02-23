using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellEntry
    {
        [Index(true)]
        public int ID;
        public string NameSubtext;
        public string Description;
        public string AuraDescription;
    }
}
