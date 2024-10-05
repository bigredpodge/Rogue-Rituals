using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, RUNNINGTURN, PARTYSCREEN, ITEMSELECTION, BATTLEOVER, CHOOSETOFORGET, FORGETMOVE, MOVEFORGOTTEN, BUSY }
public enum BattleAction { MOVE, SWITCHDEVIL, CATCHDEVIL, USEITEM }


public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleDialogueBox dialogueBox;

    [SerializeField] BattleUnit playerUnit, enemyUnit;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] GameObject captureBallPrefab;
    [SerializeField] Inventory inventory;
    [SerializeField] CameraManager cameraManager;
    //choice texts for now here, but should be consolidated into battledialoguebox script...
    [SerializeField] GameObject debugUI;
    [SerializeField] TMP_Text weatherText;
    public Weather weather;
    public int weatherCount;
    private int currentAction, currentMove, currentMemberSelection, currentItemSelection;
    private bool debugMode = false;
    public event Action<bool> OnBattleOver;
    DevilParty playerParty, enemyParty;
    Devil enemyDevil;
    bool isSummonerBattle = false;

    private int lastDamage;
    BattleState? state;
    BattleState? prevState;
    // Start is called before the first frame update
    public void StartBattle(DevilParty playerParty, Devil enemyDevil)
    {
        cameraManager.changeCamera("StaticView");
        this.enemyDevil = enemyDevil;
        this.playerParty = playerParty;
        state = BattleState.START; 
        StartCoroutine(SetupBattle());
    }

    public void StartGenericSummonerBattle(DevilParty playerParty, DevilParty summonerParty, GenericSummoner summoner)
    {
        cameraManager.changeCamera("StaticView");
        this.enemyParty = summonerParty;
        this.playerParty = playerParty;
        isSummonerBattle = true;
        state = BattleState.START; 
        StartCoroutine(SetupSummonerBattle(summoner));
    }

    public IEnumerator SetupSummonerBattle(GenericSummoner summoner) {
        SetupUnit(enemyUnit, enemyParty.GetHealthyDevil());

        yield return dialogueBox.TypeDialogue(summoner.Name + " challenges you to battle!");
        yield return new WaitForSeconds(1f);
        StartCoroutine(SetupBattle());
    }


    public IEnumerator SetupBattle() {
        if (!isSummonerBattle) {
            //Wild Battle
            SetupUnit(enemyUnit, enemyDevil);
            yield return dialogueBox.TypeDialogue("Behold, you face " + enemyUnit.Devil.Base.Name + ", " + enemyUnit.Devil.Base.Rank + " of " + enemyUnit.Devil.Base.Domain + "!");
        }
        else {
            //Summoner Battle
            yield return dialogueBox.TypeDialogue("Your opponent summons " + enemyUnit.Devil.Base.Name + ", " + enemyUnit.Devil.Base.Rank + " of " + enemyUnit.Devil.Base.Domain + "!");
        }

        yield return new WaitForSeconds(1f);
        SetupUnit(playerUnit, playerParty.GetHealthyDevil());
        dialogueBox.SetMoveNames(playerUnit.Devil.Moves);
        yield return dialogueBox.TypeDialogue("Go get em, " + playerUnit.Devil.Base.Name + "!");

        partyScreen.Init();
        dialogueBox.TargetMenu.Init();

        Debug.Log(  "Player Stats: Level - "+playerUnit.Devil.Level+" / HP - "+playerUnit.Devil.MaxHP+" / Strength - "+playerUnit.Devil.Strength+" / Discipline - "+playerUnit.Devil.Discipline+
                    " / Fortitude - "+playerUnit.Devil.Fortitude+" / Willpower - "+playerUnit.Devil.Willpower+" / Initiative - "+playerUnit.Devil.Initiative);
        Debug.Log("Enemy Stats: Level - "+enemyUnit.Devil.Level+" / HP - "+enemyUnit.Devil.MaxHP+" / Strength - "+enemyUnit.Devil.Strength+" / Discipline - "+enemyUnit.Devil.Discipline+
                    " / Fortitude - "+enemyUnit.Devil.Fortitude+" / Willpower - "+enemyUnit.Devil.Willpower+" / Initiative - "+enemyUnit.Devil.Initiative);

       
        yield return new WaitForSeconds(1f);
        
        ActionSelection();
    }

    public void SetupUnit(BattleUnit unit, Devil devil) {
        unit.Hud.StatusUIHandler.Init();
        unit.Setup(devil);
        StartCoroutine(unit.EnterBattleAnimation());
    }


    //void BattleOver(bool won) {
    //    state = BattleState.BATTLEOVER;
    //    OnBattleOver(won);
    //}

    private void Update() {
        if (state == BattleState.ACTIONSELECTION) {
            HandleActionSelection();
        }
        if (state == BattleState.MOVESELECTION) {
            HandleMoveSelection();
        }
        if (state == BattleState.PARTYSCREEN) {
            HandlePartySelection();
        }
        if (state == BattleState.ITEMSELECTION) {
            HandleItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.D)) {
            ToggleDebugMode(debugMode);
        }
    }

    void HandleActionSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
                currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentAction;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) 
                currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentAction == 0) {
                currentMove = 0;
                dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Devil.Moves[currentMove]);
                MoveSelection();
            }
            else if (currentAction == 1) {
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 2) {
                if (isSummonerBattle) {
                    StartCoroutine(dialogueBox.TypeDialogue("Your opponent prevents you from stealing its summon!"));
                }
                else
                    OpenInventory(true);
            }
            else if (currentAction == 3) {
                OpenInventory(false);
            }

            StartCoroutine(BufferSelection());
        }
    }

    void HandleItemSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentItemSelection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentItemSelection;

        currentItemSelection = Mathf.Clamp(currentItemSelection, 0, 4);

        dialogueBox.TargetMenu.UpdateTargetSelection(currentItemSelection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            dialogueBox.TargetMenu.gameObject.SetActive(false);
            inventory.UseItem(currentItemSelection, enemyUnit.Devil);
            StartCoroutine(RunTurns(BattleAction.CATCHDEVIL));
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            dialogueBox.TargetMenu.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    void HandlePartySelection() {
        if (Input.GetKeyDown(KeyCode.DownArrow))
                ++currentMemberSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
                --currentMemberSelection;

        currentMemberSelection = Mathf.Clamp(currentMemberSelection, 1, playerParty.Devils.Count - 1);

        partyScreen.UpdateMemberSelection(currentMemberSelection);

        if(Input.GetKeyDown(KeyCode.Z)) {
            var selectedMember = playerParty.Devils[currentMemberSelection];
            if (selectedMember.HP <= 0) {
                partyScreen.SetMessageText("You cannot summon a felled devil.");
                return;
            }
            if (selectedMember == playerUnit.Devil) {
                partyScreen.SetMessageText("You cannot summon your current devil");
                return;
            }
            
            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.PARTYSCREEN) {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SWITCHDEVIL));
            }
            else {
                state = BattleState.BUSY;
                StartCoroutine(SwitchDevil(selectedMember, currentMemberSelection));
            }
            StartCoroutine(BufferSelection());
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    void HandleMoveSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
                currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentMove;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
                currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Devil.Moves.Count - 1);

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Devil.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z)) {
            if (playerUnit.Devil.Moves[currentMove].AP == 0) return;
            
            StartCoroutine(RunTurns(BattleAction.MOVE));
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            ActionSelection();
        }
    }

    IEnumerator BufferSelection() {
        prevState = state;
        state = BattleState.BUSY;
        yield return new WaitForSeconds(0.01f);
        state = prevState;
    }

    IEnumerator RunTurns(BattleAction playerAction) {
        cameraManager.changeCamera("StaticView");
        state = BattleState.RUNNINGTURN;
        dialogueBox.EnableMoveSelector(false);
        dialogueBox.EnableDialogueText(true);

        if (playerAction == BattleAction.MOVE) {
            playerUnit.Devil.CurrentMove = playerUnit.Devil.Moves[currentMove];
            enemyUnit.Devil.CurrentMove = enemyUnit.Devil.GetRandomMove();

            int playerMovePriority = playerUnit.Devil.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Devil.CurrentMove.Base.Priority;

            if (debugMode)
                playerMovePriority = 3;

            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority) {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Devil.Initiative >= enemyUnit.Devil.Initiative;

            Debug.Log("Player Initiative = "+playerUnit.Devil.Initiative+" || Enemy Initiative = "+enemyUnit.Devil.Initiative);

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondDevil = secondUnit.Devil;

            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Devil.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BATTLEOVER) yield break;

            if(secondDevil.HP > 0) {
                //Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Devil.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BATTLEOVER) yield break;
            }
        }
        else {
            if (playerAction == BattleAction.SWITCHDEVIL) {
                var selectedMember = playerParty.Devils[currentMemberSelection];
                state = BattleState.BUSY;
                yield return SwitchDevil(selectedMember, currentMemberSelection);
            }
            else if (playerAction == BattleAction.CATCHDEVIL) {
                dialogueBox.EnableActionSelector(false);
                var item = inventory.RitualItemSlots[currentItemSelection].Item;
                yield return TrapRitual((RitualItem)item);
            }

            if (state == BattleState.BATTLEOVER) yield break;

            //Enemy Turn
            enemyUnit.Devil.CurrentMove = enemyUnit.Devil.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyUnit.Devil.CurrentMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BATTLEOVER) yield break;
        }

        if (state != BattleState.BATTLEOVER) {
            currentAction = 0;
            yield return CheckWeather();
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {
        bool canRunMove = sourceUnit.Devil.OnBeforeMove();
        if (!canRunMove) {
            yield return ShowStatusChanges(sourceUnit);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            sourceUnit.Hud.SetStatuses();
            yield return CheckFelled(sourceUnit);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit);
        move.AP--;

        if (sourceUnit.IsPlayerUnit)
            yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name + " uses " + move.Base.Name + "!");
        else
            yield return dialogueBox.TypeDialogue("The enemy "+sourceUnit.Devil.Base.Name + " uses " + move.Base.Name + "!");

        if (move.Base.Category == MoveCategory.Strength)
            sourceUnit.PlayAttackAnimation();
        else
            sourceUnit.PlaySpellAnimation();

        yield return new WaitForSeconds(1f);

        if (CheckIfMoveHits(move, sourceUnit.Devil, targetUnit.Devil)) {
            if (move.Base.Category != MoveCategory.Status) {
                float modifier = 1f;
                modifier = WeatherDB.GetWeatherBonus(weather, move);
                if (sourceUnit.IsPlayerUnit && debugMode)
                    modifier = 20f;

                var damageDetails = targetUnit.Devil.TakeDamage(move, sourceUnit.Devil, modifier);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
                lastDamage = damageDetails.Damage;

                //Recoil
                if (move.Base.TakeRecoil == TakeRecoil.OnHit)
                    yield return TakeRecoilDamage(sourceUnit, move, damageDetails.Damage);

                //Drain
                foreach (var effect in move.Base.Effects) {
                    if (effect.HealSource == HealSource.Damage) {
                        yield return RunHeal(sourceUnit, effect);
                    }
                }
            
            }

            if (move.Base.Effects != null && move.Base.Effects.Count > 0 && targetUnit.Devil.HP > 0)
                yield return RunMoveEffects(sourceUnit, targetUnit, move);

            yield return CheckFelled(targetUnit);
            yield return CheckFelled(sourceUnit);
        }
        else {
            if (sourceUnit.IsPlayerUnit)
                yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name+" missed!");
            else
                yield return dialogueBox.TypeDialogue("The enemy "+sourceUnit.Devil.Base.Name+" missed!");
            
            if (move.Base.TakeRecoil == TakeRecoil.OnMiss) {
                yield return new WaitForSeconds(1f);
                var simulatedDamage = targetUnit.Devil.CalculateDamage(move, sourceUnit.Devil, 1f);
                yield return TakeRecoilDamage(sourceUnit, move, simulatedDamage.Damage);
            }

            yield return CheckFelled(targetUnit); 

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator TakeRecoilDamage(BattleUnit sourceUnit, Move move, int damage) {
        yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name+" crashed and hurt themselves!");
        sourceUnit.Devil.DamageHP(Mathf.FloorToInt(damage * (move.Base.RecoilMultiplier / 4)));
        yield return new WaitForSeconds(1f);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit) {
        if (state == BattleState.BATTLEOVER) yield break;
        yield return new WaitUntil(() => state == BattleState.RUNNINGTURN);

        sourceUnit.Devil.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit);
        sourceUnit.Hud.SetStatuses();
        yield return sourceUnit.Hud.WaitForHPUpdate();
        yield return CheckFelled(sourceUnit);
    }

    IEnumerator CheckFelled(BattleUnit felledUnit) {
        if (felledUnit.Devil.HP <= 0) {
            yield return dialogueBox.TypeDialogue(felledUnit.Devil.Base.Name + " has been felled!");
            felledUnit.Devil.CureAllStatus();
            yield return felledUnit.RemoveUnit();
            yield return new WaitForSeconds(1f);

            StartCoroutine(CheckForBattleOver(felledUnit));
        }
    }

    IEnumerator NextSummonerDevil(Devil newDevil) {
        yield return dialogueBox.TypeDialogue("A fearsome " + newDevil.Base.Name + " is summoned!");
        enemyUnit.Setup(newDevil);
        yield return enemyUnit.EnterBattleAnimation();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator RunMoveEffects(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {
        var effects = move.Base.Effects;
        foreach (var effect in effects) {
            var rnd = UnityEngine.Random.Range(1, 101);
            if (rnd > effect.Chance) {
                if (move.Base.Category == MoveCategory.Status) {
                    yield return dialogueBox.TypeDialogue("It failed!");
                    yield return new WaitForSeconds(1f);
                }
                break;
            }

            // Stat Boost
            if (effect.Boosts != null) {
                if (effect.Target == MoveTarget.Self)
                    sourceUnit.Devil.ApplyBoosts(effect.Boosts);
                else
                    targetUnit.Devil.ApplyBoosts(effect.Boosts);
            }

            // Status Cond
            if (effect.Status != ConditionID.none) {
                if (effect.Target == MoveTarget.Self) {
                    sourceUnit.Devil.SetStatus(effect.Status);
                }
                else {
                    targetUnit.Devil.SetStatus(effect.Status);
                }
            }

            //Heals
            if (effect.HealSource == HealSource.Discipline)
                yield return RunHeal(sourceUnit, effect);

            //Weather
            if (effect.Weather != WeatherID.none) 
                yield return SetWeather(effect.Weather);

        }
        yield return ShowStatusChanges(sourceUnit);
        yield return ShowStatusChanges(targetUnit);
    }

    IEnumerator RunHeal(BattleUnit sourceUnit, MoveEffects effect) {
        if (effect.HealSource == HealSource.Discipline) {
            float a = (2 * sourceUnit.Devil.Level + 10) / 250f;
            int heal = Mathf.FloorToInt(a * 20 * (sourceUnit.Devil.Discipline / 4) * (effect.HealMultiplier / 3) + 2);
            Debug.Log("Heal amount: "+heal);
            sourceUnit.Devil.HealHP(heal);

            yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name + " healed.");
            yield return new WaitForSeconds(1f);
        }
        else if (effect.HealSource == HealSource.Damage) {
            int heal = Mathf.FloorToInt(lastDamage * (effect.HealMultiplier / 3));
            Debug.Log("Heal amount: "+heal);
            sourceUnit.Devil.HealHP(heal);

            yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name + " drained the opponent's health.");
            yield return new WaitForSeconds(1f);
        }
    }

    bool CheckIfMoveHits(Move move, Devil source, Devil target) {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f};

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else 
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator SetWeather(WeatherID weatherId) {
        var newWeather = WeatherDB.Weathers[weatherId];
        weather = newWeather;
        weatherCount = 5;
        weatherText.text = newWeather.Name;
        yield return dialogueBox.TypeDialogue(newWeather.StartMessage);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CheckWeather() {
        if (weather == null)
            yield break;
        else {
            yield return dialogueBox.TypeDialogue(weather.ContinueMessage);
            weather.OnAfterRound(playerUnit.Devil);
            weather.OnAfterRound(enemyUnit.Devil);
            yield return new WaitForSeconds(1f);
            
            yield return ShowStatusChanges(playerUnit);
            yield return ShowStatusChanges(enemyUnit);

            weatherCount--;
            if (weatherCount <= 0) {
                yield return dialogueBox.TypeDialogue(weather.EndMessage);
                weather = null;
                weatherText.text = "";
                yield return new WaitForSeconds(1f);
            }
        }
    }

    IEnumerator ShowStatusChanges(BattleUnit sourceUnit) {
        while (sourceUnit.Devil.StatusChanges.Count > 0)
        {
            var message = sourceUnit.Devil.StatusChanges.Dequeue();
            if (sourceUnit.IsPlayerUnit)
                yield return dialogueBox.TypeDialogue(message);
            else 
                yield return dialogueBox.TypeDialogue("The enemy "+message);

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator CheckForBattleOver(BattleUnit felledUnit) {
        if (felledUnit.IsPlayerUnit) {
            var nextDevil = playerParty.GetHealthyDevil();
            if (nextDevil != null) {
                OpenPartyScreen();
            }
            else {
                yield return EndBattle(false);
            }
        }
        else if (isSummonerBattle) {
            var nextDevil = enemyParty.GetHealthyDevil();
            if (nextDevil != null) {
                StartCoroutine(NextSummonerDevil(nextDevil));
            }
            else {
                yield return EndBattle(true);
            }
        }
        else {
            yield return EndBattle(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Critical > 1f) {
            yield return dialogueBox.TypeDialogue("A critical hit!");
            yield return new WaitForSeconds(1f);
        }

        if (damageDetails.BrandEffectiveness > 1f) {
            yield return dialogueBox.TypeDialogue ("It's super effective!");
            yield return new WaitForSeconds(1f);
        }
        else if (damageDetails.BrandEffectiveness < 1f)  {
            yield return dialogueBox.TypeDialogue ("It's not very effective.");
            yield return new WaitForSeconds(1f);
        }

    }

    IEnumerator SwitchDevil(Devil newDevil, int currentMemberSelection) {

        dialogueBox.EnableActionSelector(false);
        playerUnit.Devil.OnRecall();
        
        if (playerUnit.Devil.HP > 0) {
            yield return dialogueBox.TypeDialogue("Return whence you came.");
            yield return playerUnit.RemoveUnit();
            yield return new WaitForSeconds(1f);
        }
        dialogueBox.SetMoveNames(newDevil.Moves);

        playerParty.Devils[0] = newDevil;
        playerParty.Devils[currentMemberSelection] = playerUnit.Devil;

        yield return dialogueBox.TypeDialogue("Fight for me, " + newDevil.Base.Name + "!");
        playerUnit.Setup(newDevil);
        yield return playerUnit.EnterBattleAnimation();
        yield return new WaitForSeconds(1f);

        state = BattleState.RUNNINGTURN;
    }

    void ActionSelection() {
        cameraManager.changeCamera("StaticView");
        state = BattleState.ACTIONSELECTION;
        dialogueBox.EnableDialogueText(true);
        dialogueBox.EnableActionSelector(true);
        dialogueBox.EnableMoveSelector(false);
        dialogueBox.EnableTargetSelector(false);
        dialogueBox.SetDialogue("Choose an action.");
    }

    void OpenPartyScreen() {
        currentMemberSelection = 1;
        state = BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.Devils);
        partyScreen.gameObject.SetActive(true);
    }

    void OpenInventory(bool isRituals) { 
        if (inventory.RitualItemSlots.Count == 0) {
            dialogueBox.SetDialogue("Inventory is empty!");
            return;
        }

        state = BattleState.ITEMSELECTION;
        if (isRituals)
            dialogueBox.TargetMenu.SetItemData(inventory.RitualItemSlots);
        else
            dialogueBox.TargetMenu.SetItemData(inventory.OfferingItemSlots);
        
        dialogueBox.EnableTargetSelector(true);
        dialogueBox.SetDialogue("Choose an item.");

    }

    void MoveSelection() {
        cameraManager.changeCamera("PlayerPan");
        cameraManager.focusCamera(playerUnit.newInstance.transform);
        state = BattleState.MOVESELECTION;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    void ToggleDebugMode(bool enabled) {
        debugMode = !debugMode;
        debugUI.SetActive(debugMode);
    }


    IEnumerator EndBattle(bool won) {
        state = BattleState.BATTLEOVER;
        playerParty.Devils.ForEach(p => p.OnBattleOver());
        if (won) {
            int expYield = enemyUnit.Devil.Base.ExpYield;
            int enemyLevel = enemyUnit.Devil.Level;

            //todo: Is it a trainer enemy? add 1.5x exp bonus.
            float expModifier = 1f;
            if (debugMode)
                expModifier = 25f;

            int expGain = Mathf.FloorToInt(((expYield * enemyLevel) / 7) * expModifier);

            yield return dialogueBox.TypeDialogue("Your party gained exp!");
            yield return GiveAllExp(expGain);
            
            
            yield return playerUnit.RemoveUnit();
            OnBattleOver(true);
        }
        else {
            yield return dialogueBox.TypeDialogue("you lost...");
            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
    }

    IEnumerator GiveAllExp(int expGain) {
        dialogueBox.TargetMenu.SetDevilData(playerParty.Devils, true);
        dialogueBox.TargetMenu.gameObject.SetActive(true);

        List<bool> expGivenFlags = new List<bool>();

        for (int i = 0; i < playerParty.Devils.Count; i++) {
            expGivenFlags.Add(false);
        }

        for (int i = 0; i < playerParty.Devils.Count; i++) {
            if (i==0)
                playerParty.Devils[i].Exp += expGain;
            else
                playerParty.Devils[i].Exp += (expGain /2);

            //todo - add participated in battle flags to increase exp gain

            StartCoroutine(GiveExp(i, expGivenFlags)); 
        }

        yield return new WaitUntil(() => expGivenFlags.All(flag => flag));
        yield return new WaitForSeconds(1f);
        dialogueBox.TargetMenu.gameObject.SetActive(false);
        yield return CheckNewMoves();
    }

    IEnumerator GiveExp(int index, List<bool> expGivenFlags) {
        yield return dialogueBox.TargetMenu.UpdateExp(index, false);
        yield return new WaitForSeconds(0.5f);
        //Check Levelup
        while (playerParty.Devils[index].CheckForLevelUp()) {
            dialogueBox.TargetMenu.UpdateLevel(index);
            var newMoves = playerParty.Devils[index].GetLearnableMovesAtCurrentLevel();
                if (newMoves != null) {
                    foreach (MoveBase move in newMoves)
                        playerParty.Devils[index].QueueMoveToLearn(move);
                }

            yield return new WaitForSeconds(0.3f);
            yield return dialogueBox.TargetMenu.UpdateExp(index, true);
        }

        expGivenFlags[index] = true;
    }

    IEnumerator CheckNewMoves() {
        for (int i = 0; i < playerParty.Devils.Count; i++) {
            if (playerParty.Devils[i].MovesToLearn == null)
                break;

            yield return dialogueBox.LearnNewMoves(playerParty.Devils[i].MovesToLearn, playerParty.Devils[i]);
            playerParty.Devils[i].ClearMoveToLearnQueue();
        }

        yield return new WaitForSeconds(1f);
    }     

    IEnumerator TrapRitual(RitualItem ritualItem) {
        state = BattleState.BUSY;
        yield return dialogueBox.TypeDialogue("Initiating Capture Ritual...");
        var captureObj = Instantiate(captureBallPrefab, enemyUnit.transform.position + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
        cameraManager.changeCamera("EnemyFocus");
        cameraManager.focusCamera(captureObj.transform);
        var ball = captureObj.GetComponent<MeshRenderer>();
        yield return enemyUnit.RemoveUnit();

        int shakeCount = TryToCatch(enemyUnit.Devil, ritualItem); 
        
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++) {
            yield return new WaitForSeconds(1f);
            ball.transform.DOPunchPosition(new Vector3(0, 0, .5f), 0.8f, 20).WaitForCompletion();
        }

        if (shakeCount == 4) {
            yield return new WaitForSeconds(1f);
            yield return dialogueBox.TypeDialogue(enemyUnit.Devil.Base.Name + " was caught!");

            yield return new WaitForSeconds(1f);

            playerParty.AddDevil(enemyUnit.Devil);
            yield return dialogueBox.TypeDialogue(enemyUnit.Devil.Base.Name + " added to your party.");

            Destroy(ball);

            yield return new WaitForSeconds(1f);
            StartCoroutine(EndBattle(true));
        }
        else {
            yield return new WaitForSeconds(1f);
            Destroy(ball);
            enemyUnit.Setup(enemyDevil);
            yield return enemyUnit.EnterBattleAnimation();
            cameraManager.changeCamera("StaticView");
            yield return new WaitForSeconds(1f);
            yield return dialogueBox.TypeDialogue(enemyUnit.Devil.Base.Name + " broke out!");
            yield return new WaitForSeconds(1f);
            
            state = BattleState.RUNNINGTURN; 
        }
    }

    int TryToCatch(Devil devil, RitualItem ritualItem) {
        var statusBonus = 1f;

        foreach (var condition in devil.Statuses) {
            if (ConditionsDB.GetStatusBonus(condition) > statusBonus)
                statusBonus = ConditionsDB.GetStatusBonus(condition);
        }

        float a = (3 * devil.MaxHP - 2 * devil.HP) * devil.Base.CatchRate * ritualItem.CatchModifier * statusBonus / (3 * devil.MaxHP);
        
        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680/a));

        int shakeCount = 0;
        while (shakeCount < 4) {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

}
