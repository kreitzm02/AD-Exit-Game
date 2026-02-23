using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ContainerUI : MonoBehaviour
{
    public static ContainerUI Instance;

    [Header("ROOT")]
    [SerializeField] private Canvas root;

    [Header("SLOTS (6 Buttons)")]
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Image[] slotIcons;

    private ContainerInteractable currentContainer;
    private PlayerController player;

    private void Start()
    {
        Instance = this;
        root.enabled = false;

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
        Time.timeScale = 0.0f;

        root.enabled = true;
        RefreshUI();
    }

    public void Close()
    {
        root.enabled = false;
        player.LockInput(false);
        currentContainer.CloseFromUI();
        currentContainer = null;
        Time.timeScale = 1.0f;
    }

    private void RefreshUI()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            ContainerSlot slot = currentContainer.Slots[i];

            if (slot.item != null)
            {
                slotIcons[i].sprite = slot.item.icon;
                slotIcons[i].enabled = true;
            }
            else if (slot.HasReadable)
            {
                slotIcons[i].sprite = slot.readableIcon;
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

        ContainerSlot slot = currentContainer.Slots[index];

        if (!slot.HasItem && slot.HasReadable)
        {
            OpenReadableFromContainer(slot.GetReadablePages());
            return;
        }

        if (!slot.HasItem)
            return;

        bool collected = PlayerInventory.Instance.AddItem(slot.item);

        if (collected)
        {
            slot.item = null;
            RefreshUI();
        }
    }

    private void OpenReadableFromContainer(ReadablePage[] pages)
    {
        root.enabled = false;

        ReadableUI.Instance.Closed += OnReadableClosed;

        ReadableUI.Instance.Open(pages, currentContainer.ReadableFontSize, false);
    }

    private void OnReadableClosed()
    {
        ReadableUI.Instance.Closed -= OnReadableClosed;

        if (currentContainer != null)
            root.enabled = true;
    }
}
