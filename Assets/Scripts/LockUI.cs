using UnityEngine;
using UnityEngine.UI;

public class LockUI : MonoBehaviour
{
    public static LockUI Instance;

    [Header("ROOT")]
    [SerializeField] private GameObject root;

    [Header("LOCK LOGIC")]
    [SerializeField] private NumberLock numberLock;
    [SerializeField] private NumericDial[] dials = new NumericDial[3];

    [Header("BUTTONS")]
    [SerializeField] private Button closeButton;

    private LockInteractable currentLock;
    private PlayerController player;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);

        player = FindObjectOfType<PlayerController>();

        if (closeButton)
            closeButton.onClick.AddListener(Close);

        numberLock.OnCorrectCode.RemoveListener(OnSolved);
        numberLock.OnCorrectCode.AddListener(OnSolved);
    }

    public void Open(LockInteractable lockInteractable,
        int startA, int startB, int startC,
        int correctA, int correctB, int correctC)
    {
        currentLock = lockInteractable;

        player.LockInput(true);
        Time.timeScale = 0.0f;

        root.SetActive(true);

        dials[0].SetValue(startA, notify: false);
        dials[1].SetValue(startB, notify: false);
        dials[2].SetValue(startC, notify: false);

        numberLock.SetCode(correctA, correctB, correctC);

        numberLock.ResetLock();
        numberLock.Check();
    }

    public void Close()
    {
        root.SetActive(false);
        player.LockInput(false);

        if (currentLock != null)
            currentLock.CloseFromUI();

        currentLock = null;
        Time.timeScale = 1.0f;
    }

    private void OnSolved()
    {
        if (currentLock == null) return;

        currentLock.NotifySolved();
        Close();
    }
}
