using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellXSpellVisualEntry
    {
        [Index(false)]
        public int ID;
        public byte DifficultyID;
        public int SpellVisualID;
        public float Probability;
        public byte Flags;
        public int Priority;
        public int SpellIconFileID;
        public int ActiveIconFileID;
        public ushort ViewerUnitConditionID;
        public uint ViewerPlayerConditionID;
        public ushort CasterUnitConditionID;
        public uint CasterPlayerConditionID;
        public int SpellID;
    }
}
