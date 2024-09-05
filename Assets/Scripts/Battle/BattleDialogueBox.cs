using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Unity.VisualScripting;

public class BattleDialogueBox : MonoBehaviour
{
    public enum DialogueState { CHOOSETOFORGET, FORGETMOVE, MOVEFORGOTTEN, BUSY }
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector, moveSelector, moveDetails, targetSelector;
    [SerializeField] TargetUI[] targetSlots;
    [SerializeField] List<TMP_Text> actionTexts, moveTexts, shopItemTexts, battleItemTexts, battleItemCounts;
    [SerializeField] List<Image> shopItemSprites, battleItemSprites;
    [SerializeField] TMP_Text apText, brandText, typeText, powerText;
    [SerializeField] TargetMenu targetMenu;
    [SerializeField] LearnMoveUI learnMoveUI;
    [SerializeField] TMP_Text[] choiceTexts;
    [SerializeField] GameObject choiceTextUI, moveToForgetUI;
    private DialogueState state = DialogueState.BUSY;
    private MoveBase moveToLearn;
    private Devil devilToTeach;
    private int currentMoveForgetSelection, currentChoiceSelection;
    public TargetMenu TargetMenu {
        get { return targetMenu; }
    }
    [SerializeField] float textSpeed;

    public void Update() {
        if (state == DialogueState.CHOOSETOFORGET) {
            HandleChoiceSelection();
        }
        if (state == DialogueState.FORGETMOVE) {
            HandleMoveForgetSelection();
        }
    }

