using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class SpellDurationEntry
    {
        [Index(true)]
        public int ID;
        public int Duration;
        public int DurationPerLevel;
        public int MaxDuration;
    }
}
