using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellCooldownsEntry
    {
        [Index(true)]
        public int ID;
        public byte DifficultyID;
        public int CategoryRecoveryTime;
        public int RecoveryTime;
        public int StartRecoveryTime;
        public int SpellID;
    }
}
