using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellMissileEntry
    {
        [Index(false)]
        public int ID;
        public int SpellID;
        public byte Flags;
        public float DefaultPitchMin;
        public float DefaultPitchMax;
        public float DefaultSpeedMin;
        public float DefaultSpeedMax;
        public float RandomizeFacingMin;
        public float RandomizeFacingMax;
        public float RandomizePitchMin;
        public float RandomizePitchMax;
        public float RandomizeSpeedMin;
        public float RandomizeSpeedMax;
        public float Gravity;
        public float MaxDuration;
        public float CollisionRadius;
    }
}
