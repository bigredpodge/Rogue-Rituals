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
    [SerializeField] List<ItemSlot> wildItems;
    List<ItemSlot> items = new List<ItemSlot>();
    private int currentSelection;
    public event Action OnShopOver;

    void Start() {
        shopMenu.Init();
    }

    public void StartShop()
    {
        state = ShopState.START;
        GenerateItems(3);
        StartCoroutine(SetupShop());
    }

    IEnumerator SetupShop() {
        yield return dialogueBox.TypeDialogue("A friendly kobold has wares for sale...");
        yield return new WaitForSeconds(1f);
        shopMenu.SetItemData(items);
        PlayerChoice();
    }

    void GenerateItems(int numItems) {
        items.Clear();
        for (int i=0; i<numItems; i++) {
            var newItem = GetRandomItem();
            items.Add(newItem);
        }
    }

    public ItemSlot GetRandomItem() {
        var wildItem = wildItems[UnityEngine.Random.Range(0, wildItems.Count)];
        return wildItem;
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
            if (currentSelection < items.Count) {
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
            StartCoroutine(HandleItem(items[currentSelection]));
        }
    }

    IEnumerator HandleItem(ItemSlot item) {
        if (item.Item is RitualItem) {
            inventory.StockItem((RitualItem)item.Item, item.Count);
            yield return dialogueBox.TypeDialogue("Stocked " + item.Count + " " + item.Item.Name);
        }
        else if (item.Item is HeldItem) {
            playerUnit.Devil.HeldItem = (HeldItem)item.Item;
            yield return dialogueBox.TypeDialogue(playerUnit.Devil.Base.Name + " held the " + item.Item.Name);
        }
        else if (item.Item is RecoveryItem) {
            item.Item.Use(playerUnit.Devil);
            yield return dialogueBox.TypeDialogue(playerUnit.Devil.Base.Name + " was healed");
        }
        else 
            Debug.Log("Failed to use item");

        yield return new WaitForSeconds(1f);
        StartCoroutine(EndSelection());
    }

    IEnumerator EndSelection() {
        state = ShopState.END;
        itemsUI.SetActive(false);
        yield return dialogueBox.TypeDialogue("The kobold thanks you for your custom.");
        yield return new WaitForSeconds(1f);
        OnShopOver();
    }
}
