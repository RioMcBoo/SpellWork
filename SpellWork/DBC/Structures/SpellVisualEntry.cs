using DBFileReaderLib.Attributes;

namespace SpellWork.DBC.Structures
{
    public class SpellVisualEntry
    {
        [Index(true)]
        public int Id;
        public float MissileCastOffsetX;
        public float MissileCastOffsetY;
        public float MissileCastOffsetZ;
        public float MissileImpactOffsetX;
        public float MissileImpactOffsetY;
        public float MissileImpactOffsetZ;
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
