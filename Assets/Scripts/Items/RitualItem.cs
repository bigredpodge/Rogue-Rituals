using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Create new ritual item")]
public class RitualItem : ItemBase
{
    [SerializeField] float catchModifier = 1f;
    public float CatchModifier => catchModifier;
    public override bool Use(Devil devil) {
        return true;
    }

}

