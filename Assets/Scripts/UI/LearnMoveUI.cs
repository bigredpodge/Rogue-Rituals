using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LearnMoveUI : MonoBehaviour
{
    [SerializeField] List<TMP_Text> moveTexts;
    [SerializeField] TMP_Text apText, brandText, typeText, powerText;
    [SerializeField] List<Move> availableMoves = new List<Move>();
    public void SetMoveData(List<Move> currentMoves, MoveBase newMove) {
        availableMoves.Clear();
        moveTexts[0].text = newMove.Name;
        availableMoves.Add(new Move(newMove));

        for (int i = 1; i < currentMoves.Count+1; i++) {
            moveTexts[i].text = currentMoves[i-1].Base.Name;
            availableMoves.Add(currentMoves[i-1]);
        }
    }

    public void UpdateMoveSelection(int selectedMove) {
        for (int i=0; i<moveTexts.Count; i++) {
            if (i == selectedMove)
                moveTexts[i].color = Color.blue;
            else 
                moveTexts[i].color = Color.black;
        }

        Move move = availableMoves[selectedMove];

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
