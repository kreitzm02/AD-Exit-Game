using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private GameObject creditsRoot;
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private GameObject questRoot;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject mainMenuScene;

    private void Start()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        mainMenuCanvas.enabled = true;
        mainMenuScene.SetActive(true);

        AudioManager.Instance.PlayMenuMusic();

        newGameButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        creditsButton.onClick.RemoveAllListeners();

        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        creditsButton.onClick.AddListener(OnCreditsClicked);

        playerSprite.enabled = false;
        player.transform.position = new Vector3(11.9f, -2.37f, -10.0f);
        player.LockInput(true);
        inventoryRoot.SetActive(false);
        questRoot.SetActive(false);
        creditsRoot.SetActive(false);

        BlackFadeManager.Instance.SetBlackInstant(true);
        BlackFadeManager.Instance.FadeFromBlack(() => { }, 2.0f);
    }

    private void Update()
    {
        continueButton.interactable = GameSaveSystem.HasSave();
    }

    private void OnExitClicked()
    {
        
    }

    private void OnCreditsClicked()
    {
        creditsRoot.SetActive(true);
        mainMenuCanvas.enabled = false;

        creditsBackButton.onClick.RemoveAllListeners();
        creditsBackButton.onClick.AddListener(OnCreditsBackClicked);
    }

    private void OnCreditsBackClicked()
    {
        creditsRoot.SetActive(false);
        mainMenuCanvas.enabled = true;
    }

    private void OnNewGameClicked()
    {
        playerSprite.enabled = true;
        player.LockInput(false);
        inventoryRoot.SetActive(true);
        questRoot.SetActive(true);
        mainMenuCanvas.enabled = false;
        mainMenuScene.SetActive(false);
        RoomManager.Instance.ChangeRoomNoFade("2F_Children", 0);

        LevelManager.Instance.StartLevelFromBeginning();
    }

    private void OnContinueClicked()
    {
        playerSprite.enabled = true;
        player.LockInput(false);
        inventoryRoot.SetActive(true);
        questRoot.SetActive(true);
        mainMenuCanvas.enabled = false;
        mainMenuScene.SetActive(false);

        LevelManager.Instance.StartGameFromSave();
    }
}
