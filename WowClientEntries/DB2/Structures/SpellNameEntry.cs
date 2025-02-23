using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellNameEntry
    {
        [Index(true)]
        public int ID;
        public string Name;
    }
}
