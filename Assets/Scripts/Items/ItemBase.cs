using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite sprite;

    public string Name => name;
    public string Description => description;
    public Sprite Sprite => sprite;

    public virtual bool Use(Devil devil) {
        return false;
    }
    
}
