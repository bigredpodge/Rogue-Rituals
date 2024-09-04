using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Create new held item")]
public class HeldItem : ItemBase
{
[SerializeField] DamageModifier brandDamageModifier;
    public float GetDamageModifiers(Move move) {
        float damageModifier = 1f;

        if (move.Base.Brand == brandDamageModifier.brand)
            damageModifier = brandDamageModifier.damageModifier;

        Debug.Log("Item Mod: " + damageModifier);
        return damageModifier;
    }

}

[System.Serializable]
public class DamageModifier {
    public DevilBrand brand;
    public float damageModifier;
}

