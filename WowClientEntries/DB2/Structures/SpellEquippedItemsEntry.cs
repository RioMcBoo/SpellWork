using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public class SpellEquippedItemsEntry
    {
        [Index(true)]
        public int ID;
        public int SpellID;
        public sbyte EquippedItemClass;
        public int EquippedItemInvTypes;
        public int EquippedItemSubclass;
    }
}
