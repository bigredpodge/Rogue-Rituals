using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum GameState { START, BATTLE, SHOP, LOCALECHOICE, BUSY }

public class GameController : MonoBehaviour
{
   
    GameState state;
    [Header("System References")]
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] ShopSystem shopSystem;
    [SerializeField] TMP_Text difficultyText;
    [SerializeField] BattleDialogueBox dialogueBox;

    [Header("Locale References")]
    [SerializeField] int localeLength;
    private Locale currentLocale;
    [SerializeField] public List<Locale> locales;
    private List<Locale> newLocales;
    [SerializeField] GameObject localeChoiceUI;
    [SerializeField] TMP_Text[] localeTexts;
    private int difficultyModifier;
    private int currentSelection;
    public Locale GetRandomNewLocale() {
        return locales[(int)currentLocale.NextAvailableLocales[Random.Range(0, currentLocale.NextAvailableLocales.Count)]];
    }

    private void Awake() {
        ConditionsDB.Init();
    }
    void Start()
    {
        state = GameState.START;
        battleSystem.OnBattleOver += EndBattle;
        shopSystem.OnShopOver += EndShopVoid;
        currentLocale = locales[0];
        newLocales = new List<Locale>();
        LocaleID[] idValues = (LocaleID[])System.Enum.GetValues(typeof(LocaleID));
        for (int i = 0; i < locales.Count; i++) {
            locales[i].LocaleID = idValues[i];
        }
        StartBattle();
    }
    void StartBattle() {
        difficultyModifier++;
        difficultyText.text = difficultyModifier+"";
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
        state = GameState.LOCALECHOICE;
        
        battleSystem.gameObject.SetActive(false);
        shopSystem.gameObject.SetActive(true);
        shopSystem.StartShop();


    }
    
    void Update()
    {
        if (state == GameState.LOCALECHOICE) {
            HandleSelection();
        }
    }

    void HandleSelection() {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (currentSelection < 1) {
                ++currentSelection;
                UpdateSelection();
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (currentSelection > 0) {
                --currentSelection;
                UpdateSelection();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(SelectLocale(currentSelection));
        }

    }

    void EndShopVoid() {
        StartCoroutine(EndShop());
    }

    IEnumerator EndShop() {
        shopSystem.gameObject.SetActive(false);

        if (difficultyModifier % localeLength == 0) {
            //get two different locale choices at random from available selections
            newLocales.Clear();
            newLocales.Add(GetRandomNewLocale());
            newLocales.Add(GetRandomNewLocale());
            while (newLocales[0] == newLocales[1]) {
                newLocales.Remove(newLocales[1]);
                newLocales.Add(GetRandomNewLocale());
            }
            
            //present choices
            for (int i = 0; i < 2; i++) 
                localeTexts[i].text = newLocales[i].LocaleID.ToString();

            localeChoiceUI.SetActive(true);
            yield return dialogueBox.TypeDialogue("The park forks in two. Which road?");
            

            //unlock player choice
            state = GameState.LOCALECHOICE;
            yield break;
        }

        StartBattle();
    }

    public void UpdateSelection() {
        for (int i=0; i<localeTexts.Length; i++) {
            if (i == currentSelection)
                localeTexts[i].color = Color.blue;
            else 
                localeTexts[i].color = Color.black;
        }
    }

    IEnumerator SelectLocale(int selection) {
        state = GameState.BUSY;
        localeChoiceUI.SetActive(false);
        currentLocale = newLocales[selection];
        yield return dialogueBox.TypeDialogue("Taking the road to the "+currentLocale.LocaleID.ToString())+".";
        yield return new WaitForSeconds(1f);

        StartBattle();
    }

}
