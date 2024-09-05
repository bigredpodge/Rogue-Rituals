using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;

public class BattleHUD : MonoBehaviour
{
    
    [SerializeField] TMP_Text nameText, levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;
    
    //stat growth UI
    [SerializeField] GameObject statGrowthUI;
    [SerializeField] List<TMP_Text> statGrowthTexts;
    [SerializeField] StatusUIHandler statusUIHandler;

    public StatusUIHandler StatusUIHandler {
        get { return statusUIHandler; }
    }

    Devil _devil;

    public void SetData(Devil devil) {
        if(_devil != null) {
            _devil.OnStatusChanged -= SetStatuses;
            _devil.OnHPChanged -= UpdateHP;
        }

        _devil = devil;
        nameText.text = devil.Base.Name;
    
        hpBar.SetHP(devil.HP, devil.MaxHP);
        SetExp();
        SetLevel();

        statusUIHandler.CheckStatusUI(_devil);
        _devil.OnStatusChanged += SetStatuses;
        _devil.OnHPChanged += UpdateHP;
    }

    public void SetStatuses() {
        statusUIHandler.CheckStatusUI(_devil);
    }

    public void SetLevel() {
        levelText.text = "Lvl " + _devil.Level;
    }

    public void SetExp() {
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f);
    }
    public IEnumerator SetExpSmooth(bool reset=false) {
        if (reset)
            expBar.transform.localScale = new Vector3(0f, 1f, 1f);
        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp() {
        int currLevelExp = _devil.Base.GetExpForLevel(_devil.Level);
        int nextLevelExp = _devil.Base.GetExpForLevel(_devil.Level + 1);
        float normalizedExp = (float)(_devil.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP() {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync() {
        yield return hpBar.SetHPSmooth(_devil.HP, _devil.MaxHP);
    }

    public IEnumerator ShowStatGrowth() {
        var statGrowths = _devil.BoostStatsAfterLevelUp();
        var newStats = _devil.Stats;
        statGrowthUI.SetActive(true);
        for (int i = 0; i < 6; i++) {
            statGrowthTexts[i].text = "+"+statGrowths.ElementAt(i).Value;
            statGrowthTexts[i].color = Color.green;
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 6; i++) {
            statGrowthTexts[i].text = newStats.ElementAt(i).Value+"";
            statGrowthTexts[i].color = Color.black;
        }
        yield return new WaitForSeconds(1f);
        statGrowthUI.SetActive(false);
    }

    public IEnumerator WaitForHPUpdate() {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }       
    
}
