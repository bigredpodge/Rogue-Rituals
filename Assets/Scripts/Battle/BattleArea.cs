using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleArea : MonoBehaviour
{
    [SerializeField] List<DevilBase> wildDevilBases;
    [SerializeField] List<ItemSlot> wildItems;

    public Devil GetRandomWildDevil(int difficulty) {
        var dBase = wildDevilBases[Random.Range(0, wildDevilBases.Count)];
        var dLevel = Mathf.FloorToInt(difficulty / 3 + 6 * Random.Range(0.75f, 1.25f));
        var wildDevil = new Devil(dBase, dLevel);
        return wildDevil;
    }


    public ItemSlot GetRandomItem() {
        var wildItem = wildItems[Random.Range(0, wildItems.Count)];
        return wildItem;
    }
}
