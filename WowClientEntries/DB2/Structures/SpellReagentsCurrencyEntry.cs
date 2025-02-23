using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellReagentsCurrencyEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        public ushort CurrencyTypesID;
        public ushort CurrencyCount;
    }
}
