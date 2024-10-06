using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static void Init() {
        foreach (var kvp in Conditions) {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() {
        {ConditionID.brn, new Condition() {
                Name = "Burn",
                StartMessage = " is now burning.",

                OnStart = (Devil devil) => {
                    devil.BurnSeverity = 1;
                },

                OnRefresh = (Devil devil) => {
                    if (devil.BurnSeverity < 4) {
                        devil.BurnSeverity++;
                        devil.StatusChanges.Enqueue(devil.Base.Name + "'s burn got worse!");
                    }
                    else
                        devil.StatusChanges.Enqueue(devil.Base.Name + "'s burn couldn't get worse!");
                },

                OnAfterTurn = (Devil devil) => {
                    devil.DamageHP(devil.BurnSeverity * (devil.MaxHP / 8));
                    devil.StatusChanges.Enqueue(devil.Base.Name + " is burning.");
                }
            }
        },
        {ConditionID.slp, new Condition() {
                Name = "Sleep",
                StartMessage = " falls asleep.",

                OnStart = (Devil devil) => {
                    devil.SleepTime = Random.Range(1, 4);
                },

                OnRefresh = (Devil devil) => {
                    devil.SleepTime = devil.SleepTime + Random.Range(1, 2);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " sleeps deeper.");
                },

                OnBeforeMove = (Devil devil) => {
                    if (devil.SleepTime <= 0) {
                        devil.CureStatus(ConditionID.slp);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" woke up.");
                        return true;
                    }

                    devil.SleepTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name+" is sleeping.");
                    return false;
                }
            }
        },
        {ConditionID.dom, new Condition() {
                Name = "Doom",
                StartMessage = " is doomed.",

                OnStart = (Devil devil) => {
                    devil.DoomTime = 8;
                },

                OnRefresh = (Devil devil) => {
                    devil.DoomTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name + "'s doom inches closer...");

                    if (devil.DoomTime <= 0) {
                        devil.DamageHP(devil.MaxHP);
                        devil.StatusChanges.Enqueue(devil.Base.Name + "'s time has come.");
                    }
                },

                OnAfterTurn = (Devil devil) => {
                    devil.DoomTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name + "'s doom inches closer...");

                    if (devil.DoomTime <= 0) {
                        devil.DamageHP(devil.MaxHP);
                        devil.StatusChanges.Enqueue(devil.Base.Name + "'s time has come.");
                    }
                }
            }
        },
        {ConditionID.bsk, new Condition() {
                Name = "Berserk",
                StartMessage = " is enraged.",

                OnStart = (Devil devil) => {
                    devil.BerserkTime = Random.Range(2, 5);
                },

                OnRefresh = (Devil devil) => {
                    devil.BerserkTime = devil.BerserkTime + Random.Range(1, 2);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " rages deeper.");
                },

                //Only allow strength moves, and pick them at random.

                OnAfterTurn = (Devil devil) => {
                    if (devil.BerserkTime <= 0) {
                        devil.CureStatus(ConditionID.bsk);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" calmed down.");
                        return;
                    }

                    devil.BerserkTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name + " is furious.");
                }
            }
        },
        {ConditionID.fbl, new Condition() {
                Name = "Feeble",
                StartMessage = " is enfeebled.",

                //Max HP lowers with HP, preventing heals
            }
        },
        {ConditionID.dsp, new Condition() {
                Name = "Despair",
                StartMessage = " goes into despair.",

                OnStart = (Devil devil) => {
                    devil.DespairTime = Random.Range(1, 4);
                },

                OnRefresh = (Devil devil) => {
                    devil.DespairTime = devil.DespairTime + Random.Range(1, 2);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " spirals into despair.");
                },

                OnAfterTurn = (Devil devil) => {
                    if (devil.DespairTime <= 0) {
                        devil.CureStatus(ConditionID.dsp);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" perked up.");
                        return;
                    }

                    devil.DespairTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name + " wallows in despair.");

                    var randomStat = (Stat)System.Enum.ToObject(typeof(Stat), Random.Range(0, 8));
                    StatBoost randomDebuff = new StatBoost {
                                stat = randomStat,
                                boost = -1
                    };

                    devil.ApplyBoost(randomDebuff);
                }
            }
        },
        {ConditionID.wth, new Condition() {
                Name = "Wither",
                StartMessage = " is withering away.",

                OnStart = (Devil devil) => {
                    devil.WitherTime = Random.Range(2, 5);
                },

                //Reduces EXP and levels down.

                OnAfterTurn = (Devil devil) => {
                    if (devil.WitherTime <= 0) {
                        devil.CureStatus(ConditionID.wth);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" calmed down.");
                        return;
                    }

                    devil.WitherTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name + " is furious.");
                }
            }
        },
        {ConditionID.frt, new Condition() {
                Name = "Fright",
                StartMessage = " is frightened.",

                //Chance to flee instead of act, which cures the fear
            }
        },
        {ConditionID.dnk, new Condition() {
                Name = "Drunk",
                StartMessage = " is drunk.",

                OnStart = (Devil devil) => {
                    devil.DrunkTime = Random.Range(1, 4);
                },

                OnRefresh = (Devil devil) => {
                    devil.DrunkTime = devil.DrunkTime + Random.Range(1, 2);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " becomes more drunk.");
                },

                OnBeforeMove = (Devil devil) => {
                    if (devil.DrunkTime <= 0) {
                        devil.CureStatus(ConditionID.dnk);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" sobered up.");
                        return true;
                    }

                    devil.DrunkTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name+" sways drunkenly.");

                    var randomNum = Random.Range(0, 7);
                    if (randomNum >= 2)
                        return true;
                    
                    devil.StatusChanges.Enqueue(devil.Base.Name+"'s move failed and they hurt themselves.");
                    devil.DamageHP(devil.MaxHP / 8);
                    return false;
                }
            }
        },
    };


    public static float GetStatusBonus(Condition condition) {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp)
            return 2f;
        else if (condition.Id == ConditionID.brn )
            return 1.5f;
        
        return 1f;
    }
}

public enum ConditionID {
    none, brn, slp, dom, bsk, fbl, dsp, wth, frt, dnk
}
