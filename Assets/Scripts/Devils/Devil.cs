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
    [SerializeField] HeldItem heldItem;

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
    public HeldItem HeldItem { 
        get {
            return heldItem;
        }
        set {
            heldItem = value;
        } 
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> IVs { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public List<Condition> Statuses{ get; set;}
    public int SleepTime { get; set; }
    public int PoisonSeverity { get; set; }
    public int BurnSeverity { get; set; }
    public int DoomTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;
    

    public void Init() {
        //Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves) {
            if (Moves.Count >= DevilBase.MaxNumOfMoves+1)
                Moves.Remove(Moves[Random.Range(0, DevilBase.MaxNumOfMoves)]);

            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
        }

        //Generate IVs - later we might want to specify IVs for progression.
        GenerateIVs();

        Exp = Base.GetExpForLevel(Level);
        ResetStatBoosts();
        CalculateStats();

        StatusChanges = new Queue<string>();

        HP = MaxHP;
        Statuses = new List<Condition>();
    }

    void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.MaxHP, Mathf.FloorToInt(((2 * Base.MaxHP + IVs[Stat.MaxHP]) * Level) / 100f) + 10 + Level);
        Stats.Add(Stat.Strength, Mathf.FloorToInt(((2 * Base.Strength + IVs[Stat.Strength]) * Level) / 100f) + 5);
        Stats.Add(Stat.Discipline, Mathf.FloorToInt(((2 * Base.Discipline + IVs[Stat.Discipline]) * Level) / 100f) + 5);
        Stats.Add(Stat.Fortitude, Mathf.FloorToInt(((2 * Base.Fortitude + IVs[Stat.Fortitude]) * Level) / 100f) + 5);
        Stats.Add(Stat.Willpower, Mathf.FloorToInt(((2 * Base.Willpower + IVs[Stat.Willpower]) * Level) / 100f) + 5);
        Stats.Add(Stat.Initiative, Mathf.FloorToInt(((2 * Base.Initiative + IVs[Stat.Initiative]) * Level) / 100f) + 5);
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
           ApplyBoost(statBoost);
        }
    }

    public void ApplyBoost(StatBoost statBoost) {
        var stat = statBoost.stat;
        var boost = statBoost.boost;

        if (boost > 0) 
            StatusChanges.Enqueue(Base.Name+"'s "+stat+" rose!");
        else
            StatusChanges.Enqueue(Base.Name+"'s "+stat+" fell!");

        StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -5, 5);

        Debug.Log(stat+"has been boosted to"+StatBoosts[stat]);
    }

    public bool CheckForLevelUp() {
        if (Exp > Base.GetExpForLevel(level + 1)) {
            ++level;
            return true;
        }

        return false;
    }

    public void LearnMove(MoveBase move) {
        if (Moves.Count > DevilBase.MaxNumOfMoves)
            return;
        
        Moves.Add(new Move(move));
    }

    public List<MoveBase> GetLearnableMoveAtCurrentLevel() {
        List<LearnableMove> currentLearnableMoves = new List<LearnableMove>(Base.LearnableMoves.Where(x => x.Level == level));
        List<MoveBase> moveBases = new List<MoveBase>();
        for (int i = 0; i < currentLearnableMoves.Count; i++)
            moveBases.Add(currentLearnableMoves[i].Base);
        return moveBases;
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
        OnHPChanged?.Invoke();

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

    public DamageDetails CalculateDamage(Move move, Devil attacker, float weather) {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 1.5f;

        float item = 1f;
        if (attacker.heldItem != null)
            item = attacker.heldItem.GetDamageModifiers(move);

        float brand = BrandChart.GetEffectivness(move.Base.Brand, this.Base.Brand1) * BrandChart.GetEffectivness(move.Base.Brand, this.Base.Brand2);
        if (move.Base.AlwaysSuperEffective)
            brand = 2f;

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


        float modifiers = Random.Range(0.8f, 1f) * brand * critical * stab * item * weather;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        damageDetails.Damage = damage;

        return damageDetails;
    }

    public DamageDetails TakeDamage(Move move, Devil attacker, float weather) {

        var damageDetails = CalculateDamage(move, attacker, weather);

        DamageHP(damageDetails.Damage);
        
        return damageDetails;
    }

    public void DamageHP(int damage) {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        OnHPChanged?.Invoke();
    }

    public void HealHP(int value) {
        HP = Mathf.Clamp(HP + value, 0, MaxHP);
        OnHPChanged?.Invoke();
    }

    public Move GetRandomMove() {
        var movesWithAP = Moves.Where(x => x.AP > 0).ToList();

        int r = Random.Range(0, movesWithAP.Count);
        return movesWithAP[r];
    }

    public bool KnowsMove(MoveBase moveBase) {
        return Moves.Any(item => item.Base == moveBase);
    }

    public void SetStatus(ConditionID conditionId) {
        var status = ConditionsDB.Conditions[conditionId];
        if (!Statuses.Contains(status)) {
            Statuses.Add(status);
            status?.OnStart?.Invoke(this);
            StatusChanges.Enqueue(Base.Name + status.StartMessage);
            OnStatusChanged?.Invoke();
        }
        else {
            status?.OnRefresh?.Invoke(this);
            OnStatusChanged?.Invoke();
        }
    }

    public void CureStatus(ConditionID conditionId) {
        var status = ConditionsDB.Conditions[conditionId];
        if (!Statuses.Contains(status))
            return; 
        StatusChanges.Enqueue(Base.Name + " shook off its " + ConditionsDB.Conditions[conditionId].Name);
        Statuses.Remove(status);
        OnStatusChanged?.Invoke();
    }

    public void CureAllStatus() {
        for (int i = 0; i < Statuses.Count; i++) {
            CureStatus(Statuses[i].Id);
        }
    }

    public bool OnBeforeMove() {
        bool canPerformMove = true;
        /*if (Statuses.Count == 0)
            return canPerformMove;

        for (int i = 0; i < Statuses.Count; i++) {
            var condition = Statuses[i];
            
            if (condition?.OnBeforeMove != null) {
                if (!condition.OnBeforeMove(this)) 
                    canPerformMove = false;
            }
        }
        */
        return canPerformMove;
    }

    public void OnAfterTurn () {
        for (int i = 0; i < Statuses.Count; i++) {
            var condition = Statuses[i];
            if(condition?.OnAfterTurn != null)
                condition?.OnAfterTurn?.Invoke(this);
        }
        OnStatusChanged?.Invoke();
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
    public int Damage { get; set; }
}
