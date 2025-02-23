using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellAuraOptionsEntry
    {
        [Index(true)]
        public int ID;
        public byte DifficultyID;
        public int CumulativeAura;
        public int ProcCategoryRecovery;
        public byte ProcChance;
        public int ProcCharges;
        public ushort SpellProcsPerMinuteID;
        [Cardinality(2)]
        public int[] ProcTypeMask = new int[2];
        public int SpellID;
    }
}
