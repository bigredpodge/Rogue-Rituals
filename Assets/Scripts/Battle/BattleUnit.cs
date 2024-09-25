using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    public bool IsPlayerUnit {
        get { return isPlayerUnit; }
    }
    public BattleHUD Hud {
        get { return hud; }
    }

    [SerializeField] Transform playerBattleStation, enemyBattleStation;
    public int maxHP;
    public int currentHP;
    private GameObject newInstance;
    private Vector3 originalPos;
    private Vector3 originalScale;

    public Devil Devil { get; set; }

    public void Setup(Devil devil) {
        Devil = devil;
        if (isPlayerUnit) {
            Vector3 relativePos = enemyBattleStation.position - playerBattleStation.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            originalPos = playerBattleStation.position;
            newInstance = Instantiate(Devil.Base.Model, originalPos, rotation);
        }
        else {
            Vector3 relativePos = playerBattleStation.position - enemyBattleStation.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            originalPos = enemyBattleStation.position;
            newInstance = Instantiate(Devil.Base.Model, originalPos, rotation);  
        }
        hud.SetData(devil);

        originalScale = newInstance.transform.localScale;
        newInstance.transform.localScale = new Vector3 (0f, 0f, 0f);
    }
    
    public IEnumerator EnterBattleAnimation() {
        float elapsedTime = 0f;
        float fadeDuration = 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            Vector3 newScale = new Vector3(Mathf.Clamp(originalScale.x * (elapsedTime / fadeDuration), 0f, originalScale.x), Mathf.Clamp(originalScale.y * (elapsedTime / fadeDuration), 0f, originalScale.y), Mathf.Clamp(originalScale.z * (elapsedTime / fadeDuration), 0f, originalScale.z));
            newInstance.transform.localScale = newScale;

            yield return null; 
        }

        newInstance.transform.localScale = originalScale;
    }

    public void RemoveUnit() {
        Destroy(newInstance);
    }

}
