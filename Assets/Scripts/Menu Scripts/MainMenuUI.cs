using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private GameObject creditsRoot;
    [SerializeField] private GameObject controlsRoot;
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private GameObject questRoot;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject mainMenuScene;
    [SerializeField] private GameObject combineUIRoot;
    [SerializeField] private GameObject itemDetailsRoot;

    private void Start()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        mainMenuCanvas.enabled = true;
        mainMenuScene.SetActive(true);

        AudioManager.Instance.PlayNewMusic(AudioManager.MusicType.MENU);

        newGameButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        creditsButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();

        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        creditsButton.onClick.AddListener(OnCreditsClicked);
        controlsButton.onClick.AddListener(OnControlsClicked);

        playerSprite.enabled = false;
        player.transform.position = new Vector3(11.9f, -2.37f, -10.0f);
        player.LockInput(true);
        inventoryRoot.SetActive(false);
        questRoot.SetActive(false);
        creditsRoot.SetActive(false);
        combineUIRoot.SetActive(false);
        itemDetailsRoot.SetActive(false);

        BlackFadeManager.Instance.SetBlackInstant(true);
        BlackFadeManager.Instance.FadeFromBlack(() => { }, 2.0f);
    }

    public void OpenCreditsFromMainMenu()
    {
        OnCreditsClicked();
    }

    private void Update()
    {
        continueButton.interactable = GameSaveSystem.HasSave();
    }

    private void OnExitClicked()
    {
#if !UNITY_EDITOR
        Application.Quit();
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnCreditsClicked()
    {
        creditsRoot.SetActive(true);
        mainMenuCanvas.enabled = false;

        creditsBackButton.onClick.RemoveAllListeners();
        creditsBackButton.onClick.AddListener(OnCreditsBackClicked);

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnCreditsBackClicked()
    {
        creditsRoot.SetActive(false);
        mainMenuCanvas.enabled = true;

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnControlsClicked()
    {
        controlsRoot.SetActive(true);
        mainMenuCanvas.enabled = false;

        controlsBackButton.onClick.RemoveAllListeners();
        controlsBackButton.onClick.AddListener(OnControlsBackClicked);

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnControlsBackClicked()
    {
        controlsRoot.SetActive(false);
        mainMenuCanvas.enabled = true;

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnNewGameClicked()
    {
        playerSprite.enabled = true;
        player.LockInput(false);
        inventoryRoot.SetActive(true);
        questRoot.SetActive(true);
        combineUIRoot.SetActive(true);
        itemDetailsRoot.SetActive(true);
        mainMenuCanvas.enabled = false;
        mainMenuScene.SetActive(false);
        RoomManager.Instance.ChangeRoomNoFade("2F_Children", 0);

        LevelManager.Instance.StartLevelFromBeginning();

        AudioManager.Instance.PlayButtonSound();
    }

    private void OnContinueClicked()
    {
        playerSprite.enabled = true;
        player.LockInput(false);
        inventoryRoot.SetActive(true);
        questRoot.SetActive(true);
        mainMenuCanvas.enabled = false;
        mainMenuScene.SetActive(false);
        combineUIRoot.SetActive(true);
        itemDetailsRoot.SetActive(true);

        LevelManager.Instance.StartGameFromSave();

        AudioManager.Instance.PlayButtonSound();
    }
}
