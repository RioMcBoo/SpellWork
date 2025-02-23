using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellRadiusEntry
    {
        [Index(true)]
        public int ID;
        public float Radius;
        public float RadiusPerLevel;
        public float RadiusMin;
        public float MaxRadius;
    }
}
