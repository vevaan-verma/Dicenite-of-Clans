using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviourPunCallbacks {

    [Header("References")]
    [SerializeField] private PhotonView playerPrefab;
    private GameManager gameManager;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuHUD;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    private TMP_Text loadingText;

    [Header("Connection Settings")]
    [SerializeField] private float connectedDisplayDuration;

    [Header("Animations")]
    [SerializeField] private float menuHUDFadeDuration;
    [SerializeField][Range(0f, 1f)] private float menuHUDFadeOpacity;
    private Coroutine menuHUDFadeCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float loadingFadeDuration;
    [SerializeField][Range(0f, 1f)] private float loadingFadeOpacity;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        loadingText = loadingScreen.GetComponentInChildren<TMP_Text>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        loadingScreen.gameObject.SetActive(true);
        playButton.onClick.AddListener(JoinRandomGame);
        quitButton.onClick.AddListener(QuitGame);

        SetLoadingText("Connecting to Server...");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

    }

    public void SetLoadingText(string text) {

        loadingText.text = text;

    }

    private void JoinRandomGame() {

        if (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) {

            return;

        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = gameManager.GetMaxPlayers();
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        StartFadeOutMenuHUD(0f);

    }

    public override void OnConnectedToMaster() {

        StartCoroutine(ConnectToMaster());

    }

    private IEnumerator ConnectToMaster() {

        SetLoadingText("Connected to Server...");
        yield return new WaitForSeconds(connectedDisplayDuration);
        StartFadeOutLoadingScreen();

    }

    public override void OnJoinedRoom() {

        DontDestroyOnLoad(PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity));
        loadingScreen.gameObject.SetActive(true);
        SetLoadingText("Waiting for Players...");
        gameManager.UpdateGameState(GameManager.GameState.Waiting);

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers()) {

            PhotonNetwork.LoadLevel(nextSceneName);

        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {

        gameManager.UpdateGameState(GameManager.GameState.Waiting);

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers()) {

            PhotonNetwork.LoadLevel(nextSceneName);

        }
    }

    public void LoadDiceScene() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f);
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, loadingFadeOpacity), true, nextSceneName));

    }

    public void StartFadeOutLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f), false));

    }

    private IEnumerator FadeLoadingScreen(Color startColor, Color targetColor, bool fadeIn, string sceneName = "") {

        float currentTime = 0f;
        loadingScreen.gameObject.SetActive(true);

        while (currentTime < loadingFadeDuration) {

            currentTime += Time.deltaTime;
            loadingScreen.color = Color.Lerp(startColor, targetColor, currentTime / loadingFadeDuration);
            yield return null;

        }

        loadingScreen.color = targetColor;
        loadingFadeCoroutine = null;

        if (!fadeIn) {

            loadingScreen.gameObject.SetActive(false);

        }
    }

    public void StartFadeInMenuHUD() {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, 1f));

    }

    public void StartFadeOutMenuHUD(float targetOpacity) {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, targetOpacity));

    }

    private IEnumerator FadeMenuHUD(float startOpacity, float targetOpacity) {

        float currentTime = 0f;

        while (currentTime < menuHUDFadeDuration) {

            currentTime += Time.deltaTime;
            menuHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / menuHUDFadeDuration);
            yield return null;

        }

        menuHUD.alpha = targetOpacity;
        menuHUDFadeCoroutine = null;

    }

    private void QuitGame() {

        Application.Quit();

    }
}
