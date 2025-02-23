using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellXDescriptionVariablesEntry
    {
        [Index(true)]
        public uint ID;
        public int SpellID;
        public int SpellDescriptionVariablesID;
    }
}
