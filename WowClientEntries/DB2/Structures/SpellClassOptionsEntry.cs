using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellClassOptionsEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        public int ModalNextSpell;
        public byte SpellClassSet;
        [Cardinality(4)]
        public int[] SpellClassMask = new int[4];
    }
}
