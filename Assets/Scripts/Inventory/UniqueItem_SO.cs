using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Unique Item")]
public class UniqueItem_SO : ScriptableObject
{
    public string itemId;          
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
}
