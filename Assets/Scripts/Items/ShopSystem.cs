using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { START, SELECT, TARGETSELECT, BUSY, END }

public class ShopSystem : MonoBehaviour
{
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] GameObject itemsUI;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] TargetMenu shopMenu;
    [SerializeField] Inventory inventory;

    ShopState state, prevState;
    [SerializeField] List<ItemSlot> wildItems;
    List<ItemSlot> items = new List<ItemSlot>();
    private int currentSelection, currentTargetSelection;
    private ItemSlot selectedItem;
    private DevilParty targetParty;
    private Dictionary<Devil, bool> DevilsToTeach;
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
        if (state == ShopState.TARGETSELECT) {
            HandleTargetSelection();
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

        currentSelection = Mathf.Clamp(currentSelection, 0, items.Count-1);
        shopMenu.UpdateTargetSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(HandleItem(items[currentSelection]));
            StartCoroutine(BufferSelection());
        }
    }
    IEnumerator BufferSelection() {
        prevState = state;
        state = ShopState.BUSY;
        yield return new WaitForSeconds(0.01f);
        state = prevState;
    }

    void HandleTargetSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (currentTargetSelection < targetParty.Devils.Count) {
                ++currentTargetSelection;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (currentTargetSelection > 0) {
                --currentTargetSelection;
            }
        }

        currentTargetSelection = Mathf.Clamp(currentTargetSelection, 0, targetParty.Devils.Count-1);
        dialogueBox.TargetMenu.UpdateTargetSelection(currentTargetSelection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(TargetItem(targetParty.Devils[currentTargetSelection]));
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            ReturnToItemSelect();
        }
    }

    IEnumerator HandleItem(ItemSlot item) {
        if (item.Item is RitualItem) {
            inventory.StockItem((RitualItem)item.Item, item.Count);
            yield return dialogueBox.TypeDialogue("Stocked " + item.Count + " " + item.Item.Name);
            yield return new WaitForSeconds(1f);
            StartCoroutine(EndSelection());
        }
        else if (item.Item is ScrollItem) {
            selectedItem = item;
            ChooseTargetDevilToTeach((ScrollItem)item.Item);
        }
        else {
            selectedItem = item;
            ChooseTargetDevil();
        }
    }

    void ChooseTargetDevil() {
        state = ShopState.TARGETSELECT;
        targetParty = playerUnit.GetComponent<DevilParty>();
        dialogueBox.TargetMenu.SetDevilData(targetParty.Devils, false);
        dialogueBox.EnableTargetSelector(true);
        dialogueBox.SetDialogue("Choose a devil to give the item.");
    }

    void ChooseTargetDevilToTeach(ScrollItem scrollItem) {
        state = ShopState.TARGETSELECT;
        targetParty = playerUnit.GetComponent<DevilParty>();
        DevilsToTeach = new Dictionary<Devil, bool>();
        for (int i = 0; i < targetParty.Devils.Count; i++) {
            Devil devil = targetParty.Devils[i];
            bool canLearn = false;
            if (devil.Base.TeachableMoves.Contains(scrollItem.Move))
                canLearn = true;
            
            DevilsToTeach.Add(devil, canLearn);
        }
        dialogueBox.TargetMenu.SetDevilToTeachData(DevilsToTeach);
        dialogueBox.EnableTargetSelector(true);
        dialogueBox.SetDialogue("Choose a devil to teach the move.");
    }

    void ReturnToItemSelect() {
        state = ShopState.SELECT;
        dialogueBox.EnableTargetSelector(false);
        dialogueBox.SetDialogue("Choose an item to take with you.");
    }

    IEnumerator TargetItem(Devil devil) {
        if (selectedItem.Item is HeldItem) {
            if (devil.HeldItem != null) {
                dialogueBox.SetDialogue(devil.Base.Name + " already has an item.");
                yield break;
            }

            devil.HeldItem = (HeldItem)selectedItem.Item;
            yield return dialogueBox.TypeDialogue(devil.Base.Name + " held the " + selectedItem.Item.Name);
        }
        else if (selectedItem.Item is RecoveryItem) {
            selectedItem.Item.Use(devil);
            yield return dialogueBox.TypeDialogue(devil.Base.Name + " was healed");
        }
        else if (selectedItem.Item is ScrollItem) {
            var scroll = (ScrollItem)selectedItem.Item;
            if (!devil.Base.TeachableMoves.Contains(scroll.Move)) { 
                dialogueBox.SetDialogue(devil.Base.Name + " cannot learn that move.");
                yield break;
            }
            else if (devil.KnowsMove(scroll.Move)) {
                dialogueBox.SetDialogue(devil.Base.Name + " cannot learn that move.");
                yield break;
            }
            else { 
                state = ShopState.BUSY;
                itemsUI.SetActive(false);
                dialogueBox.EnableTargetSelector(false);
                yield return dialogueBox.LearnMove(scroll.Move, devil);
            }      
        }
        else
            Debug.Log("Failed to use item");

        yield return new WaitForSeconds(1f);
        StartCoroutine(EndSelection());
    }
    

    IEnumerator EndSelection() {
        state = ShopState.END;
        itemsUI.SetActive(false);
        dialogueBox.EnableTargetSelector(false);
        yield return dialogueBox.TypeDialogue("The kobold thanks you for your custom.");
        yield return new WaitForSeconds(1f);
        OnShopOver();
    }
}
