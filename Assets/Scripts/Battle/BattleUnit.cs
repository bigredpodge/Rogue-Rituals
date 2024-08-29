using System.Collections;
using System.Collections.Generic;
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

    public Devil Devil { get; set; }

    public void Setup(Devil devil) {
        Devil = devil;
        if (isPlayerUnit) {
            Vector3 relativePos = enemyBattleStation.position - playerBattleStation.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            newInstance = Instantiate(Devil.Base.Model, playerBattleStation.position, rotation);
        }
        else {
            Vector3 relativePos = playerBattleStation.position - enemyBattleStation.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            newInstance = Instantiate(Devil.Base.Model, enemyBattleStation.position, rotation);  
        }

        hud.SetData(devil);

    }

    public void RemoveUnit() {
        Destroy(newInstance);
    }

}
