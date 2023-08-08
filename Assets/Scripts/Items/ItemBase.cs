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
    [SerializeField] private float price;
    [SerializeField] private bool isSellabe;
    
    public virtual string Name => name;

    public string Description => description;

    public Sprite Icon => icon;
    public float Price => price;
    public bool IsSellabe => isSellabe;

    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }

    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;
    public virtual bool IsReusable => false;
}
