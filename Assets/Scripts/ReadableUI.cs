using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadableUI : MonoBehaviour
{
    public static ReadableUI Instance;

    [Header("ROOT")]
    [SerializeField] private GameObject root;

    [Header("TEXT")]
    [SerializeField] private TextMeshProUGUI textField;

    [Header("BUTTONS")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button closeButton;

    private ReadablePage[] pages;
    private int currentPage;

    private PlayerController player;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);

        if (nextButton)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton)
            prevButton.onClick.AddListener(PreviousPage);

        if (closeButton)
            closeButton.onClick.AddListener(Close);
    }

    public void Open(ReadablePage[] newPages)
    {
        pages = newPages;
        currentPage = 0;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        player.LockInput(true);

        root.SetActive(true);
        UpdatePage();
    }

    public void Close()
    {
        root.SetActive(false);

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        player.LockInput(false);
    }

    public void NextPage()
    {
        if (currentPage >= pages.Length - 1)
            return;

        currentPage++;
        UpdatePage();
    }

    public void PreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        UpdatePage();
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
