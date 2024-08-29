using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class BattleHUD : MonoBehaviour
{
    
    [SerializeField] TMP_Text nameText, levelText, hpText, statusText;
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
        _devil = devil;
        nameText.text = devil.Base.Name;
        
        hpText.text = devil.HP + " / " + devil.MaxHP;
        hpBar.SetHP((float) devil.HP / devil.MaxHP);
        SetExp();
        SetLevel();

        statusUIHandler.CheckStatusUI(_devil);
        _devil.OnStatusChanged += SetStatuses;
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

    public IEnumerator UpdateHP() {
        if (_devil.HpChanged) {
            yield return hpBar.SetHPSmooth((float) _devil.HP / _devil.MaxHP);
            hpText.text = _devil.HP + " / " + _devil.MaxHP;
            _devil.HpChanged = false;
        }
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
                    
    
}
