using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMenu : MonoBehaviour
{
    ItemUI[] memberSlots;
    List<ItemSlot> items;

    public void Init() {
        memberSlots = GetComponentsInChildren<ItemUI>();
    }

    public void SetItemData(List<ItemSlot> items) {
        this.items = items;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < items.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Setup(items[i]);
            }
            else 
                memberSlots[i].gameObject.SetActive(false);
        }
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < items.Count; i++) {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }
}
