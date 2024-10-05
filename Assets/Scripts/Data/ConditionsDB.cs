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
                    devil.DoomTime = 10;
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
    none, brn, slp, dom
}
