using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellCastingRequirementsEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        public byte FacingCasterFlags;
        public ushort MinFactionID;
        public int MinReputation;
        public ushort RequiredAreasID;
        public byte RequiredAuraVision;
        public ushort RequiresSpellFocus;
    }
}
