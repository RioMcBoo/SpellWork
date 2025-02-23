using System;

#pragma warning disable CA1712 // Do not prefix enum values with type name

namespace SpellWork.Spell
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    ///
    /// </summary>
    

// ReSharper restore InconsistentNaming

    public class SpellEnums
    {
        #region ProcFlagDesc

        public static readonly string[] ProcFlagDesc =
        {
            //00 0x00000001 000000000000000000000001 -
            "00 Heartbeat",
            //01 0x00000002 000000000000000000000010 -
            "01 Kill that yields experience or honor",

            //02 0x00000004 000000000000000000000100 -
            "02 Successful melee attack",
            //03 0x00000008 000000000000000000001000 -
            "03 Taken damage from melee strike hit",

            //04 0x00000010 000000000000000000010000 -
            "04 Successful attack by Spell that use melee weapon",
            //05 0x00000020 000000000000000000100000 -
            "05 Taken damage by Spell that use melee weapon",

            //06 0x00000040 000000000000000001000000 -
            "06 Successful Ranged attack(and wand spell cast)",
            //07 0x00000080 000000000000000010000000 -
            "07 Taken damage from ranged attack",

            //08 0x00000100 000000000000000100000000 -
            "08 Successful Ranged attack by Spell that use ranged weapon",
            //09 0x00000200 000000000000001000000000 -
            "09 Taken damage by Spell that use ranged weapon",

            //10 0x00000400 000000000000010000000000 -
            "10 Successful positive spell hit",
            //11 0x00000800 000000000000100000000000 -
            "11 Taken positive spell hit",

            //12 0x00001000 000000000001000000000000 -
            "12 Successful negative spell cast",
            //13 0x00002000 000000000010000000000000 -
            "13 Taken negative spell hit",

            //14 0x00004000 000000000100000000000000 -
            "14 Successful cast positive magic spell",
            //15 0x00008000 000000001000000000000000 -
            "15 Taken positive magic spell hit",

            //16 0x00010000 000000010000000000000000 -
            "16 Successful damage from harmful magic spell cast",
            //17 0x00020000 000000100000000000000000 -
            "17 Taken magic spell damage",

            //18 0x00040000 000001000000000000000000 -
            "18 Deal periodic damage",
            //19 0x00080000 000010000000000000000000 -
            "19 Taken periodic damage",

            //20 0x00100000 000100000000000000000000 -
            "20 Taken any damage",
            //21 0x00200000 001000000000000000000000 -
            "21 Deal helpful periodic on trap activation",

            //22 0x00800000 010000000000000000000000 -
            "22 Successful main-hand melee attacks",
            //23 0x00800000 100000000000000000000000 -
            "23 Successful off-hand melee attacks",

            //24 0x01000000
            "24 On death (Died in any way)",
            //25 0x02000000
            "25 Jumped",
            "26 Proc Clone Spell",
            //27 0x08000000
            "27 Entered combat",
            //28 0x10000000
            "28 Encounter started",
            "29 On end of spell cast",
            "30 Looted something",
            "31 Taken helpful periodic",
            "32 Target Died",
            "33 Knockback",
            "34 Cast Successful",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63"
        };
        #endregion
    }
}
