using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class KingdomUIController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private PlaceableObjectDatabase placeableObjectDatabase;
    private NetworkManager networkManager;
    private PlayerData playerData;

    [Header("Kingdom UI")]
    [SerializeField] private CanvasGroup kingdomHUD;
    [SerializeField] private Button storeButton;
    [SerializeField] private TMP_Text countdownText;

    [Header("Store UI")]
    [SerializeField] private CanvasGroup storeHUD;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text brickText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] private Button storeCloseButton;
    [SerializeField] private Transform storeContent;
    [SerializeField] private GameObject storeItemTemplate;

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingScreen;

    [Header("Animations")]
    [SerializeField] private float kingdomHUDFadeDuration;
    [SerializeField] private float materialLerpDuration;
    [SerializeField] private float storeFadeDuration;
    [SerializeField][Range(0f, 1f)] private float storeOpacity;
    private Coroutine fadeKingdomHUDCoroutine;
    private Coroutine storeFadeCoroutine;
    private Coroutine woodLerpCoroutine;
    private Coroutine brickLerpCoroutine;
    private Coroutine metalLerpCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string diceSceneName;
    [SerializeField] private float loadingFadeDuration;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        foreach (NetworkManager networkManager in FindObjectsOfType<NetworkManager>()) {

            if (networkManager.photonView.IsMine) {

                this.networkManager = networkManager;

            }
        }

        playerData = FindObjectOfType<PlayerData>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.gameObject.SetActive(true);

        storeButton.onClick.AddListener(OpenStoreHUD);
        storeCloseButton.onClick.AddListener(CloseStoreHUD);

        storeHUD.alpha = 0f;
        storeHUD.gameObject.SetActive(false);

        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

        foreach (ObjectData objectData in placeableObjectDatabase.objectData) {

            Instantiate(storeItemTemplate, storeContent).GetComponent<StoreItemButton>().InitializeButton(objectData);

        }

        if (!networkManager.photonView.Owner.CustomProperties.ContainsKey("Loaded")) {

            networkManager.SetPlayerProperty(networkManager.photonView, NetworkManager.PlayerProperty.Loaded, true);

        } else {

            CloseLoadingScreen();

        }
    }

    public void SetCountdownText(string text) {

        countdownText.text = text;

    }

    public void LoadDiceScene() {

        PhotonNetwork.LoadLevel(diceSceneName);
        OpenLoadingScreen();

    }

    public void OpenKingdomHUD() {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        fadeKingdomHUDCoroutine = StartCoroutine(FadeKingdomHUD(kingdomHUD.alpha, 1f, true));

    }

    public void CloseKingdomHUD(float targetOpacity) {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        storeButton.interactable = false;
        fadeKingdomHUDCoroutine = StartCoroutine(FadeKingdomHUD(kingdomHUD.alpha, targetOpacity, false));

    }

    private IEnumerator FadeKingdomHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;

        while (currentTime < kingdomHUDFadeDuration) {

            currentTime += Time.deltaTime;
            kingdomHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / kingdomHUDFadeDuration);
            yield return null;

        }

        kingdomHUD.alpha = targetOpacity;
        fadeKingdomHUDCoroutine = null;

        if (fadeIn) {

            storeButton.interactable = true;

        }
    }

    private void OpenStoreHUD() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeButton.GetComponent<SlidingButton>().DisableSlideIn();

        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, storeOpacity, true));

    }

    public void CloseStoreHUD() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeButton.GetComponent<SlidingButton>().EnableSlideIn();
        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, 0f, false));

    }

    private IEnumerator FadeStoreHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;
        storeHUD.gameObject.SetActive(true);

        while (currentTime < storeFadeDuration) {

            currentTime += Time.deltaTime;
            storeHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / storeFadeDuration);
            yield return null;

        }

        storeHUD.alpha = targetOpacity;
        storeFadeCoroutine = null;

        if (!fadeIn) {

            storeHUD.gameObject.SetActive(false);

        }
    }

    public void UpdateWoodCount() {

        if (woodLerpCoroutine != null) {

            StopCoroutine(woodLerpCoroutine);

        }

        int.TryParse(woodText.text, out int woodCount);
        woodLerpCoroutine = StartCoroutine(LerpWoodCount(woodCount, playerData.GetWoodCount()));

    }

    private IEnumerator LerpWoodCount(int startWood, int targetWood) {

        float currentTime = 0f;

        while (currentTime < materialLerpDuration) {

            currentTime += Time.deltaTime;
            woodText.text = (int) Mathf.Lerp(startWood, targetWood, currentTime / materialLerpDuration) + "";
            yield return null;

        }

        woodText.text = targetWood + "";
        woodLerpCoroutine = null;

    }

    public void UpdateBrickCount() {

        if (brickLerpCoroutine != null) {

            StopCoroutine(brickLerpCoroutine);

        }

        int.TryParse(brickText.text, out int brickCount);
        brickLerpCoroutine = StartCoroutine(LerpBrickCount(brickCount, playerData.GetBrickCount()));

    }

    private IEnumerator LerpBrickCount(int startBrick, int targetBrick) {

        float currentTime = 0f;

        while (currentTime < materialLerpDuration) {

            currentTime += Time.deltaTime;
            brickText.text = (int) Mathf.Lerp(startBrick, targetBrick, currentTime / materialLerpDuration) + "";
            yield return null;

        }

        brickText.text = targetBrick + "";
        brickLerpCoroutine = null;

    }

    public void UpdateMetalCount() {

        if (metalLerpCoroutine != null) {

            StopCoroutine(metalLerpCoroutine);

        }

        int.TryParse(metalText.text, out int metalCount);
        metalLerpCoroutine = StartCoroutine(LerpMetalCount(metalCount, playerData.GetMetalCount()));

    }

    private IEnumerator LerpMetalCount(int startMetal, int targetMetal) {

        float currentTime = 0f;

        while (currentTime < materialLerpDuration) {

            currentTime += Time.deltaTime;
            metalText.text = (int) Mathf.Lerp(startMetal, targetMetal, currentTime / materialLerpDuration) + "";
            yield return null;

        }

        metalText.text = targetMetal + "";
        metalLerpCoroutine = null;

    }

    private void OpenLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        CloseKingdomHUD(0f);
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.alpha, 1f, true));

    }

    public void CloseLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

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
}
