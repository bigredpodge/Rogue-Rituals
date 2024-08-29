using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, levelText, hpText;
    [SerializeField] Slider hpSlider;
    [SerializeField] Color highlightedColor;

    Devil _devil;

    public void Setup(Devil devil) {
        _devil = devil;
        nameText.text = devil.Base.Name;
        levelText.text = "Lvl " + devil.Level;
        hpText.text = devil.HP + " / " + devil.MaxHP;
        hpSlider.maxValue = devil.MaxHP;
        hpSlider.value = devil.HP;
    }
    public void SetSelected(bool selected) {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }

}
