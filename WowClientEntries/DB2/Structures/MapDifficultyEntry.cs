using FileDataReader;
using System;

namespace WOWClient.DB2.Structures
{
    public class MapDifficultyEntry : IComparable
    {
        [Index(true)]
        public int ID;
        public string Message;
        public int ItemContextPickerID;
        public int ContentTuningID;
        public byte DifficultyID;
        public byte LockID;
        public byte ResetInterval;
        public byte MaxPlayers;
        public byte ItemContext;
        public byte Flags;
        public int MapID;

        public int CompareTo(object obj)
        {
            return obj is MapDifficultyEntry m ? ID.CompareTo(m.ID) : 1;
        }
    }
}
