using UnityEngine;

[System.Serializable]
public class ContainerSlot
{
    public UniqueItem_SO item;
    public ReadablePage[] readablePages;
    public Sprite readableIcon;

    public bool HasItem => item != null;
    public bool HasReadable => readablePages != null && readablePages.Length > 0;
}
