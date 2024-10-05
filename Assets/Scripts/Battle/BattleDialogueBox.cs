using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Unity.VisualScripting;
using DG.Tweening;

public class BattleDialogueBox : MonoBehaviour
{
    public enum DialogueState { CHOOSE, FORGETMOVE, RELEASEDEVIL, MOVEFORGOTTEN, DEVILRELEASED, BUSY }
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector, moveSelector, moveDetails, targetSelector, dialogueBox, playerMenu;
    [SerializeField] RectTransform dialogueTextBox, dialogueBoxBackground;
    [SerializeField] TargetUI[] targetSlots;
    [SerializeField] List<TMP_Text> actionTexts, moveTexts, shopItemTexts, battleItemTexts, battleItemCounts;
    [SerializeField] List<Image> shopItemSprites, battleItemSprites;
    [SerializeField] TMP_Text apText, brandText, typeText, powerText;
    [SerializeField] TargetMenu targetMenu;
    [SerializeField] LearnMoveUI learnMoveUI;
    [SerializeField] TMP_Text[] choiceTexts;
    [SerializeField] GameObject choiceTextUI, moveToForgetUI;
    private DialogueState state = DialogueState.BUSY;
    private MoveBase targetMove;
    private Devil targetDevil;
    private DevilParty targetParty;
    private string fullText;
    private int currentMoveForgetSelection, currentChoiceSelection, currentReleaseSelection;
    private Vector3 playerMenuOriginalPos;
    private float posYOffset = 220f;
    private bool forgetMoveChoice;
    public TargetMenu TargetMenu {
        get { return targetMenu; }
    }
    [SerializeField] float textSpeed;

    public void Awake() {
        playerMenuOriginalPos = playerMenu.transform.localPosition;
        playerMenu.transform.localPosition = new Vector3(playerMenuOriginalPos.x, playerMenuOriginalPos.y - posYOffset);
    }
    
    public void Update() {
        if (state == DialogueState.CHOOSE) {
            HandleChoiceSelection();
        }
        if (state == DialogueState.FORGETMOVE) {
            HandleMoveForgetSelection();
        }
        if (state == DialogueState.RELEASEDEVIL) {
            HandleDevilReleaseSelection();
        }
    }

