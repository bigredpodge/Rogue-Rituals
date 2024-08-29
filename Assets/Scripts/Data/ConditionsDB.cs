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
        {ConditionID.psn, new Condition() {
                Name = "Poison",
                StartMessage = " has been poisoned.",
                OnAfterTurn = (Devil devil) => {
                    devil.UpdateHP(devil.MaxHP / 8);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " was hurt by poison.");
                }
            }
        },
        {ConditionID.brn, new Condition() {
                Name = "Burn",
                StartMessage = " is now burning.",
                OnAfterTurn = (Devil devil) => {
                    devil.UpdateHP(devil.MaxHP / 8);
                    devil.StatusChanges.Enqueue(devil.Base.Name + " is burning.");
                }
            }
        },
        {ConditionID.slp, new Condition() {
                Name = "Sleep",
                StartMessage = " falls asleep.",
                OnStart = (Devil devil) => {
                    devil.StatusTime = Random.Range(1, 4);
                },
                OnBeforeMove = (Devil devil) => {
                    if (devil.StatusTime <= 0) {
                        devil.CureStatus(ConditionID.slp);
                        devil.StatusChanges.Enqueue(devil.Base.Name+" woke up.");
                        return true;
                    }

                    devil.StatusTime--;
                    devil.StatusChanges.Enqueue(devil.Base.Name+" is sleeping.");
                    return false;
                }
            }
        },
        {ConditionID.par, new Condition() {
                Name = "Paralyse",
                StartMessage = " becomes paralysed.",
                OnBeforeMove = (Devil devil) => {
                    if (Random.Range(1, 5) == 1) {
                        devil.StatusChanges.Enqueue(devil.Base.Name + " is paralysed.");
                        return false;
                    }
                    return true;
                }
            }
        },
        {ConditionID.frz, new Condition() {
                Name = "Freeze",
                StartMessage = " becomes frozen.",
                OnBeforeMove = (Devil devil) => {
                    if (Random.Range(1, 5) == 1) {
                        devil.CureStatus(ConditionID.frz);
                        devil.StatusChanges.Enqueue(devil.Base.Name + " is no longer frozen!");
                        return true;
                    }
                    devil.StatusChanges.Enqueue(devil.Base.Name + " is frozen.");
                    return false;
                }
            }
        },
        {ConditionID.dom, new Condition() {
                Name = "Doom",
                StartMessage = " is doomed.",
                OnStart = (Devil devil) => {
                    //effect
                }
            }
        },
    };

    public static float GetStatusBonus(Condition condition) {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz )
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn )
            return 1.5f;
        
        return 1f;
    }
}

public enum ConditionID {
    none, psn, brn, slp, par, frz, dom
}
