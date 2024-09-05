using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyPreviewUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, strengthText, disciplineText, fortitudeText, willpowerText, initiativeText;
    [SerializeField] Transform ModelPreviewTransform;
    [SerializeField] TMP_Text[] moveTexts;
    private GameObject thisInstance;

    public void SetData(Devil devil) {
        if (thisInstance != devil.Base.Model) {
            Destroy(thisInstance);
            
            thisInstance = Instantiate(devil.Base.Model, ModelPreviewTransform.position, ModelPreviewTransform.rotation);
        }

        nameText.text = "Lvl " + devil.Level + " " + devil.Base.Name + " - " + devil.Base.Rank + " of " + devil.Base.Domain;
        strengthText.text = "Strength: "+devil.Strength;
        disciplineText.text = "Discipline: "+devil.Discipline;
        fortitudeText.text = "Fortitude: "+devil.Fortitude;
        willpowerText.text = "Willpower: "+devil.Willpower;
        initiativeText.text = "Initiative: "+devil.Initiative;

        for (int i = 0; i < moveTexts.Length; i++) {
            if (i < devil.Moves.Count) {
                moveTexts[i].text = devil.Moves[i].Base.Name;
                moveTexts[i].color = GetBrandColor(devil.Moves[i].Base.Brand);
            }
            else
                moveTexts[i].text = "-";
        } 
    } 

    Color GetBrandColor(DevilBrand brand) {
        Color[] colors = {  /*Orange*/ new Color(1f, 0.5f, 0f), /*Blue*/ new Color(0f, 0f, 0.75f), /*Green*/ new Color(0f, 0.75f, 0f), /*Yellow*/ new Color(.8f, .8f, 0f), /*Red*/ new Color(0.75f, 0f, 0f), 
                            /*Pink*/ new Color(.8f, 0f, .8f), /*Cyan*/ new Color(0f, 0.8f, 0.8f), /*Purple*/ new Color (0.5f, 0f, 0.5f), /*Gray*/ new Color(0.4f, 0.4f, 0.4f)};
        return colors[(int)brand - 1];
    }
}
