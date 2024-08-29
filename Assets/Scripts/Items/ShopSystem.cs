using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { START, SELECT, END }

public class ShopSystem : MonoBehaviour
{
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] GameObject itemsUI;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] ItemMenu shopMenu;
    [SerializeField] Inventory inventory;

    ShopState state;
    List<ItemSlot> items = new List<ItemSlot>();
    private int currentSelection;
    public event Action OnShopOver;

    void Start() {
        shopMenu.Init();
    }

    public void StartShop()
    {
        state = ShopState.START;
        GenerateItems();
        StartCoroutine(SetupShop());
    }

    IEnumerator SetupShop() {
        yield return dialogueBox.TypeDialogue("A friendly kobold has wares for sale...");
        yield return new WaitForSeconds(1f);
        shopMenu.SetItemData(items);
        PlayerChoice();
    }

    void GenerateItems() {
        items.Clear();
        for (int i=0; i<3; i++) {
            var newItem = FindObjectOfType<BattleArea>().GetComponent<BattleArea>().GetRandomItem();
            items.Add(newItem);
        }
    }

    void PlayerChoice() {
        itemsUI.SetActive(true);
        state = ShopState.SELECT;
    }

    void Update()
    {
        if (state == ShopState.SELECT) {
            HandleSelection();
        }
    }

    void HandleSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (currentSelection < 2) {
                ++currentSelection;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (currentSelection > 0) {
                --currentSelection;
            }
        }

        dialogueBox.UpdateShopItemSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            inventory.StockItem(items[currentSelection].Item, items[currentSelection].Count);
            StartCoroutine(EndSelection());
        }
    }

    IEnumerator EndSelection() {
        itemsUI.SetActive(false);
        yield return dialogueBox.TypeDialogue("The kobold thanks you for your custom.");
        yield return new WaitForSeconds(1f);
        OnShopOver();
    }
}
