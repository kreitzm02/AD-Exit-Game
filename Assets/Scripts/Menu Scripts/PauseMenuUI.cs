using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
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
        if (!mainMenuUI.GetComponent<Canvas>().enabled && !pauseCanvas.enabled && !controlsRoot.activeSelf && !creditsRoot.activeSelf && 
            subtitleUI.GetComponent<CanvasGroup>().alpha < 1.0f) pauseMenuCanBeOpened = true;
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
    }

    private void OnMainMenuButtonClicked()
    {

        debugController.DeactivateEverything();
        mainMenuUI.OpenMainMenu();
        pauseCanvas.enabled = false;
        player.LockInput(false);
        Time.timeScale = 1.0f;
        RoomManager.Instance.DeactivateAllRooms();
        PlayerInventory.Instance.ClearInventory();

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnContinueButtonClicked()
    {
        pauseCanvas.enabled = false;

        Time.timeScale = 1.0f;

        AudioManager.Instance.PlayButtonSound();
        player.LockInput(false);
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
