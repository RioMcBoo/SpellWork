using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellCategoriesEntry
    {
        [Index(true)]
        public int ID;
        public byte DifficultyID;
        public short Category;
        public sbyte DefenseType;
        public sbyte DispelType;
        public sbyte Mechanic;
        public sbyte PreventionType;
        public short StartRecoveryCategory;
        public short ChargeCategory;
        public int SpellID;
    }
}
