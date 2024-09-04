using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;
    [SerializeField] StatusUIHandler statusUIHandler;
    [SerializeField] Color highlightedColor;
    Devil _devil;

    public StatusUIHandler StatusUIHandler {
        get { return statusUIHandler; }
    }

    public void Setup(Devil devil) {
        _devil = devil;
        nameText.text = devil.Base.Name;
        levelText.text = "Lvl " + devil.Level;
        hpBar.SetHP(devil.HP, devil.MaxHP);

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f);

        StatusUIHandler.CheckStatusUI(devil);
    }

    float GetNormalizedExp() {
        int currLevelExp = _devil.Base.GetExpForLevel(_devil.Level);
        int nextLevelExp = _devil.Base.GetExpForLevel(_devil.Level + 1);
        float normalizedExp = (float)(_devil.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void SetSelected(bool selected) {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }

}
