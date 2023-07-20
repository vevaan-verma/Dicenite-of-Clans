using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class MainMenuUIController : MonoBehaviourPunCallbacks {

    [Header("References")]
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private NetworkManager networkManagerPrefab;
    private MainMenuAudioManager mainMenuAudioManager;
    private GameManager gameManager;

    [Header("Main Menu UI")]
    [SerializeField] private CanvasGroup menuHUD;
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Room List UI")]
    [SerializeField] private CanvasGroup roomListHUD;
    [SerializeField] private Button closeRoomListButton;
    [SerializeField] private GameObject roomButtonPrefab;
    [SerializeField] private Transform roomViewContent;
    [SerializeField] private TMP_Text noRoomsText;
    [SerializeField] private Button createRoomHUDButton;
    private List<RoomButton> roomButtons;

    [Header("Create Room UI")]
    [SerializeField] private int roomNameCharLimit;
    [SerializeField] private CanvasGroup createRoomHUD;
    [SerializeField] private Button closeCreateRoomButton;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Options UI")]
    [SerializeField] private CanvasGroup optionsHUD;
    [SerializeField] private Button closeOptionsButton;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Transform qualityArrow;
    [SerializeField] private Button creditsButton;
    private bool arrowExpanded;

    [Header("Credits UI")]
    [SerializeField] private CanvasGroup creditsHUD;
    [SerializeField] private Button closeCreditsButton;

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingScreen;
    private TMP_Text loadingText;

    [Header("Skybox")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;
    [SerializeField] private int nightSkyboxOdds;

    [Header("Connection Settings")]
    [SerializeField] private float connectedDisplayDuration;

    [Header("Animations")]
    [SerializeField] private float menuHUDFadeDuration;
    [SerializeField] private float roomListHUDFadeDuration;
    [SerializeField] private float createRoomHUDFadeDuration;
    [SerializeField] private float clearErrorFadeDuration;
    [SerializeField] private float clearErrorWaitDuration;
    [SerializeField] private float optionsHUDFadeDuration;
    [SerializeField] private float qualityArrowRotateDuration;
    [SerializeField] private float creditsHUDFadeDuration;
    private Coroutine menuHUDFadeCoroutine;
    private Coroutine roomListHUDFadeCoroutine;
    private Coroutine createRoomHUDFadeCoroutine;
    private Coroutine errorTextFadeCoroutine;
    private Coroutine optionsHUDFadeCoroutine;
    private Coroutine qualityArrowCoroutine;
    private Coroutine creditsHUDFadeCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float loadingFadeDuration;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        mainMenuAudioManager = FindObjectOfType<MainMenuAudioManager>();
        loadingText = loadingScreen.GetComponentInChildren<TMP_Text>();

        roomButtons = new List<RoomButton>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");
        SetErrorText("");
        noRoomsText.gameObject.SetActive(false);
        noRoomsText.text = "No Rooms Available";

        loadingScreen.gameObject.SetActive(true);

        menuHUD.gameObject.SetActive(false);
        playButton.onClick.AddListener(OpenRoomListHUD);
        optionsButton.onClick.AddListener(OpenOptionsHUD);
        quitButton.onClick.AddListener(QuitGame);

        closeRoomListButton.onClick.AddListener(OnCloseRoomListHUD);

        roomListHUD.gameObject.SetActive(false);
        roomListHUD.alpha = 0f;
        createRoomHUDButton.onClick.AddListener(OpenCreateRoomHUD);

        createRoomHUD.gameObject.SetActive(false);
        createRoomHUD.alpha = 0f;
        closeCreateRoomButton.onClick.AddListener(OnCloseCreateRoomHUD);
        roomNameInput.characterLimit = roomNameCharLimit;
        maxPlayersInput.text = gameManager.GetMaxPlayers() + "";
        maxPlayersInput.interactable = false;
        createRoomButton.onClick.AddListener(CreateRoom);

        optionsHUD.gameObject.SetActive(false);
        optionsHUD.alpha = 0f;
        closeOptionsButton.onClick.AddListener(OnCloseOptionsHUD);
        qualityDropdown.onValueChanged.AddListener(ChangeQuality);
        creditsButton.onClick.AddListener(OpenCreditsHUD);

        string qualityLevel = PlayerPrefs.GetString("Quality");

        if (!qualityLevel.IsNullOrEmpty()) {

            int level = int.Parse(qualityLevel);
            qualityDropdown.value = level;
            QualitySettings.SetQualityLevel(level);

        } else {

            qualityDropdown.value = 4;
            QualitySettings.SetQualityLevel(4);
            PlayerPrefs.SetString("Quality", "4");

        }

        creditsHUD.gameObject.SetActive(false);
        closeCreditsButton.onClick.AddListener(OnCloseCreditsHUD);

        RandomizeSkybox();
        SetLoadingText("Connecting to Server...");

        if (!PhotonNetwork.IsConnected) {

            PhotonNetwork.ConnectUsingSettings();

        }

        PhotonNetwork.AutomaticallySyncScene = true;

    }

    private void Update() {

        if (arrowExpanded != qualityDropdown.IsExpanded) {

            if (qualityDropdown.IsExpanded) {

                ExpandQualityArrow();

            } else {

                CloseQualityArrow();

            }

            arrowExpanded = qualityDropdown.IsExpanded;

        }
    }

    public void ExpandQualityArrow() {

        if (qualityArrowCoroutine != null) {

            StopCoroutine(qualityArrowCoroutine);

        }

        qualityArrowCoroutine = StartCoroutine(RotateQualityArrow(qualityArrow.localRotation, Quaternion.Euler(0f, 0f, 180f)));

    }

    public void CloseQualityArrow() {

        if (qualityArrowCoroutine != null) {

            StopCoroutine(qualityArrowCoroutine);

        }

        qualityArrowCoroutine = StartCoroutine(RotateQualityArrow(qualityArrow.localRotation, Quaternion.identity));

    }

    private IEnumerator RotateQualityArrow(Quaternion startRotation, Quaternion targetRotation) {

        float currentTime = 0f;

        while (currentTime < qualityArrowRotateDuration) {

            currentTime += Time.deltaTime;
            qualityArrow.localRotation = Quaternion.Lerp(startRotation, targetRotation, currentTime / qualityArrowRotateDuration);
            yield return null;

        }

        qualityArrow.localRotation = targetRotation;
        qualityArrowCoroutine = null;

    }

    private void SetLoadingText(string text) {

        loadingText.text = text;

    }

    private void SetErrorText(string text) {

        errorText.text = text;
        errorText.color = new Color(errorText.color.r, errorText.color.g, errorText.color.b, 1f);

        if (errorTextFadeCoroutine != null) {

            StopCoroutine(errorTextFadeCoroutine);

        }

        errorTextFadeCoroutine = StartCoroutine(ClearErrorText(errorText.color, new Color(errorText.color.r, errorText.color.g, errorText.color.b, 0f)));

    }

    public override void OnConnectedToMaster() {

        StartCoroutine(ConnectedToMaster());

    }

    private IEnumerator ConnectedToMaster() {

        SetLoadingText("Connected to Server...");
        yield return new WaitForSeconds(connectedDisplayDuration);
        SetLoadingText("Joining Lobby...");
        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby() {

        StartCoroutine(JoinedLobby());

    }

    private IEnumerator JoinedLobby() {

        SetLoadingText("Joined Lobby...");
        yield return new WaitForSeconds(connectedDisplayDuration);
        OpenMenuHUD();
        CloseLoadingScreen();

    }

    private void RandomizeSkybox() {

        int daytime = Random.Range(0, nightSkyboxOdds);

        if (daytime == nightSkyboxOdds) {

            RenderSettings.skybox = nightSkybox;
            mainMenuAudioManager.PlaySound(MainMenuAudioManager.MainMenuSoundType.Music_Night);

        } else {

            RenderSettings.skybox = daySkybox;
            mainMenuAudioManager.PlaySound(MainMenuAudioManager.MainMenuSoundType.Music_Day);

        }
    }

    private void CreateRoom() {

        if (roomNameInput.text.IsNullOrEmpty()) {

            SetErrorText("Error: No Name Given");
            return;

        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = gameManager.GetMaxPlayers();
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        OpenLoadingScreen();

    }

    private IEnumerator ClearErrorText(Color startColor, Color targetColor) {

        yield return new WaitForSeconds(clearErrorWaitDuration);

        float currentTime = 0f;
        errorText.gameObject.SetActive(true);

        while (currentTime < loadingFadeDuration) {

            currentTime += Time.deltaTime;
            errorText.color = Color.Lerp(startColor, targetColor, currentTime / clearErrorFadeDuration);
            yield return null;

        }

        errorText.color = targetColor;
        errorTextFadeCoroutine = null;
        SetErrorText("");

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {

        foreach (RoomButton button in roomButtons) {

            Destroy(button);

        }

        roomButtons.Clear();

        if (roomList.Count == 0) {

            noRoomsText.gameObject.SetActive(true);
            return;

        }

        noRoomsText.gameObject.SetActive(false);

        foreach (RoomInfo roomInfo in roomList) {

            if (roomInfo.IsVisible && roomInfo.IsOpen) {

                RoomButton roomButton = Instantiate(roomButtonPrefab, roomViewContent).GetComponent<RoomButton>();
                roomButton.Initialize(roomInfo);
                roomButtons.Add(roomButton);

            }
        }
    }

    public override void OnJoinedRoom() {

        DontDestroyOnLoad(PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity));
        gameManager.UpdateGameState(GameManager.GameState.Waiting);

        if (PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers()) {

            SetLoadingText("Loading Game...");

            if (PhotonNetwork.IsMasterClient) {

                PhotonNetwork.LoadLevel(nextSceneName);
                loadingScreen.gameObject.SetActive(true);

            }
        } else {

            SetLoadingText("Waiting for Players...");
            loadingScreen.gameObject.SetActive(true);

        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {

        gameManager.UpdateGameState(GameManager.GameState.Waiting);

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers()) {

            PhotonNetwork.LoadLevel(nextSceneName);

        }
    }

    private void OpenMenuHUD() {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, 1f, true));

    }

    private void CloseMenuHUD() {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, 0f, false));

    }

    private IEnumerator FadeMenuHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        menuHUD.gameObject.SetActive(true);

        while (currentTime < menuHUDFadeDuration) {

            currentTime += Time.deltaTime;
            menuHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / menuHUDFadeDuration);
            yield return null;

        }

        menuHUD.alpha = targetOpacity;
        menuHUDFadeCoroutine = null;

        if (!fadeIn) {

            menuHUD.gameObject.SetActive(false);

        }
    }

    private void OnCloseRoomListHUD() {

        OpenMenuHUD();
        CloseRoomListHUD();

    }

    private void OpenRoomListHUD() {

        if (roomListHUDFadeCoroutine != null) {

            StopCoroutine(roomListHUDFadeCoroutine);

        }

        if (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) {

            return;

        }

        CloseMenuHUD();
        roomListHUDFadeCoroutine = StartCoroutine(FadeRoomList(roomListHUD.alpha, 1f, true));

    }

    private void CloseRoomListHUD() {

        if (roomListHUDFadeCoroutine != null) {

            StopCoroutine(roomListHUDFadeCoroutine);

        }

        roomListHUDFadeCoroutine = StartCoroutine(FadeRoomList(roomListHUD.alpha, 0f, false));

    }

    private IEnumerator FadeRoomList(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        roomListHUD.gameObject.SetActive(true);

        while (currentTime < roomListHUDFadeDuration) {

            currentTime += Time.deltaTime;
            roomListHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / roomListHUDFadeDuration);
            yield return null;

        }

        roomListHUD.alpha = targetOpacity;
        roomListHUDFadeCoroutine = null;

        if (!fadeIn) {

            roomListHUD.gameObject.SetActive(false);

        }
    }

    private void OnCloseCreateRoomHUD() {

        OpenRoomListHUD();
        CloseCreateRoomHUD();

    }

    private void OpenCreateRoomHUD() {

        if (createRoomHUDFadeCoroutine != null) {

            StopCoroutine(createRoomHUDFadeCoroutine);

        }

        CloseRoomListHUD();
        createRoomHUDFadeCoroutine = StartCoroutine(FadeCreateRoomHUD(createRoomHUD.alpha, 1f, true));

    }

    private void CloseCreateRoomHUD() {

        if (createRoomHUDFadeCoroutine != null) {

            StopCoroutine(createRoomHUDFadeCoroutine);

        }

        createRoomHUDFadeCoroutine = StartCoroutine(FadeCreateRoomHUD(createRoomHUD.alpha, 0f, false));

    }

    private IEnumerator FadeCreateRoomHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        createRoomHUD.gameObject.SetActive(true);

        while (currentTime < createRoomHUDFadeDuration) {

            currentTime += Time.deltaTime;
            createRoomHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / createRoomHUDFadeDuration);
            yield return null;

        }

        createRoomHUD.alpha = targetOpacity;
        createRoomHUDFadeCoroutine = null;

        if (fadeIn) {

            roomNameInput.ActivateInputField();
            roomNameInput.Select();

        } else {

            createRoomHUD.gameObject.SetActive(false);

        }
    }

    private void OnCloseOptionsHUD() {

        OpenMenuHUD();
        CloseOptionsHUD();

    }

    private void OpenOptionsHUD() {

        if (optionsHUDFadeCoroutine != null) {

            StopCoroutine(optionsHUDFadeCoroutine);

        }

        CloseMenuHUD();
        optionsHUDFadeCoroutine = StartCoroutine(FadeOptionsHUD(optionsHUD.alpha, 1f, true));

    }

    private void CloseOptionsHUD() {

        if (optionsHUDFadeCoroutine != null) {

            StopCoroutine(optionsHUDFadeCoroutine);

        }

        optionsHUDFadeCoroutine = StartCoroutine(FadeOptionsHUD(optionsHUD.alpha, 0f, false));

    }

    private IEnumerator FadeOptionsHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        optionsHUD.gameObject.SetActive(true);

        while (currentTime < optionsHUDFadeDuration) {

            currentTime += Time.deltaTime;
            optionsHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / optionsHUDFadeDuration);
            yield return null;

        }

        optionsHUD.alpha = targetOpacity;
        optionsHUDFadeCoroutine = null;

        if (!fadeIn) {

            optionsHUD.gameObject.SetActive(false);

        }
    }

    private void ChangeQuality(int qualityLevel) {

        QualitySettings.SetQualityLevel(qualityLevel);
        PlayerPrefs.SetString("Quality", qualityLevel + "");

    }

    private void OnCloseCreditsHUD() {

        OpenOptionsHUD();
        CloseCreditsHUD();

    }

    private void OpenCreditsHUD() {

        if (creditsHUDFadeCoroutine != null) {

            StopCoroutine(creditsHUDFadeCoroutine);

        }

        CloseOptionsHUD();
        creditsHUDFadeCoroutine = StartCoroutine(FadeCreditsHUD(creditsHUD.alpha, 1f, true));

    }

    private void CloseCreditsHUD() {

        if (creditsHUDFadeCoroutine != null) {

            StopCoroutine(creditsHUDFadeCoroutine);

        }

        creditsHUDFadeCoroutine = StartCoroutine(FadeCreditsHUD(creditsHUD.alpha, 0f, false));

    }

    private IEnumerator FadeCreditsHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        creditsHUD.gameObject.SetActive(true);

        while (currentTime < creditsHUDFadeDuration) {

            currentTime += Time.deltaTime;
            creditsHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / creditsHUDFadeDuration);
            yield return null;

        }

        creditsHUD.alpha = targetOpacity;
        creditsHUDFadeCoroutine = null;

        if (!fadeIn) {

            creditsHUD.gameObject.SetActive(false);

        }
    }

    private void OpenLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        CloseRoomListHUD();
        CloseCreateRoomHUD();
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.alpha, 1f, true));

    }

    private void CloseLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.alpha, 0f, false));

    }

    private IEnumerator FadeLoadingScreen(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        loadingScreen.gameObject.SetActive(true);

        while (currentTime < loadingFadeDuration) {

            currentTime += Time.deltaTime;
            loadingScreen.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / loadingFadeDuration);
            yield return null;

        }

        loadingScreen.alpha = targetOpacity;
        loadingFadeCoroutine = null;

        if (!fadeIn) {

            loadingScreen.gameObject.SetActive(false);

        }
    }

    private void QuitGame() {

        Application.Quit();

    }
}
