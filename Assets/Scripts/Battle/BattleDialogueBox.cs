using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Unity.VisualScripting;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector, moveSelector, moveDetails, targetSelector;
    [SerializeField] TargetUI[] targetSlots;
    [SerializeField] List<TMP_Text> actionTexts, moveTexts, shopItemTexts, battleItemTexts, battleItemCounts;
    [SerializeField] List<Image> shopItemSprites, battleItemSprites;
    [SerializeField] TMP_Text apText, brandText, typeText, powerText;
    [SerializeField] TargetMenu targetMenu;
    public TargetMenu TargetMenu {
        get { return targetMenu; }
    }
    [SerializeField] float textSpeed;


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


    Color GetBrandColor(DevilBrand brand) {
        Color[] colors = {  /*Orange*/ new Color(1f, 0.5f, 0f), /*Blue*/ new Color(0f, 0f, 0.75f), /*Yellow*/ new Color(.8f, .8f, 0f), /*Green*/ new Color(0f, 0.75f, 0f), /*Red*/ new Color(0.75f, 0f, 0f), 
                            /*Pink*/ new Color(.8f, 0f, .8f), /*Cyan*/ new Color(0f, 0.8f, 0.8f), /*Purple*/ new Color (0.5f, 0f, 0.5f), /*Gray*/ new Color(0.4f, 0.4f, 0.4f)};
        return colors[(int)brand - 1];
    }

}