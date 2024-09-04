using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMenu : MonoBehaviour
{
    TargetUI[] memberSlots;
    List<ItemSlot> items;
    List<Devil> devils;
    private int activeMemberSlots;

    public void Init() {
        memberSlots = GetComponentsInChildren<TargetUI>();
    }

    public void SetItemData(List<ItemSlot> items) {
        this.items = items;
        activeMemberSlots = 0;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < items.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetDataFromItem(items[i]);
                ++activeMemberSlots;
            }
            else 
                memberSlots[i].gameObject.SetActive(false);
        }
    }

    public void SetDevilData(List<Devil> devils) {
        this.devils = devils;
        activeMemberSlots = 0;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < devils.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetDataFromDevil(devils[i]);
                ++activeMemberSlots;
            }
            else 
                memberSlots[i].gameObject.SetActive(false);
        }
    }

    public void UpdateTargetSelection(int selectedMember) {
        for (int i = 0; i < activeMemberSlots; i++) {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }
}
