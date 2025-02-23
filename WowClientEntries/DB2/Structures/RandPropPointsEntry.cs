using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class RandPropPointsEntry
    {
        [Index(true)]
        public int ID;
        public int DamageReplaceStat;
        [Cardinality(5)]
        public uint[] Epic;
        [Cardinality(5)]
        public uint[] Superior;
        [Cardinality(5)]
        public uint[] Good;
    }
}
