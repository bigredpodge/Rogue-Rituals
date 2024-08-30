using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Devil/Create New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] DevilBrand brand;
    [SerializeField] int power;
    [SerializeField] int accuracy = 100;
    [SerializeField] bool alwaysHits;
    [SerializeField] int ap = 10;
    [SerializeField] int priority;
    [SerializeField] MoveCategory moveCategory;
    [SerializeField] List<MoveEffects> effects = new List<MoveEffects>();

//Following allows properties to be accessed as functions
    public string Name {
        get { return name; }
    }
    public string Description {
        get { return description; }
    }
    public DevilBrand Brand {
        get { return brand; }
    }
    public int Power {
        get { return power; }
    }
    public int Accuracy {
        get { return accuracy; }
    }
    public bool AlwaysHits {
        get { return alwaysHits; }
    }
    public int AP {
        get { return ap; }
    }
    public int Priority {
        get { return priority; }
    }
    public MoveCategory Category {
        get { return moveCategory; }
    }
    public List<MoveEffects> Effects {
        get { return effects; }
    }

}

[System.Serializable]
public class MoveEffects {
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] int statusTime = 10;
    [SerializeField] int chance = 100;
    [SerializeField] MoveTarget target;

    public List<StatBoost> Boosts {
        get { return boosts; }
    }
    public ConditionID Status {
        get { return status; }
    }
    public int StatusTime {
        get { return statusTime; }
    }
    public int Chance {
        get { return chance; }
    }
    public MoveTarget Target {
        get { return target; }
    }

}

[System.Serializable]
public class StatBoost {
    public Stat stat;
    public int boost;
}


public enum MoveCategory {
    Strength, Discipline, Status
}

public enum MoveTarget {
    Foe, Self
}