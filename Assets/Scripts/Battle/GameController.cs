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
    private Locale currentLocale;
    [SerializeField] public List<Locale> locales;
    private int difficultyModifier;
    public void GetRandomNewLocale() {
        currentLocale = locales[(int)currentLocale.NextAvailableLocales[Random.Range(0, currentLocale.NextAvailableLocales.Count)]];
    }

    private void Awake() {
        ConditionsDB.Init();
    }
    void Start()
    {
        state = GameState.START;
        battleSystem.OnBattleOver += EndBattle;
        shopSystem.OnShopOver += StartBattle;
        currentLocale = locales[0];
        LocaleID[] idValues = (LocaleID[])System.Enum.GetValues(typeof(LocaleID));
        for (int i = 0; i < locales.Count; i++) {
            locales[i].localeID = idValues[i];
        }
        StartBattle();
    }
    void StartBattle() {
        state = GameState.BATTLE;
        battleSystem.gameObject.SetActive(true);
        shopSystem.gameObject.SetActive(false);

        //establish units
        var playerParty = GetComponent<DevilParty>();
        var wildDevil = currentLocale.GetRandomWildDevil(currentLocale.CommonDevilBases, difficultyModifier);

        //Try chance for uncommon or rare wild encounter
        int randomNumber = Random.Range(1, 101);
        Debug.Log("Random encounter number = " + randomNumber);
        if (randomNumber < 6 && currentLocale.RareDevilBases.Count > 0) {
            wildDevil = currentLocale.GetRandomWildDevil(currentLocale.RareDevilBases, difficultyModifier);
        }
        else if (randomNumber < 21 && currentLocale.UncommonDevilBases.Count > 0) {
            wildDevil = currentLocale.GetRandomWildDevil(currentLocale.UncommonDevilBases, difficultyModifier);
        }

        //Copy encounter to allow capture
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

        if (difficultyModifier == 5) {
            GetRandomNewLocale();
        }
    }

}
