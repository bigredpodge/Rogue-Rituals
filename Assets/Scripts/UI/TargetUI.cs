using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TargetUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, countText, lvlUpText;
    [SerializeField] GameObject expBar;
    [SerializeField] Image sprite, background;
    [SerializeField] Color highlightedColor;
    Devil _devil;
    public void SetDataFromDevil(Devil devil, bool expBarActive) {
        _devil = devil;

        if (expBarActive) {
            expBar.SetActive(true);
            SetExp();
        }
        else
            expBar.SetActive(false);

        sprite.sprite = devil.Base.Sprite;
        nameText.text = devil.Base.Name;
        countText.text = "Lvl"+devil.Level;
    }

    public void SetDataFromItem(ItemSlot item) {
        sprite.sprite = item.Item.Sprite;
        nameText.text = item.Item.Name;
        countText.text = ""+item.Count;
    }

    public void SetExp() {
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f);
    }

    public void SetLevel() {
        countText.text = "Lvl"+_devil.Level;
        //Level Up Text
    }

    public IEnumerator SetExpSmooth(bool reset=false) {
        if (reset)
            expBar.transform.localScale = new Vector3(0f, 1f, 1f);
        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, .5f).WaitForCompletion();
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
    public void SetBackground(bool canLearn) {
        if (!canLearn)
            background.color = Color.red;
        else
            background.color = Color.gray;
    }
}