    public void SetDialogue(string dialogue) {
        dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string dialogue) {
        dialogueText.text = "";
        foreach (var letter in dialogue.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f/textSpeed);
        }
    }

    public void EnableDialogueText(bool enabled) {
        dialogueText.enabled = enabled;
    }
    public void EnableActionSelector(bool enabled) {
        actionSelector.SetActive(enabled);
    } 
    public void EnableMoveSelector(bool enabled) {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableTargetSelector(bool enabled) {
        targetMenu.gameObject.SetActive(enabled);
    }
    
    public void UpdateActionSelection(int selectedAction) {
        for (int i=0; i<actionTexts.Count; i++) {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else 
                actionTexts[i].color = Color.black;
        }
    }

    public void SetMoveNames(List<Move> moves) {
        for (int i=0; i<moveTexts.Count; i++) {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else 
                moveTexts[i].text = "-";
        }
    }
    

    public void UpdateMoveSelection(int selectedMove, Move move) {
        for (int i=0; i<moveTexts.Count; i++) {
            if (i == selectedMove)
                moveTexts[i].color = highlightedColor;
            else 
                moveTexts[i].color = Color.black;
        }

        powerText.text = "Power "+move.Base.Power;
        typeText.text = move.Base.Category+"";

        apText.text = "AP " + move.AP + " / " + move.Base.AP;

        if (move.AP == 0)
            apText.color = new Color(0.75f, 0f, 0f);
        else if (move.AP <= move.Base.AP*(1f/4f))
            apText.color = new Color(1f, 0.5f, 0f);
        else
            apText.color = Color.black;

        brandText.text = move.Base.Brand.ToString();
        brandText.color = GetBrandColor(move.Base.Brand);
    }

    public void UpdateChoiceSelection() {
        for (int i=0; i<choiceTexts.Length; i++) {
            if (i == currentChoiceSelection)
                choiceTexts[i].color = Color.blue;
            else 
                choiceTexts[i].color = Color.black;
        }
    }

    void HandleChoiceSelection() {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentChoiceSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentChoiceSelection;

        UpdateChoiceSelection();
        if (Input.GetKeyDown(KeyCode.Z)) {
            choiceTextUI.SetActive(false);
                if(currentChoiceSelection == 0)
                    StartCoroutine(ForgetMoves(devilToTeach));
                else
                    StartCoroutine(DontForgetMoves());
        }
    }

    void HandleMoveForgetSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentMoveForgetSelection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
                currentMoveForgetSelection += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentMoveForgetSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
                currentMoveForgetSelection -= 2;

        currentMoveForgetSelection = Mathf.Clamp(currentMoveForgetSelection, 0, DevilBase.MaxNumOfMoves);

        learnMoveUI.UpdateMoveSelection(currentMoveForgetSelection);

        if(Input.GetKeyDown(KeyCode.Z)) {
            if (currentMoveForgetSelection == 0) {
                StartCoroutine(DontForgetMoves());
            }
            else {
                StartCoroutine(ForgetMove(currentMoveForgetSelection, devilToTeach));
            }
        }
    }

    public IEnumerator LearnNewMoves(List<MoveBase> newMoves, Devil target) {
        for (int i = 0; i < newMoves.Count; i++) {
            yield return LearnMove(newMoves[i], target);
            yield return new WaitForSeconds(1f);
        }
    }
    public IEnumerator LearnMove(MoveBase newMove, Devil target) {
            if (target.Moves.Count < DevilBase.MaxNumOfMoves) {
                target.LearnMove(newMove);
                yield return TypeDialogue(target.Base.Name + " learned " + newMove.Name + "!");
                SetMoveNames(target.Moves);

            }
            else {
                moveToLearn = newMove;
                devilToTeach = target;
                yield return TypeDialogue(devilToTeach.Base.Name + " wants to learn "+ moveToLearn.Name + ".");
                yield return new WaitForSeconds(1f);
                yield return TypeDialogue("But they already know four moves. Forget one?");
                choiceTexts[0].text = "Yes";
                choiceTexts[1].text = "No";
                choiceTextUI.SetActive(true);
                state = DialogueState.CHOOSETOFORGET;

                yield return new WaitUntil(() => state == DialogueState.MOVEFORGOTTEN);
            }
    }

    public IEnumerator ForgetMoves(Devil target) {
        state = DialogueState.BUSY;

        yield return TypeDialogue("Choose a move to forget.");
        yield return new WaitForSeconds(1f);
        learnMoveUI.SetMoveData(target.Moves, moveToLearn);
        EnableDialogueText(false);
        EnableMoveSelector(true);
        moveToForgetUI.SetActive(true);
        state = DialogueState.FORGETMOVE;
    }

    public IEnumerator ForgetMove(int selection, Devil target) {
        state = DialogueState.BUSY;

        EnableDialogueText(true);
        EnableMoveSelector(false);
        moveToForgetUI.SetActive(false);

        var selectedMove = target.Moves[selection-1].Base;
        target.Moves[selection-1] = new Move(moveToLearn);
        yield return TypeDialogue(target.Base.Name + " forgot " + selectedMove.Name + " and learned " + moveToLearn.Name + ".");
        state = DialogueState.MOVEFORGOTTEN;
    }

    public IEnumerator DontForgetMoves() {
        state = DialogueState.BUSY;

        EnableDialogueText(true);
        EnableMoveSelector(false);
        moveToForgetUI.SetActive(false);

        yield return TypeDialogue("Did not learn " + moveToLearn.Name + ".");
        state = DialogueState.MOVEFORGOTTEN;
    }  


    Color GetBrandColor(DevilBrand brand) {
        Color[] colors = {  /*Orange*/ new Color(1f, 0.5f, 0f), /*Blue*/ new Color(0f, 0f, 0.75f), /*Green*/ new Color(0f, 0.75f, 0f), /*Yellow*/ new Color(.8f, .8f, 0f), /*Red*/ new Color(0.75f, 0f, 0f), 
                            /*Pink*/ new Color(.8f, 0f, .8f), /*Cyan*/ new Color(0f, 0.8f, 0.8f), /*Purple*/ new Color (0.5f, 0f, 0.5f), /*Gray*/ new Color(0.4f, 0.4f, 0.4f)};
        return colors[(int)brand - 1];
    }

}