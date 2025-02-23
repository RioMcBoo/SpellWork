using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellScalingEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        public int Class;
        public uint MinScalingLevel;
        public uint MaxScalingLevel;
        public short ScalesFromItemLevel;
    }
}
