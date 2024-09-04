using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, countText;
    [SerializeField] Image sprite;
    [SerializeField] Color highlightedColor;
    public void SetDataFromDevil(Devil devil) {
        sprite.sprite = devil.Base.Sprite;
        nameText.text = devil.Base.Name;
        countText.text = "Lvl"+devil.Level;
    }

    public void SetDataFromItem(ItemSlot item) {
        sprite.sprite = item.Item.Sprite;
        nameText.text = item.Item.Name;
        countText.text = ""+item.Count;
    }

    public void SetSelected(bool selected) {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}
