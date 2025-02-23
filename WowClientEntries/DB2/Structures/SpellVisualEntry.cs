using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellVisualEntry
    {
        [Index(true)]
        public int Id;
        public float[] MissileCastOffset = new float[3];
        public float[] MissileImpactOffset = new float[3];
        public int AnimEventSoundID;
        public int Flags;
        public int MissileAttachment;
        public int MissileDestinationAttachment;
        public int MissileCastPositionerID;
        public int MissileImpactPositionerID;
        public int MissileTargetingKit;
        public int HostileSpellVisualID;
        public int CasterSpellVisualID;
        public int SpellVisualMissileSetID;
        public int DamageNumberDelay;
        public int LowViolenceSpellVisualID;
        public int RaidSpellVisualMissileSetID;
        public int ReducedUnexpectedCameraMovementSpellVisualID;
        public int AreaModel;
        public int HasMissile;
    };
}
