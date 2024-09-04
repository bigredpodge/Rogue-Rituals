using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PartyScreen : MonoBehaviour
{
    
    [SerializeField] TMP_Text messageText;
    [SerializeField] PartyPreviewUI partyPreviewUI;
    PartyMemberUI[] memberSlots;
    private int currentSelection;
    List<Devil> devils;

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        for (int i = 0; i < memberSlots.Length; i++) {
            memberSlots[i].StatusUIHandler.Init();
        }
    }

    public void SetPartyData(List<Devil> devils) {
        this.devils = devils;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < devils.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Setup(devils[i]);
            }
            else 
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Summon new Devil.";
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 1; i < devils.Count; i++) {
            if (i == selectedMember) 
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }

        if (currentSelection != selectedMember) {
            partyPreviewUI.SetData(devils[selectedMember]);
            currentSelection = selectedMember;
        }
    }

    public void SetMessageText(string message) {
        messageText.text = message;
    }


}
