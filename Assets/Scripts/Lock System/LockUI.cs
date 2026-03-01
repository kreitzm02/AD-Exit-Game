using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LockUI : MonoBehaviour
{
    public static LockUI Instance;

    [Header("ROOT")]
    [SerializeField] private Canvas root;

    [Header("LOCK LOGIC")]
    [SerializeField] private NumberLock numberLock;
    [SerializeField] private NumericDial[] dials = new NumericDial[3];

    [Header("BUTTONS")]
    [SerializeField] private Button closeButton;

    private LockInteractable currentLock;
    private PlayerController player;
    private bool isDummyLock = false;

    private void Start()
    {
        Instance = this;
        root.enabled = false;

        player = FindObjectOfType<PlayerController>();

        if (closeButton)
            closeButton.onClick.AddListener(Close);

        numberLock.OnCorrectCode.RemoveListener(OnSolved);
        numberLock.OnCorrectCode.AddListener(OnSolved);
    }

    public void Open(LockInteractable lockInteractable,
        int startA, int startB, int startC,
        int correctA, int correctB, int correctC, bool isDummy = false)
    {
        currentLock = lockInteractable;

        player.LockInput(true);

        root.enabled = true;

        dials[0].SetValue(startA, notify: false);
        dials[1].SetValue(startB, notify: false);
        dials[2].SetValue(startC, notify: false);

        numberLock.SetCode(correctA, correctB, correctC);

        numberLock.ResetLock();

        isDummyLock = isDummy;

        numberLock.Check();

        Time.timeScale = 0.0f;
    }

    public void Close()
    {
        root.enabled = false;
        player.LockInput(false);

        if (currentLock != null)
            currentLock.CloseFromUI();

        currentLock = null;
        Time.timeScale = 1.0f;

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnSolved()
    {
        if (currentLock == null || isDummyLock) return;

        StartCoroutine(OnSolvedCoroutine());
    }

    private IEnumerator OnSolvedCoroutine()
    {
        currentLock.NotifySolved();

        yield return new WaitForSecondsRealtime(2.0f);

        Close();
    }
}
