using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour // TODO refactor this
{
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private GameObject controlsRoot;
    [SerializeField] private GameObject creditsRoot;
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private DebugController debugController;
    [SerializeField] private SubtitleUI subtitleUI;
    [SerializeField] private PlayerController player;

    private bool pauseMenuCanBeOpened = false;

    void Update()
    {
        if (!mainMenuUI.GetComponent<Canvas>().enabled && !pauseCanvas.enabled && !controlsRoot.activeSelf && !creditsRoot.activeSelf) pauseMenuCanBeOpened = true;
        else pauseMenuCanBeOpened = false;

        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenuCanBeOpened)
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        continueButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();

        continueButton.onClick.AddListener(OnContinueButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        controlsButton.onClick.AddListener(OnControlsButtonClicked);

        pauseCanvas.enabled = true;

        player.LockInput(true);

        Time.timeScale = 0.0f;

        GameEvents.PauseMenuIsOpen(true);
    }

    private void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1.0f;

        AudioManager.Instance.PlayButtonSound();

        AudioManager.Instance.StopMusicImmediate();

        GameEvents.PauseMenuIsOpen(false);

        SceneManager.LoadScene(0);
    }

    private void OnContinueButtonClicked()
    {
        pauseCanvas.enabled = false;

        Time.timeScale = 1.0f;

        AudioManager.Instance.PlayButtonSound();
        player.LockInput(false);

        GameEvents.PauseMenuIsOpen(false);
    }

    private void OnControlsButtonClicked()
    {
        pauseCanvas.enabled = false;

        controlsBackButton.onClick.RemoveAllListeners();
        controlsBackButton.onClick.AddListener(OnControlsBackButtonClicked);
        controlsRoot.SetActive(true);

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnControlsBackButtonClicked()
    {
        pauseCanvas.enabled = true;
        controlsRoot.SetActive(false);

        AudioManager.Instance.PlayButtonSound();
    }
}
