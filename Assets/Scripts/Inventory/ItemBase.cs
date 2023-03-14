using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//TODO: component instead of inheritance
public class ItemBase : ScriptableObject
{
    [SerializeField] new string  name;
    [TextArea][SerializeField] string description;
    [SerializeField] Sprite icon;
    
    public string Name => name;

    public string Description => description;

    public Sprite Icon => icon;

    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }

    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;
}
