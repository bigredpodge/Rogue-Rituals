using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum GameState { START, BATTLE, SHOP }

public class GameController : MonoBehaviour
{
   
    GameState state;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] ShopSystem shopSystem;
    [SerializeField] TMP_Text difficultyText;
    private int difficultyModifier;

    private void Awake() {
        ConditionsDB.Init();
    }
    void Start()
    {
        state = GameState.START;
        battleSystem.OnBattleOver += EndBattle;
        shopSystem.OnShopOver += StartBattle;
        StartBattle();
    }
    void StartBattle() {
        state = GameState.BATTLE;
        battleSystem.gameObject.SetActive(true);
        shopSystem.gameObject.SetActive(false);

        var playerParty = FindObjectOfType<BattleArea>().GetComponent<DevilParty>();
        var wildDevil = FindObjectOfType<BattleArea>().GetComponent<BattleArea>().GetRandomWildDevil(difficultyModifier);

        var wildDevilCopy = new Devil(wildDevil.Base, wildDevil.Level);
        battleSystem.StartBattle(playerParty, wildDevilCopy);
    }

    void EndBattle(bool won) {
        state = GameState.SHOP;
        difficultyModifier++;
        difficultyText.text = difficultyModifier+"";
        battleSystem.gameObject.SetActive(false);
        shopSystem.gameObject.SetActive(true);
        shopSystem.StartShop();
    }

}
