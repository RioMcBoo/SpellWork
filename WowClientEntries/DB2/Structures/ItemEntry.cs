using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class ItemEntry
    {
        [Index(true)]
        public int ID;
        public byte ClassID;
        public byte SubclassID;
        public byte Material;
        public sbyte InventoryType;
        public int RequiredLevel;
        public byte SheatheType;
        public ushort RandomSelect;
        public ushort ItemRandomSuffixGroupID;
        public sbyte SoundOverrideSubclassID;
        public ushort ScalingStatDistributionID;
        public int IconFileDataID;
        public byte ItemGroupSoundsID;
        public int ContentTuningID;
        public uint MaxDurability;
        public byte AmmunitionType;
        public int ScalingStatValue;
        [Cardinality(5)]
        public byte[] DamageType = new byte[5];
        [Cardinality(7)]
        public short[] Resistances = new short[7];
        [Cardinality(5)]
        public ushort[] MinDamage = new ushort[5];
        [Cardinality(5)]
        public ushort[] MaxDamage = new ushort[5];
    }
}
