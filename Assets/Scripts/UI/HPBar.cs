using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [SerializeField] TMP_Text hpText;
    public bool IsUpdating;

    public void SetHP(int currentHP, int maxHP) {
        float hpNormalized = (float) currentHP / maxHP;
        health.transform.localScale = new Vector3(hpNormalized, 1f);
        hpText.text = currentHP + " / " + maxHP;
    }

    public IEnumerator SetHPSmooth(int currentHP, int maxHP) {
        IsUpdating = true;

        float newHp = (float) currentHP / maxHP;
        float curHp = health.transform.localScale.x;
        if (curHp > newHp) {
            float changeAmt = curHp - newHp;
            while (curHp - newHp > Mathf.Epsilon) {
                curHp -= changeAmt * Time.deltaTime;
                health.transform.localScale = new Vector3(curHp, 1f);
                int newHpAmt = Mathf.Clamp(Mathf.FloorToInt(curHp * (float)maxHP), currentHP, maxHP);
                hpText.text = newHpAmt + " / " + maxHP;
                yield return null;
            }
        }
        else {
            float changeAmt = newHp - curHp;
            while (newHp - curHp > Mathf.Epsilon) {
                curHp += changeAmt * Time.deltaTime;
                health.transform.localScale = new Vector3(curHp, 1f);
                int newHpAmt = Mathf.Clamp(Mathf.FloorToInt(curHp * (float)maxHP), 0, currentHP);
                hpText.text = newHpAmt + " / " + maxHP;
                yield return null;
            }
        }
        hpText.text = currentHP + " / " + maxHP;
        health.transform.localScale = new Vector3(newHp, 1f);

        IsUpdating = false;
    }

}
