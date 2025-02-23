using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellMiscEntry
    {
        [Index(true)]
        public int ID;
        [Cardinality(15)]
        public int[] Attributes = new int[15];
        public byte DifficultyID;
        public ushort CastingTimeIndex;
        public ushort DurationIndex;
        public ushort RangeIndex;
        public byte SchoolMask;
        public float Speed;
        public float LaunchDelay;
        public float MinDuration;
        public int SpellIconFileDataID;
        public int ActiveIconFileDataID;
        public int ContentTuningID;
        public int ShowFutureSpellPlayerConditionID;
        public int SpellID;
    }
}
