using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellDescriptionVariablesEntry
    {
        [Index(true)]
        public uint ID;
        public string Variables;
    }
}
