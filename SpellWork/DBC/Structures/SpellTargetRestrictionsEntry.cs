using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public sealed class SpellTargetRestrictionsEntry
    {
        [Index(true)]
        public int ID;
        public byte DifficultyID;
        public float ConeDegrees;
        public byte MaxTargets;
        public int MaxTargetLevel;
        public short TargetCreatureType;
        public int Targets;
        public float Width;
        public int SpellID;
    }
}
