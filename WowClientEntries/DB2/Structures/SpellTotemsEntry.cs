using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellTotemsEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        [Cardinality(2)]
        public ushort[] RequiredTotemCategoryID = new ushort[2];
        [Cardinality(2)]
        public int[] Totem = new int[2];
    }
}
