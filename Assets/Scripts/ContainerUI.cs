using UnityEngine;
using UnityEngine.UI;

public class ContainerUI : MonoBehaviour
{
    public static ContainerUI Instance;

    [Header("ROOT")]
    [SerializeField] private GameObject root;

    [Header("SLOTS (6 Buttons)")]
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Image[] slotIcons;

    private ContainerInteractable currentContainer;
    private PlayerController player;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);

        player = FindObjectOfType<PlayerController>();

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    public void Open(ContainerInteractable container)
    {
        currentContainer = container;
        player.LockInput(true);

        root.SetActive(true);
        RefreshUI();
    }

    public void Close()
    {
        root.SetActive(false);
        player.LockInput(false);
        currentContainer = null;
    }

    private void RefreshUI()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            var slot = currentContainer.Slots[i];

            if (slot.item != null)
            {
                slotIcons[i].sprite = slot.item.icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }
    }

    private void OnSlotClicked(int index)
    {
        if (currentContainer == null)
            return;

        var slot = currentContainer.Slots[index];

        if (slot.item == null)
            return;

        bool collected = PlayerInventory.Instance.AddItem(slot.item);

        if (collected)
        {
            slot.item = null;
            RefreshUI();
        }
    }
}
