using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using JetBrains.Annotations;

public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, RUNNINGTURN, PARTYSCREEN, ITEMSELECTION, BATTLEOVER, BUSY }
public enum BattleAction { MOVE, SWITCHDEVIL, CATCHDEVIL, USEITEM }


public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleDialogueBox dialogueBox;

    [SerializeField] BattleUnit playerUnit, enemyUnit;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] ItemMenu itemMenu;
    [SerializeField] GameObject captureBallPrefab;
    [SerializeField] Inventory inventory;
    private int currentAction, currentMove, currentMemberSelection, currentItemSelection;

    public event Action<bool> OnBattleOver;

    DevilParty playerParty;
    Devil enemyDevil;
    bool isTrainerBattle = false;

    BattleState? state;
    BattleState? prevState;
    // Start is called before the first frame update
    public void StartBattle(DevilParty playerParty, Devil enemyDevil)
    {
        this.enemyDevil = enemyDevil;
        this.playerParty = playerParty;
        state = BattleState.START; 
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {

        playerUnit.Setup(playerParty.GetHealthyDevil());
        enemyUnit.Setup(enemyDevil);

        partyScreen.Init();
        itemMenu.Init();

        dialogueBox.SetMoveNames(playerUnit.Devil.Moves);

        yield return dialogueBox.TypeDialogue("A fearsome " + enemyUnit.Devil.Base.Name + " approaches!");
        yield return new WaitForSeconds(1f);
        
        ActionSelection();
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
                MoveSelection();
            }
            else if (currentAction == 1) {
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 2) {
                if (isTrainerBattle) {
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

        dialogueBox.UpdateBattleItemSelection(currentItemSelection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            itemMenu.gameObject.SetActive(false);
            inventory.UseItem(currentItemSelection, enemyUnit.Devil);
            StartCoroutine(RunTurns(BattleAction.CATCHDEVIL));
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            itemMenu.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    void HandlePartySelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentMemberSelection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
                currentMemberSelection += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentMemberSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
                currentMemberSelection -= 2;

        currentMemberSelection = Mathf.Clamp(currentMemberSelection, 0, playerParty.Devils.Count - 1);

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
                StartCoroutine(SwitchDevil(selectedMember));
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
            if (playerUnit.Devil.Moves[currentMove].PP == 0) return;
            
            StartCoroutine(RunTurns(BattleAction.MOVE));
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            ActionSelection();
        }
    }

    IEnumerator BufferSelection() {
        prevState = state;
        state = BattleState.BUSY;
        yield return new WaitForSeconds(0.05f);
        state = prevState;
    }

    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RUNNINGTURN;
        dialogueBox.EnableMoveSelector(false);
        dialogueBox.EnableDialogueText(true);

        if (playerAction == BattleAction.MOVE) {
            playerUnit.Devil.CurrentMove = playerUnit.Devil.Moves[currentMove];
            enemyUnit.Devil.CurrentMove = enemyUnit.Devil.GetRandomMove();

            int playerMovePriority = playerUnit.Devil.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Devil.CurrentMove.Base.Priority;

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
                yield return SwitchDevil(selectedMember);
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

        if (state != BattleState.BATTLEOVER)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {

        bool canRunMove = sourceUnit.Devil.OnBeforeMove();
        if (!canRunMove) {
            yield return ShowStatusChanges(sourceUnit.Devil);
            yield return sourceUnit.Hud.UpdateHP();
            yield return CheckFelled(sourceUnit);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Devil);

        move.PP--;
        yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name + " uses " + move.Base.Name + "!");
        yield return new WaitForSeconds(1f);

        if (CheckIfMoveHits(move, sourceUnit.Devil, targetUnit.Devil)) {
            if (move.Base.Category != MoveCategory.Status) {
                var damageDetails = targetUnit.Devil.TakeDamage(move, sourceUnit.Devil);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Effects != null && move.Base.Effects.Count > 0 && targetUnit.Devil.HP > 0)
                yield return RunMoveEffects(sourceUnit.Devil, targetUnit.Devil, move);

            yield return CheckFelled(targetUnit);
        }
        else {
            yield return dialogueBox.TypeDialogue(sourceUnit.Devil.Base.Name+" missed!");
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit) {
        if (state == BattleState.BATTLEOVER) yield break;
        yield return new WaitUntil(() => state == BattleState.RUNNINGTURN);

        sourceUnit.Devil.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Devil);
        yield return sourceUnit.Hud.UpdateHP();
        yield return CheckFelled(sourceUnit);
    }

    IEnumerator CheckFelled(BattleUnit felledUnit) {
        if (felledUnit.Devil.HP <= 0) {
            yield return dialogueBox.TypeDialogue(felledUnit.Devil.Base.Name + " has been felled!");
            felledUnit.RemoveUnit();
            yield return new WaitForSeconds(1f);

            StartCoroutine(CheckForBattleOver(felledUnit));
        }
    }

    IEnumerator RunMoveEffects(Devil source, Devil target, Move move) {
        var effects = move.Base.Effects;
        foreach (var effect in effects) {
            var rnd = UnityEngine.Random.Range(1, 101);
            if (rnd > effect.Chance)
                break;

            // Stat Boost
            if (effect.Boosts != null) {
                if (effect.Target == MoveTarget.Self)
                    source.ApplyBoosts(effect.Boosts);
                else
                    target.ApplyBoosts(effect.Boosts);
            }

            // Status Cond
            if (effect.Status != ConditionID.none) {
                if (effect.Target == MoveTarget.Self)
                    source.SetStatus(effect.Status, effect.StatusTime);
                else
                    target.SetStatus(effect.Status, effect.StatusTime);
            }
        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
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

    IEnumerator ShowStatusChanges(Devil devil) {
        while (devil.StatusChanges.Count > 0)
        {
            var message = devil.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
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

    IEnumerator SwitchDevil(Devil newDevil) {

        dialogueBox.EnableActionSelector(false);
        playerUnit.Devil.OnRecall();
        
        if (playerUnit.Devil.HP > 0) {
            yield return dialogueBox.TypeDialogue("Return whence you came.");
            playerUnit.RemoveUnit();
            yield return new WaitForSeconds(1f);
        }
        dialogueBox.SetMoveNames(newDevil.Moves);

        yield return dialogueBox.TypeDialogue("Fight for me, " + newDevil.Base.Name + "!");
        playerUnit.Setup(newDevil);
        yield return new WaitForSeconds(1f);

        state = BattleState.RUNNINGTURN;
    }

    void ActionSelection() {
        state = BattleState.ACTIONSELECTION;
        dialogueBox.EnableDialogueText(true);
        dialogueBox.EnableActionSelector(true);
        dialogueBox.EnableMoveSelector(false);
        itemMenu.gameObject.SetActive(false);
        dialogueBox.SetDialogue("Choose an action.");
    }

    void OpenPartyScreen() {
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
            itemMenu.SetItemData(inventory.RitualItemSlots);
        else
            itemMenu.SetItemData(inventory.OfferingItemSlots);
        
        itemMenu.gameObject.SetActive(true);
        dialogueBox.SetDialogue("Choose an item.");

    }

    void MoveSelection() {
        state = BattleState.MOVESELECTION;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }


    IEnumerator EndBattle(bool won) {
        state = BattleState.BATTLEOVER;
        playerParty.Devils.ForEach(p => p.OnBattleOver());
        if (won) {
            int expYield = enemyUnit.Devil.Base.ExpYield;
            int enemyLevel = enemyUnit.Devil.Level;
            //Is it a trainer enemy? add 1.5x bonus

            int expGain = Mathf.FloorToInt((expYield * enemyLevel) / 7);
            playerUnit.Devil.Exp += expGain;
            //make it the whole party and show this

            yield return dialogueBox.TypeDialogue(playerUnit.Devil.Base.Name+" gained "+expGain+" exp!");
            yield return playerUnit.Hud.SetExpSmooth(false);
            yield return new WaitForSeconds(1f); 
            //Check Levelup
            while (playerUnit.Devil.CheckForLevelUp()) {
                playerUnit.Hud.SetLevel();
                yield return dialogueBox.TypeDialogue(playerUnit.Devil.Base.Name+" grew to level "+playerUnit.Devil.Level+"!");
                yield return playerUnit.Hud.ShowStatGrowth();
                yield return playerUnit.Hud.SetExpSmooth(true);
            }
            yield return new WaitForSeconds (1f);
            playerUnit.RemoveUnit();
            OnBattleOver(true);
        }
        else {
            yield return dialogueBox.TypeDialogue("you lost...");
            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
    }

    IEnumerator TrapRitual(RitualItem ritualItem) {
        state = BattleState.BUSY;
        yield return dialogueBox.TypeDialogue("Initiating Capture Ritual...");
        var captureObj = Instantiate(captureBallPrefab, enemyUnit.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        var ball = captureObj.GetComponent<MeshRenderer>();
        enemyUnit.RemoveUnit();

        int shakeCount = TryToCatch(enemyUnit.Devil, ritualItem); 
        
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++) {
            yield return new WaitForSeconds(1f);
            ball.transform.DOPunchPosition(new Vector3(0, 0, .5f), 0.8f, 20).WaitForCompletion();
        }

        if (shakeCount == 4) {
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
            yield return new WaitForSeconds(1f);
            yield return dialogueBox.TypeDialogue(enemyUnit.Devil.Base.Name + " broke out!");
            yield return new WaitForSeconds(1f);
            
            state = BattleState.RUNNINGTURN; 
        }
    }

    int TryToCatch(Devil devil, RitualItem ritualItem) {
        var statusBonus = 1f;

        foreach (var entry in devil.Statuses) {
            var condition = entry.Key;
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
