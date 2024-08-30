using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleArea : MonoBehaviour
{
    [SerializeField] List<ItemSlot> wildItems;

    


    public ItemSlot GetRandomItem() {
        var wildItem = wildItems[Random.Range(0, wildItems.Count)];
        return wildItem;
    }
}
