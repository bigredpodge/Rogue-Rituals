using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> ritualItemSlots, offeringItemSlots;

    public event Action OnUpdated;

    public List<ItemSlot> RitualItemSlots { 
        get {
            return ritualItemSlots; 
        }
    }

    public List<ItemSlot> OfferingItemSlots { 
        get {
            return offeringItemSlots; 
        }
    }

    public ItemBase UseItem(int itemIndex, Devil selectedDevil) {
        var item = ritualItemSlots[itemIndex].Item;
        bool itemUsed = item.Use(selectedDevil);
        if(itemUsed) {
            RemoveItem(item);
            return item;
        }
        return null;
    }

    public void StockItem(ItemBase item, int stock) {
        var itemSlot = ritualItemSlots.First(slot => slot.Item == item);
        if (itemSlot == null) {
            ritualItemSlots.Add(
                new ItemSlot {
                    Item = item,
                    Count = stock
                });
            return;
        }
        itemSlot.Count += stock;
    }

    public void RemoveItem(ItemBase item) {
        var itemSlot = ritualItemSlots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0) {
            ritualItemSlots.Remove(itemSlot);
        }
    }
}

[Serializable]
public class ItemSlot {
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item { 
        get => item;
        set => item = value;
    }
    
    public int Count {
        get => count;
        set => count = value;
    }
}

