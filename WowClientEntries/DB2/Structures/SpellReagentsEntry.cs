using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellReagentsEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        [Cardinality(8)]
        public int[] Reagent = new int[8];
        [Cardinality(8)]
        public short[] ReagentCount = new short[8];
    }
}
