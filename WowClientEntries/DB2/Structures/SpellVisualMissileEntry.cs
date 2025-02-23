using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellVisualMissileEntry
    {
        public float[] CastOffset = new float[3];
        public float[] ImpactOffset = new float[3];
        [Index(false)]
        public int Id;
        public int SpellVisualEffectNameID;
        public int SoundEntriesID;
        public int Attachment;
        public int DestinationAttachment;
        public int CastPositionerID;
        public int ImpactPositionerID;
        public int FollowGroundHeight;
        public int FollowGroundDropSpeed;
        public int FollowGroundApproach;
        public int Flags;
        public int SpellMissileMotionID;
        public int AnimKitID;
        public int SpellVisualMissileSetID;
    };
}
