using Photon.Pun;
using Photon.Realtime;
using System;
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
    private GameManager gameManager;

    [Header("Main Menu UI")]
    [SerializeField] private CanvasGroup menuHUD;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button testGameButton;
    [SerializeField] private TMP_Text noRoomsText;
    private TMP_Text loadingText;

    [Header("Room List UI")]
    [SerializeField] private CanvasGroup roomListHUD;
    [SerializeField] private Button closeRoomListButton;
    [SerializeField] private GameObject roomButtonPrefab;
    [SerializeField] private Transform roomViewContent;
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

    [Header("Connection Settings")]
    [SerializeField] private float connectedDisplayDuration;

    [Header("Animations")]
    [SerializeField] private float menuHUDFadeDuration;
    [SerializeField][Range(0f, 1f)] private float menuHUDFadeOpacity;
    [SerializeField] private float roomListHUDFadeDuration;
    [SerializeField] private float createRoomHUDFadeDuration;
    [SerializeField] private float clearErrorFadeDuration;
    [SerializeField] private float clearErrorWaitDuration;
    private Coroutine menuHUDFadeCoroutine;
    private Coroutine roomListHUDFadeCoroutine;
    private Coroutine createRoomHUDFadeCoroutine;
    private Coroutine errorTextFadeCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float loadingFadeDuration;
    [SerializeField][Range(0f, 1f)] private float loadingFadeOpacity;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        loadingText = loadingScreen.GetComponentInChildren<TMP_Text>();

        roomButtons = new List<RoomButton>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");
        SetErrorText("");
        noRoomsText.gameObject.SetActive(false);
        noRoomsText.text = "No Rooms Available";

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        loadingScreen.gameObject.SetActive(true);
        playButton.onClick.AddListener(OpenRoomList);
        quitButton.onClick.AddListener(QuitGame);

        if (Application.isEditor) {

            testGameButton.gameObject.SetActive(true);
            testGameButton.onClick.AddListener(CreateTestGame);

        } else {

            testGameButton.gameObject.SetActive(false);

        }

        closeRoomListButton.onClick.AddListener(CloseRoomList);

        createRoomHUDButton.onClick.AddListener(OpenCreateRoomHUD);
        roomListHUD.gameObject.SetActive(false);
        roomListHUD.alpha = 0f;

        closeCreateRoomButton.onClick.AddListener(CloseCreateRoomHUD);
        createRoomHUD.gameObject.SetActive(false);
        createRoomHUD.alpha = 0f;
        roomNameInput.characterLimit = roomNameCharLimit;
        maxPlayersInput.text = gameManager.GetMaxPlayers() + "";
        maxPlayersInput.interactable = false;
        createRoomButton.onClick.AddListener(CreateRoom);

        SetLoadingText("Connecting to Server...");

        if (!PhotonNetwork.IsConnected) {

            PhotonNetwork.ConnectUsingSettings();

        }

        PhotonNetwork.AutomaticallySyncScene = true;

    }

    public void SetLoadingText(string text) {

        loadingText.text = text;

    }

    public void SetErrorText(string text) {

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
        StartFadeOutLoadingScreen();

    }

    private void CreateRoom() {

        if (roomNameInput.text.IsNullOrEmpty()) {

            SetErrorText("Error: No Name Given");
            return;

        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = gameManager.GetMaxPlayers();
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        StartFadeInLoadingScreen();

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

        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        newPlayer.name = "Player " + newPlayer.GetComponent<PhotonView>().CreatorActorNr;
        DontDestroyOnLoad(newPlayer);

        if (PhotonNetwork.IsMasterClient) {

            PhotonNetwork.Instantiate(networkManagerPrefab.name, Vector3.zero, Quaternion.identity);

        }

        gameManager.UpdateGameState(GameManager.GameState.Waiting);

        if (PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers()) {

            if (PhotonNetwork.IsMasterClient) {

                PhotonNetwork.LoadLevel(nextSceneName);

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

    public void StartFadeInLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f);
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, loadingFadeOpacity), true));

    }

    public void StartFadeOutLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        SetLoadingText("");
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f), false));

    }

    private IEnumerator FadeLoadingScreen(Color startColor, Color targetColor, bool fadeIn) {

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

    private void OpenRoomList() {

        if (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) {

            return;

        }

        //RoomOptions roomOptions = new RoomOptions();
        //roomOptions.MaxPlayers = gameManager.GetMaxPlayers();
        //PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        //StartFadeOutMenuHUD(0f);

        StartFadeOutMenuHUD(0f);
        StartFadeInRoomList();

    }

    private void CloseRoomList() {

        StartFadeOutRoomList(0f);
        StartFadeInMenuHUD();

    }

    public void StartFadeInRoomList() {

        if (roomListHUDFadeCoroutine != null) {

            StopCoroutine(roomListHUDFadeCoroutine);

        }

        roomListHUDFadeCoroutine = StartCoroutine(FadeRoomList(roomListHUD.alpha, 1f, true));

    }

    public void StartFadeOutRoomList(float targetOpacity) {

        if (roomListHUDFadeCoroutine != null) {

            StopCoroutine(roomListHUDFadeCoroutine);

        }

        roomListHUDFadeCoroutine = StartCoroutine(FadeRoomList(roomListHUD.alpha, targetOpacity, false));

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

    private void OpenCreateRoomHUD() {

        StartFadeOutRoomList(0f);
        StartFadeInCreateRoomHUD();

    }

    private void CloseCreateRoomHUD() {

        StartFadeOutCreateRoomHUD(0f);
        StartFadeInRoomList();

    }

    public void StartFadeInCreateRoomHUD() {

        if (createRoomHUDFadeCoroutine != null) {

            StopCoroutine(createRoomHUDFadeCoroutine);

        }

        createRoomHUDFadeCoroutine = StartCoroutine(FadeCreateRoomHUD(createRoomHUD.alpha, 1f, true));

    }

    public void StartFadeOutCreateRoomHUD(float targetOpacity) {

        if (createRoomHUDFadeCoroutine != null) {

            StopCoroutine(createRoomHUDFadeCoroutine);

        }

        createRoomHUDFadeCoroutine = StartCoroutine(FadeCreateRoomHUD(createRoomHUD.alpha, targetOpacity, false));

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

    public void StartFadeInMenuHUD() {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, 1f, true));

    }

    public void StartFadeOutMenuHUD(float targetOpacity) {

        if (menuHUDFadeCoroutine != null) {

            StopCoroutine(menuHUDFadeCoroutine);

        }

        menuHUDFadeCoroutine = StartCoroutine(FadeMenuHUD(menuHUD.alpha, targetOpacity, false));

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

    private void QuitGame() {

        Application.Quit();

    }

    private void CreateTestGame() {

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = gameManager.GetMaxPlayers();
        PhotonNetwork.CreateRoom("TestGame", roomOptions);
        StartFadeInLoadingScreen();

    }
}
