using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Devil", menuName = "Devil/Create New Devil")]
[System.Serializable]
public class DevilBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string rank;
    [SerializeField] string domain;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite sprite;

    [SerializeField] GameObject model;

    [SerializeField] DevilBrand brand1;
    [SerializeField] DevilBrand brand2;

    [SerializeField] int maxHP;
    [SerializeField] int strength;
    [SerializeField] int discipline;
    [SerializeField] int fortitude;
    [SerializeField] int willpower;
    [SerializeField] int initiative;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;
    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> teachableMoves;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level) {
        if (growthRate == GrowthRate.Fast) {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.Medium) {
            return level * level * level;
        }

        return -1;
    }


//Following allows properties to be accessed as functions
    public string Name {
        get { return name; }
    }
    public string Rank {
        get { return rank; }
    }
    public string Domain {
        get { return domain; }
    }
    public string Description {
        get { return description; }
    }
    public Sprite Sprite {
        get { return sprite; }
    }
    public GameObject Model {
        get { return model; }
    }
    public DevilBrand Brand1 {
        get { return brand1; }
    }
    public DevilBrand Brand2 {
        get { return brand2; }
    }
    public int MaxHP {
        get { return maxHP; }
    }
    public int Strength {
        get { return strength; }
    }
    public int Discipline {
        get { return discipline; }
    }
    public int Fortitude {
        get { return fortitude; }
    }
    public int Willpower {
        get { return willpower; }
    }
    public int Initiative {
        get { return initiative; }
    }
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
    public int CatchRate => catchRate;
    public List<LearnableMove> LearnableMoves {
        get { return learnableMoves; }
    }
    public List<MoveBase> TeachableMoves {
        get { return teachableMoves; }
    }

}

[System.Serializable]
public class LearnableMove {
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase Base {
        get { return moveBase; }
    }
    public int Level {
        get { return level; }
    }
}

public enum DevilBrand {
        None,
        Heat,
        Squall,
        Filth,
        Insight,
        Rage,
        Allure,
        Lunacy,
        Discord,
        Misery
    }

public enum GrowthRate {
    Fast, Medium
}

public enum Stat {
    MaxHP,
    Strength,
    Discipline,
    Fortitude,
    Willpower,
    Initiative,

    // moveAccuracy Stats
    Accuracy,
    Evasion
}

public class BrandChart {
        static float[][] chart = 
        {
            //                   HEA SQU FIL INS RAG ALL LUN DIS MIS
            /*HEA*/ new float[] {.5f,.5f, 2f, 2f, 1f, 2f, 1f, 1f, 1f },
            /*SQU*/ new float[] { 2f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 2f },
            /*FIL*/ new float[] { 2f, 2f,.5f, 1f,.5f, 1f, 2f, 1f, 2f },
            /*INS*/ new float[] { 1f, 1f, 2f, 1f,.5f,.5f, 2f, 1f, 1f },
            /*RAG*/ new float[] { 1f, 1f,.5f, 2f, 1f, 1f, 1f, 2f, 1f },
            /*ALL*/ new float[] { 1f,.5f, 1f,.5f, 2f, 2f, 2f, 1f,.5f },
            /*LUN*/ new float[] { 1f, 2f, 1f, 1f, 2f,.5f, 2f, 1f, 1f },
            /*DIS*/ new float[] { 1f, 2f, 2f, 1f, 2f, 1f, 2f,.5f,.5f },
            /*MIS*/ new float[] { 1f, 1f,.5f, 2f, 1f,.5f,.5f, 2f, 2f }
        };

        public static float GetEffectivness(DevilBrand attackBrand, DevilBrand defenseBrand) {
            if (attackBrand == DevilBrand.None || defenseBrand == DevilBrand.None)
                return 1;

            int row = (int)attackBrand - 1;
            int col = (int)defenseBrand - 1;

            return chart[row][col];
        }
}
