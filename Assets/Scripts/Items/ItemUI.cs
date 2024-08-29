using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, countText;
    [SerializeField] Image sprite;
    [SerializeField] Color highlightedColor;

    ItemSlot _item;

    public void Setup(ItemSlot item) {
        _item = item;
        nameText.text = item.Item.Name;
        sprite.sprite = item.Item.Sprite;
        countText.text = item.Count+"";
    }
    public void SetSelected(bool selected) {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}
