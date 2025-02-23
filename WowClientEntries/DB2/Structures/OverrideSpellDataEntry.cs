using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class OverrideSpellDataEntry
    {
        [Index(true)]
        public int ID;
        [Cardinality(10)]
        public int[] Spells = new int[10];
        public int PlayerActionbarFileDataID;
        public byte Flags;
    }
}