    public void SetDialogue(string dialogue) {
        dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string dialogue) {
        dialogueText.text = dialogue;
        dialogueText.ForceMeshUpdate();
        Vector2 textSize = dialogueText.GetPreferredValues(dialogue);
        dialogueBoxBackground.sizeDelta = new Vector2(textSize.x, textSize.y);
        dialogueTextBox.sizeDelta = new Vector2(textSize.x, dialogueBoxBackground.sizeDelta.y);
        dialogueText.text = "";
        foreach (var letter in dialogue.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f/textSpeed);
        }
    }

    public void EnableDialogueText(bool enabled) {
        dialogueBox.SetActive(enabled);
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
            if (currentChoiceSelection == 0) {
                if (forgetMoveChoice)
                    StartCoroutine(ForgetMoves(targetDevil));
                else
                    StartCoroutine(ChooseDevilToRelease());
            }
            else {
                if (forgetMoveChoice)
                    StartCoroutine(DontForgetMoves());
                else
                    StartCoroutine(DontReleaseDevil());
            }
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
            if (currentMoveForgetSelection == 0)
                StartCoroutine(DontForgetMoves());
            else
                StartCoroutine(ForgetMove(currentMoveForgetSelection, targetDevil));
        }
    }

    void HandleDevilReleaseSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow))
                ++currentReleaseSelection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --currentReleaseSelection;

        currentReleaseSelection = Mathf.Clamp(currentReleaseSelection, 0, 5);

        targetMenu.UpdateTargetSelection(currentReleaseSelection);
        if(Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(ReleaseDevil());
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine(DontReleaseDevil());
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
                targetMove = newMove;
                targetDevil = target;
                yield return TypeDialogue(targetDevil.Base.Name + " wants to learn "+ targetMove.Name + ".");
                yield return new WaitForSeconds(1f);
                yield return TypeDialogue("But they already know four moves. Forget one?");
                choiceTexts[0].text = "Yes";
                choiceTexts[1].text = "No";
                choiceTextUI.SetActive(true);
                state = DialogueState.CHOOSE;
                forgetMoveChoice = true;

                yield return new WaitUntil(() => state == DialogueState.MOVEFORGOTTEN);
            }
    }

    public IEnumerator TryToAddDevil(Devil devil, DevilParty party) {
        Debug.Log(party.Devils.Count);
        if (party.Devils.Count < 6) {
            party.AddDevil(devil);
            yield return TypeDialogue(devil.Base.Name + " added to your party.");
            yield break;
        }
       
        yield return TypeDialogue("Your party is too full to add " + devil.Base.Name + ". Release a devil?");
        choiceTexts[0].text = "Yes";
        choiceTexts[1].text = "No";
        choiceTextUI.SetActive(true);

        targetDevil = devil;
        targetParty = party;
        state = DialogueState.CHOOSE;
        forgetMoveChoice = false;

        yield return new WaitUntil(() => state == DialogueState.DEVILRELEASED);
    }

    IEnumerator DontReleaseDevil() {
        yield return TypeDialogue("You retain your party members. " + targetDevil.Base.Name + " returned to hell.");
        yield return new WaitForSeconds(1f);
        state = DialogueState.DEVILRELEASED;
    }

    IEnumerator ChooseDevilToRelease() {
        yield return TypeDialogue("Choose a devil to replace with " + targetDevil.Base.Name + ".");
        yield return new WaitForSeconds(1f);
        TargetMenu.SetDevilData(targetParty.Devils, false);
        EnableTargetSelector(true);
        state = DialogueState.RELEASEDEVIL;
    }

    IEnumerator ReleaseDevil() {
        state = DialogueState.BUSY;
        EnableTargetSelector(false);

        yield return TypeDialogue("Goodbye, " + targetParty.Devils[currentReleaseSelection].Base.Name + "!");
        targetParty.ReleaseDevilAt(currentReleaseSelection);
        yield return new WaitForSeconds(1f);

        yield return TypeDialogue("Welcome, " + targetDevil.Base.Name + "!");
        targetParty.AddDevil(targetDevil);
        yield return new WaitForSeconds(1f);

        state = DialogueState.DEVILRELEASED;
    }

    public IEnumerator ForgetMoves(Devil target) {
        state = DialogueState.BUSY;

        yield return TypeDialogue("Choose a move to forget.");
        yield return new WaitForSeconds(1f);
        learnMoveUI.SetMoveData(target.Moves, targetMove);
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
        target.Moves[selection-1] = new Move(targetMove);
        yield return TypeDialogue(target.Base.Name + " forgot " + selectedMove.Name + " and learned " + targetMove.Name + ".");
        state = DialogueState.MOVEFORGOTTEN;
    }

    public IEnumerator DontForgetMoves() {
        state = DialogueState.BUSY;

        EnableDialogueText(true);
        EnableMoveSelector(false);
        moveToForgetUI.SetActive(false);

        yield return TypeDialogue("Did not learn " + targetMove.Name + ".");
        state = DialogueState.MOVEFORGOTTEN;
    }  

    public void PushUpPlayerMenu() {
        playerMenu.transform.DOLocalMoveY(playerMenuOriginalPos.y, 1f);
    }

    public void PushDownPlayerMenu() {
        playerMenu.transform.DOLocalMoveY(playerMenuOriginalPos.y - posYOffset, 1f)
        .OnComplete(() => {
            EnableActionSelector(false);
            EnableMoveSelector(false);
        });

    }   

    Color GetBrandColor(DevilBrand brand) {
        Color[] colors = {  /*Orange*/ new Color(1f, 0.5f, 0f), /*Blue*/ new Color(0f, 0f, 0.75f), /*Green*/ new Color(0f, 0.75f, 0f), /*Yellow*/ new Color(.8f, .8f, 0f), /*Red*/ new Color(0.75f, 0f, 0f), 
                            /*Pink*/ new Color(.8f, 0f, .8f), /*Cyan*/ new Color(0f, 0.8f, 0.8f), /*Purple*/ new Color (0.5f, 0f, 0.5f), /*Gray*/ new Color(0.4f, 0.4f, 0.4f)};
        return colors[(int)brand - 1];
    }

}