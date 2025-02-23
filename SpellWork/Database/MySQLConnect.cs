using MySql.Data.MySqlClient;
using SpellWork.DBC.Structures;
using SpellWork.Properties;
using SpellWork.Spell;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpellWork.Database
{
    public static class MySqlConnection
    {
        public static bool Connected { get; private set; }
        public static List<string> Dropped = new List<string>();
        public static List<SpellProcEntry> SpellProcEvent = new List<SpellProcEntry>();

        private static string ConnectionString
        {
            get
            {
                if (Settings.Default.Host == ".")
                    return
                        $"Server=localhost;Pipe={Settings.Default.PortOrPipe};UserID={Settings.Default.User};Password={Settings.Default.Pass};Database={Settings.Default.WorldDbName};CharacterSet=utf8mb4;ConnectionTimeout=5;ConnectionProtocol=Pipe;";

                return
                    $"Server={Settings.Default.Host};Port={Settings.Default.PortOrPipe};UserID={Settings.Default.User};Password={Settings.Default.Pass};Database={Settings.Default.WorldDbName};CharacterSet=utf8mb4;ConnectionTimeout=5;";
            }
        }

        private static string GetSpellName(uint id)
        {
            if (DBC.DBC.SpellInfoStore.ContainsKey((int)id))
                return DBC.DBC.SpellInfoStore[(int)id].NameAndSubname;

            Dropped.Add($"DELETE FROM `spell_proc` WHERE `SpellId`={id};\r\n");
            return string.Empty;
        }

        public static void SelectProc(string query)
        {
            if (!Connected)
                return;

            Dropped.Clear();
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(query, conn))
                {
                    SpellProcEvent.Clear();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var spellId = reader.GetInt32(0);
                            SpellProcEvent.Add(new SpellProcEntry
                            {
                                SpellId              = spellId,
                                SpellName            = GetSpellName((uint)Math.Abs(spellId)),
                                SchoolMask           = (SpellSchoolMask)reader.GetByte(1),
                                SpellFamilyName      = (SpellFamilyNames)reader.GetUInt16(2),
                                SpellFamilyMask      = new[]
                                {
                                    reader.GetUInt32(3),
                                    reader.GetUInt32(4),
                                    reader.GetUInt32(5),
                                    reader.GetUInt32(6)
                                },
                                ProcFlags            = (ProcFlags)reader.GetUInt32(7),
                                SpellTypeMask        = (ProcFlagsSpellType)reader.GetUInt32(8),
                                SpellPhaseMask       = (ProcFlagsSpellPhase)reader.GetUInt32(9),
                                HitMask              = (ProcFlagsHit)reader.GetUInt32(10),
                                AttributesMask       = (ProcAttributes)reader.GetUInt32(11),
                                DisableEffectsMask   = reader.GetUInt32(12),
                                ProcsPerMinute       = reader.GetFloat(13),
                                Chance               = reader.GetFloat(14),
                                Cooldown             = reader.GetUInt32(15),
                                Charges              = reader.GetByte(16)
                            });
                        }
                    }
                }
            }
        }

        public static void LoadServersideSpells()
        {
            if (!Connected)
                return;

            var spellsQuery =
                "SELECT " +                         // fields:  // types:
                "Id, " +                            // 0    uint
                "DifficultyID, " +                  // 1    int
                "CategoryId, " +                    // 2    uint
                "Dispel, " +                        // 3    uint
                "Mechanic, " +                      // 4    uint 
                "Attributes, " +                    // 5    uint
                "AttributesEx, " +                  // 6    uint
                "AttributesEx2, " +                 // 7    uint
                "AttributesEx3, " +                 // 8    uint
                "AttributesEx4, " +                 // 9    uint
                "AttributesEx5, " +                 // 10   uint
                "AttributesEx6, " +                 // 11   uint
                "AttributesEx7, " +                 // 12   uint
                "AttributesEx8, " +                 // 13   uint
                "AttributesEx9, " +                 // 14   uint
                "AttributesEx10, " +                // 15   uint
                "AttributesEx11, " +                // 16   uint
                "AttributesEx12, " +                // 17   uint
                "AttributesEx13, " +                // 18   uint
                "AttributesEx14, " +                // 19   uint
                "Stances, " +                       // 20   ulong
                "StancesNot, " +                    // 21   ulong
                "Targets, " +                       // 22   uint
                "TargetCreatureType, " +            // 23   uint
                "RequiresSpellFocus, " +            // 24   uint
                "FacingCasterFlags, " +             // 25   uint
                "CasterAuraState, " +               // 26   uint
                "TargetAuraState, " +               // 27   uint
                "ExcludeCasterAuraState, " +        // 28   uint
                "ExcludeTargetAuraState, " +        // 29   uint
                "CasterAuraSpell, " +               // 30   uint
                "TargetAuraSpell, " +               // 31   uint
                "ExcludeCasterAuraSpell, " +        // 32   uint
                "ExcludeTargetAuraSpell, " +        // 33   uint
                "CasterAuraType, " +                // 34   int
                "TargetAuraType, " +                // 35   int
                "ExcludeCasterAuraType, " +         // 36   int
                "ExcludeTargetAuraType, " +         // 37   int
                "CastingTimeIndex, " +              // 38   uint
                "RecoveryTime, " +                  // 39   uint
                "CategoryRecoveryTime, " +          // 40   uint
                "StartRecoveryCategory, " +         // 41   uint
                "StartRecoveryTime, " +             // 42   uint
                "InterruptFlags, " +                // 43   uint
                "AuraInterruptFlags1, " +           // 44   uint
                "AuraInterruptFlags2, " +           // 45   uint
                "ChannelInterruptFlags1, " +        // 46   uint
                "ChannelInterruptFlags2, " +        // 47   uint
                "ProcFlags, " +                     // 48   uint
                "ProcFlags2, " +                    // 49   uint
                "ProcChance, " +                    // 50   uint
                "ProcCharges, " +                   // 51   uint
                "ProcCooldown, " +                  // 52   uint
                "ProcBasePPM, " +                   // 53   float
                "MaxLevel, " +                      // 54   uint
                "BaseLevel, " +                     // 55   uint
                "SpellLevel, " +                    // 56   uint
                "DurationIndex, " +                 // 57   uint
                "RangeIndex, " +                    // 58   uint
                "Speed, " +                         // 59   float
                "LaunchDelay, " +                   // 60   float
                "StackAmount, " +                   // 61   uint
                "EquippedItemClass, " +             // 62   int
                "EquippedItemSubClassMask, " +      // 63   int
                "EquippedItemInventoryTypeMask, " + // 64   int
                "ContentTuningId, " +               // 65   uint
                "SpellName, " +                     // 66   string
                "ConeAngle, " +                     // 67   float
                "ConeWidth, " +                     // 68   float
                "MaxTargetLevel, " +                // 69   uint
                "MaxAffectedTargets, " +            // 70   uint
                "SpellFamilyName, " +               // 71   uint
                "SpellFamilyFlags1, " +             // 72   uint
                "SpellFamilyFlags2, " +             // 73   uint
                "SpellFamilyFlags3, " +             // 74   uint
                "SpellFamilyFlags4, " +             // 75   uint
                "DmgClass, " +                      // 76   uint
                "PreventionType, " +                // 77   uint
                "AreaGroupId, " +                   // 78   int
                "SchoolMask, " +                    // 79   uint
                "ChargeCategoryId " +               // 80   uint
                "FROM serverside_spell";

            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(spellsQuery, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var difficulty = reader.GetInt32(1);
                            if (difficulty != 0)
                                continue;

                            var spellId = (int)reader.GetUInt32(0);
                            var spellInfo = new SpellInfo(reader.GetString(66) + " (SERVERSIDE)",
                                new SpellEntry()
                                {
                                    ID = spellId
                                });

                            spellInfo.AuraOptions = new SpellAuraOptionsEntry()
                            {
                                CumulativeAura = (ushort)reader.GetUInt32(61),
                                ProcCategoryRecovery = (int)reader.GetUInt32(52),
                                ProcChance = (byte)reader.GetUInt32(50),
                                ProcCharges = (int)reader.GetUInt32(51),
                                ProcTypeMask = new[] { (int)reader.GetUInt32(48), 0 }
                            };

                            spellInfo.AuraRestrictions = new SpellAuraRestrictionsEntry()
                            {
                                CasterAuraState = (byte)reader.GetUInt32(26),
                                TargetAuraState = (byte)reader.GetUInt32(27),
                                ExcludeCasterAuraState = (byte)reader.GetUInt32(28),
                                ExcludeTargetAuraState = (byte)reader.GetUInt32(29),
                                CasterAuraSpell = (int)reader.GetUInt32(30),
                                TargetAuraSpell = (int)reader.GetUInt32(31),
                                ExcludeCasterAuraSpell = (int)reader.GetUInt32(32),
                                ExcludeTargetAuraSpell = (int)reader.GetUInt32(33)
                            };

                            spellInfo.CastingRequirements = new SpellCastingRequirementsEntry()
                            {
                                FacingCasterFlags = (byte)reader.GetUInt32(25),
                                RequiredAreasID = (ushort)reader.GetInt32(78),
                                RequiresSpellFocus = (ushort)reader.GetUInt32(24)
                            };

                            spellInfo.Categories = new SpellCategoriesEntry()
                            {
                                Category = (short)reader.GetUInt32(2),
                                DefenseType = (sbyte)reader.GetUInt32(76),
                                DispelType = (sbyte)reader.GetUInt32(3),
                                Mechanic = (sbyte)reader.GetUInt32(4),
                                PreventionType = (sbyte)reader.GetUInt32(77),
                                StartRecoveryCategory = (short)reader.GetUInt32(41),
                                ChargeCategory = (short)reader.GetUInt32(80)
                            };

                            spellInfo.ClassOptions = new SpellClassOptionsEntry()
                            {
                                SpellClassSet = (byte)reader.GetUInt32(71),
                                SpellClassMask = new[]
                                {
                                    (int)reader.GetUInt32(72),
                                    (int)reader.GetUInt32(73),
                                    (int)reader.GetUInt32(74),
                                    (int)reader.GetUInt32(75)
                                }
                            };

                            spellInfo.Cooldowns = new SpellCooldownsEntry()
                            {
                                CategoryRecoveryTime = (int)reader.GetUInt32(40),
                                RecoveryTime = (int)reader.GetUInt32(39),
                                StartRecoveryTime = (int)reader.GetUInt32(42)
                            };

                            spellInfo.EquippedItems = new SpellEquippedItemsEntry()
                            {
                                EquippedItemClass = (sbyte)reader.GetInt32(62),
                                EquippedItemInvTypes = reader.GetInt32(64),
                                EquippedItemSubclass = reader.GetInt32(63)
                            };

                            spellInfo.Interrupts = new SpellInterruptsEntry()
                            {
                                InterruptFlags = (short)reader.GetUInt32(43),
                                AuraInterruptFlags = new[]
                                {
                                    (int)reader.GetUInt32(44),
                                    (int)reader.GetUInt32(45)
                                },
                                ChannelInterruptFlags = new[]
                                {
                                    (int)reader.GetUInt32(46),
                                    (int)reader.GetUInt32(47)
                                }
                            };

                            spellInfo.Levels = new SpellLevelsEntry()
                            {
                                MaxLevel = (short)reader.GetUInt32(54),
                                BaseLevel = (short)reader.GetUInt32(55),
                                SpellLevel = (short)reader.GetUInt32(56)
                            };

                            spellInfo.Misc = new SpellMiscEntry()
                            {
                                Attributes = new[]
                                {
                                    (int)reader.GetUInt32(5),
                                    (int)reader.GetUInt32(6),
                                    (int)reader.GetUInt32(7),
                                    (int)reader.GetUInt32(8),
                                    (int)reader.GetUInt32(9),
                                    (int)reader.GetUInt32(10),
                                    (int)reader.GetUInt32(11),
                                    (int)reader.GetUInt32(12),
                                    (int)reader.GetUInt32(13),
                                    (int)reader.GetUInt32(14),
                                    (int)reader.GetUInt32(15),
                                    (int)reader.GetUInt32(16),
                                    (int)reader.GetUInt32(17),
                                    (int)reader.GetUInt32(18),
                                    (int)reader.GetUInt32(19)
                                },
                                CastingTimeIndex = (ushort)reader.GetUInt32(34),
                                DurationIndex = (ushort)reader.GetUInt32(52),
                                RangeIndex = (ushort)reader.GetUInt32(53),
                                SchoolMask = (byte)reader.GetUInt32(74),
                                Speed = reader.GetFloat(54),
                                LaunchDelay = reader.GetFloat(55),
                                ContentTuningID = (int)reader.GetUInt32(60)
                            };

                            spellInfo.ProcsPerMinute = new SpellProcsPerMinuteEntry()
                            {
                                BaseProcRate = reader.GetFloat(53)
                            };

                            spellInfo.Shapeshift = new SpellShapeshiftEntry()
                            {
                                ShapeshiftMask = new[]
                                {
                                    (int)(reader.GetUInt64(20) & 0xFFFFFFFF),
                                    (int)(reader.GetUInt64(20) >> 32)
                                },
                                ShapeshiftExclude = new[]
                                {
                                    (int)(reader.GetUInt64(21) & 0xFFFFFFFF),
                                    (int)(reader.GetUInt64(21) >> 32)
                                },
                            };

                            spellInfo.TargetRestrictions = new SpellTargetRestrictionsEntry()
                            {
                                ConeDegrees = reader.GetFloat(67),
                                MaxTargets = (byte)reader.GetUInt32(70),
                                MaxTargetLevel = reader.GetInt32(64),
                                TargetCreatureType = (short)reader.GetUInt32(23),
                                Targets = (int)reader.GetUInt32(22),
                                Width = reader.GetFloat(68)
                            };

                            if (DBC.DBC.SpellDuration.TryGetValue(spellInfo.Misc.DurationIndex, out var duration))
                                spellInfo.DurationEntry = duration;

                            if (DBC.DBC.SpellRange.TryGetValue(spellInfo.Misc.RangeIndex, out var range))
                                spellInfo.Range = range;

                            DBC.DBC.SpellInfoStore[spellId] = spellInfo;
                        }
                    }
                }

                var effectsQuery = "SELECT " +             // fields:  // types:
                    "SpellID, " +                          // 0     uint
                    "EffectIndex, " +                      // 1     int
                    "DifficultyID, " +                     // 2     int
                    "Effect, " +                           // 3     int
                    "EffectAura, " +                       // 4     short
                    "EffectAmplitude, " +                  // 5     float
                    "EffectAttributes, " +                 // 6     int
                    "EffectAuraPeriod, " +                 // 7     int
                    "EffectBonusCoefficient, " +           // 8     float
                    "EffectChainAmplitude, " +             // 9     float
                    "EffectChainTargets, " +               // 10    int
                    "EffectItemType, " +                   // 11    int
                    "EffectMechanic, " +                   // 12    int
                    "EffectPointsPerResource, " +          // 13    float
                    "EffectPosFacing, " +                  // 14    float
                    "EffectRealPointsPerLevel, " +         // 15    float
                    "EffectTriggerSpell, " +               // 16    int
                    "BonusCoefficientFromAP, " +           // 17    float
                    "PvpMultiplier, " +                    // 18    float
                    "Coefficient, " +                      // 19    float
                    "Variance, " +                         // 20    float
                    "ResourceCoefficient, " +              // 21    float
                    "GroupSizeBasePointsCoefficient, " +   // 22    float
                    "EffectBasePoints, " +                 // 23    float
                    "EffectMiscValue1, " +                 // 24    int
                    "EffectMiscValue2, " +                 // 25    int
                    "EffectRadiusIndex1, " +               // 26    uint
                    "EffectRadiusIndex2, " +               // 27    uint
                    "EffectSpellClassMask1, " +            // 28    int
                    "EffectSpellClassMask2, " +            // 29    int
                    "EffectSpellClassMask3, " +            // 30    int
                    "EffectSpellClassMask4, " +            // 31    int
                    "ImplicitTarget1, " +                  // 32    short
                    "ImplicitTarget2 " +                   // 33    short
                    "FROM serverside_spell_effect";


                using (var command = new MySqlCommand(effectsQuery, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var difficulty = reader.GetInt32(2);
                            if (difficulty != 0)
                                continue;

                            var spellId = (int)reader.GetUInt32(0);
                            if (!DBC.DBC.SpellInfoStore.TryGetValue((int)spellId, out var spellInfo))
                                continue;

                            var effect = new SpellEffectEntry()
                            {
                                EffectIndex = reader.GetInt32(1),
                                Effect = reader.GetInt32(3),
                                EffectAura = reader.GetInt16(4),
                                EffectAmplitude = reader.GetFloat(5),
                                EffectAttributes = reader.GetInt32(6),
                                EffectAuraPeriod = reader.GetInt32(7),
                                EffectBonusCoefficient = reader.GetFloat(8),
                                EffectChainAmplitude = reader.GetFloat(9),
                                EffectChainTargets = reader.GetInt32(10),
                                EffectItemType = reader.GetInt32(11),
                                EffectMechanic = reader.GetInt32(12),
                                EffectPointsPerResource = reader.GetFloat(13),
                                EffectPosFacing = reader.GetFloat(14),
                                EffectRealPointsPerLevel = reader.GetFloat(15),
                                EffectTriggerSpell = reader.GetInt32(16),
                                BonusCoefficientFromAP = reader.GetFloat(17),
                                PvpMultiplier = reader.GetFloat(18),
                                Coefficient = reader.GetFloat(19),
                                Variance = reader.GetFloat(20),
                                ResourceCoefficient = reader.GetFloat(21),
                                GroupSizeBasePointsCoefficient = reader.GetFloat(22),
                                EffectBasePoints = reader.GetInt32(23),
                                EffectMiscValue = new[]
                                {
                                    reader.GetInt32(24),
                                    reader.GetInt32(25)
                                },
                                EffectRadiusIndex = new[]
                                {
                                    reader.GetUInt32(26),
                                    reader.GetUInt32(27),
                                },
                                EffectSpellClassMask = new[]
                                {
                                    reader.GetInt32(28),
                                    reader.GetInt32(29),
                                    reader.GetInt32(30),
                                    reader.GetInt32(31)
                                },
                                ImplicitTarget = new[]
                                {
                                    reader.GetInt16(32),
                                    reader.GetInt16(33)
                                },
                                SpellID = (int)spellId
                            };

                            spellInfo.SpellEffectInfoStore.Add(new SpellEffectInfo(effect));
                        }
                    }
                }
            }
        }

        public static void Insert(string query)
        {
            if (!Connected || Settings.Default.DbIsReadOnly)
                return;

            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(query, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void TestConnect()
        {
            if (!Settings.Default.UseDbConnect)
            {
                Connected = false;
                return;
            }

            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.Close();
                }
                Connected = true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Errno {ex.Number}{Environment.NewLine}{ex.Message}");
                Connected = false;
            }
            catch
            {
                Connected = false;
            }
        }
    }
}
