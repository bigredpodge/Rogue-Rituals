using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using TMPro;

[System.Serializable]
public class Devil
{
    [SerializeField] DevilBase _base;
    [SerializeField] int level;

    public Devil(DevilBase dBase, int dLevel) {
        _base = dBase;
        level = dLevel;

        Init();
    }
    
    public DevilBase Base { 
        get {
            return _base;
        }
        set {
            _base = value;
        } 
    }

    public int Level { 
        get {
            return level;
        }
        set {
            level = value;
        } 
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> IVs { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public Dictionary<Condition, int> Statuses{ get; set;}
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;
    

    public void Init() {
        //Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves) {
            if (move.Level <= Level) {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= 4)
                break;
        }

        //Generate IVs - later we might want to specify IVs for progression.
        GenerateIVs();

        Exp = Base.GetExpForLevel(Level);
        ResetStatBoosts();
        CalculateStats();

        StatusChanges = new Queue<string>();

        HP = MaxHP;
        Statuses = new Dictionary<Condition, int>();
    }

    void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.MaxHP, Mathf.FloorToInt((4 * Base.MaxHP + IVs[Stat.MaxHP] * Level) / 100f) + 10 + Level);
        Stats.Add(Stat.Strength, Mathf.FloorToInt((4 * Base.Strength + IVs[Stat.Strength] * Level) / 100f) + 5);
        Stats.Add(Stat.Discipline, Mathf.FloorToInt((4 * Base.Discipline + IVs[Stat.Discipline]* Level) / 100f) + 5);
        Stats.Add(Stat.Fortitude, Mathf.FloorToInt((4 * Base.Fortitude + IVs[Stat.Fortitude] * Level) / 100f) + 5);
        Stats.Add(Stat.Willpower, Mathf.FloorToInt((4 * Base.Willpower + IVs[Stat.Willpower] * Level) / 100f) + 5);
        Stats.Add(Stat.Initiative, Mathf.FloorToInt((4 * Base.Initiative + IVs[Stat.Initiative] * Level) / 100f) + 5);
    }

    void GenerateIVs() {
        IVs = new Dictionary<Stat, int>() {
            {Stat.MaxHP, Random.Range(0, 31)},
            {Stat.Strength, Random.Range(0, 31)},
            {Stat.Discipline, Random.Range(0, 31)},
            {Stat.Fortitude, Random.Range(0, 31)},
            {Stat.Willpower, Random.Range(0, 31)},
            {Stat.Initiative, Random.Range(0, 31)},
        };
    }

    void ResetStatBoosts() {
        StatBoosts = new Dictionary<Stat, int>() {
            {Stat.MaxHP, 0},
            {Stat.Strength, 0},
            {Stat.Discipline, 0},
            {Stat.Fortitude, 0},
            {Stat.Willpower, 0},
            {Stat.Initiative, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    int GetStat(Stat stat) {
        int statVal = Stats[stat];

        //Statboostin
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 2f, 2.5f, 3f, 3.5f, 4f};
        if (boost >= 0) 
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts) {
        foreach (var statBoost in statBoosts) {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            if (boost > 0) 
                StatusChanges.Enqueue(Base.Name+"'s "+stat+" rose!");
            else
                StatusChanges.Enqueue(Base.Name+"'s "+stat+" fell!");

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -5, 5);

            Debug.Log(stat+"has been boosted to"+StatBoosts[stat]);
        }
    }

    public bool CheckForLevelUp() {
        if (Exp > Base.GetExpForLevel(level + 1)) {
            ++level;
            return true;
        }

        return false;
    }

    public Dictionary<Stat, int> BoostStatsAfterLevelUp() {
        var oldStats = new Dictionary<Stat, int> {
            {Stat.MaxHP, MaxHP},
            {Stat.Strength, Strength},
            {Stat.Discipline, Discipline},
            {Stat.Fortitude, Fortitude},
            {Stat.Willpower, Willpower},
            {Stat.Initiative, Initiative},
        };

        CalculateStats();
        
        var statGrowths = new Dictionary<Stat, int> {
            {Stat.MaxHP, MaxHP - oldStats[Stat.MaxHP]},
            {Stat.Strength, Strength - oldStats[Stat.Strength]},
            {Stat.Discipline, Discipline - oldStats[Stat.Discipline]},
            {Stat.Fortitude, Fortitude - oldStats[Stat.Fortitude]},
            {Stat.Willpower, Willpower - oldStats[Stat.Willpower]},
            {Stat.Initiative, Initiative - oldStats[Stat.Initiative]},
        };

        HP = HP + statGrowths[Stat.MaxHP];
        HpChanged = true;

        return statGrowths;
    }
 
//Calculate stat based on level
    public int MaxHP {
        get { return GetStat(Stat.MaxHP); }
    }
    public int Strength {
        get { return GetStat(Stat.Strength); }
    }
    public int Discipline {
        get { return GetStat(Stat.Discipline); }
    }
    public int Fortitude {
        get { return GetStat(Stat.Fortitude); }
    }
    public int Willpower {
        get { return GetStat(Stat.Willpower); }
    }
    public int Initiative {
        get { return GetStat(Stat.Initiative); }
    }

    public DamageDetails TakeDamage(Move move, Devil attacker) {

        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 1.5f;

        float brand = BrandChart.GetEffectivness(move.Base.Brand, this.Base.Brand1) * BrandChart.GetEffectivness(move.Base.Brand, this.Base.Brand2);

        float stab = 1f;
        if (move.Base.Brand == this.Base.Brand1 || move.Base.Brand == this.Base.Brand2)
            stab = 1.5f;

        var damageDetails = new DamageDetails() {
            BrandEffectiveness = brand,
            Critical = critical,
            IsFelled = false
        };

        float attack = (move.Base.Category == MoveCategory.Discipline) ? attacker.Discipline : attacker.Strength;
        float defense = (move.Base.Category == MoveCategory.Discipline) ? Willpower : Fortitude;

        float modifiers = Random.Range(0.8f, 1f) * brand * critical * stab;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);
        
        return damageDetails;
    }

    public void UpdateHP(int damage) {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    public Move GetRandomMove() {

        var movesWithPP = Moves.Where(x => x.AP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public void SetStatus(ConditionID conditionId, int time) {
        var status = ConditionsDB.Conditions[conditionId];
        if (Statuses.ContainsKey(status))
            Statuses[status] = time;
        else {
            Statuses.Add(status, time);
            status?.OnStart?.Invoke(this);
            StatusChanges.Enqueue(Base.Name + status.StartMessage);
            OnStatusChanged?.Invoke();
        }
    }

    public void CureStatus(ConditionID conditionId) {
        var status = ConditionsDB.Conditions[conditionId];
        if (!Statuses.ContainsKey(status))
            return; 
        StatusChanges.Enqueue(Base.Name + " shook off its " + ConditionsDB.Conditions[conditionId].Name);
        Statuses.Remove(status);
        OnStatusChanged?.Invoke();
    }

    public bool OnBeforeMove() {
        bool canPerformMove = true;
        if (Statuses.Count == 0)
            return canPerformMove;

        for (int i = 0; i < Statuses.Count; i++) {
            var condition = Statuses.ElementAt(i).Key;
            
            if (condition?.OnBeforeMove != null) {
                Statuses[condition] -= 1;

                if (Statuses[condition] <= 0) {
                    CureStatus(condition.Id);
                    break;
                }

                if (!condition.OnBeforeMove(this)) 
                    canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn () {
        for (int i = 0; i < Statuses.Count; i++) {
            var condition = Statuses.ElementAt(i).Key;
            if(condition?.OnAfterTurn != null) {
                condition?.OnAfterTurn?.Invoke(this);

                Statuses[condition] -= 1;
                if (Statuses[condition] <= 0) {
                    CureStatus(condition.Id);
                    break;
                }
            }
        }
    }

    public void OnRecall() {
        ResetStatBoosts();
    }
    public void OnBattleOver() {
        OnRecall();
    }

}


public class DamageDetails {
    public bool IsFelled { get; set; }
    public float Critical { get; set; }
    public float BrandEffectiveness { get; set; }
}
