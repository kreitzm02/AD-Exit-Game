using FMODUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadableUI : MonoBehaviour
{
    public static ReadableUI Instance;

    public event Action Closed;

    [Header("ROOT")]
    [SerializeField] private Canvas root;

    [Header("TEXT")]
    [SerializeField] private TextMeshProUGUI textField;

    [Header("BUTTONS")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button closeButton;

    [Header("SFX")]
    [SerializeField] private EventReference pageFlipSound;
    [SerializeField] private EventReference closeSound;

    private bool lockPlayer = true;

    private ReadablePage[] pages;
    private int currentPage;

    private PlayerController player;

    private void Start()
    {
        Instance = this;
        root.enabled = false;

        if (nextButton)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton)
            prevButton.onClick.AddListener(PreviousPage);

        if (closeButton)
            closeButton.onClick.AddListener(Close);
    }

    public void Open(ReadablePage[] newPages, float fontSize, bool lockPl = true)
    {
        pages = newPages;
        currentPage = 0;
        lockPlayer = lockPl;
        textField.fontSize = fontSize;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (lockPlayer) player.LockInput(true);

        root.enabled = true;
        UpdatePage();
    }

    public void Close()
    {
        root.enabled = false;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (lockPlayer) player.LockInput(false);

        AudioManager.Instance.PlaySFX(closeSound);

        Closed?.Invoke();
    }

    public void NextPage()
    {
        if (currentPage >= pages.Length - 1)
            return;

        currentPage++;
        UpdatePage();

        AudioManager.Instance.PlaySFX(pageFlipSound);
    }

    public void PreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        UpdatePage();

        AudioManager.Instance.PlaySFX(pageFlipSound);
    }

    private void UpdatePage()
    {
        if (pages == null || pages.Length == 0)
            return;

        textField.text = pages[currentPage].text;

        if (nextButton)
            nextButton.gameObject.SetActive(currentPage < pages.Length - 1);

        if (prevButton)
            prevButton.gameObject.SetActive(currentPage > 0);
    }
}
