using FileDataReader;

namespace WOWClient.DB2.Structures
{
    public sealed class AreaGroupMemberEntry
    {
        [Index(true)]
        public int ID;
        public ushort AreaID;
        public int AreaGroupID;
    }
}
