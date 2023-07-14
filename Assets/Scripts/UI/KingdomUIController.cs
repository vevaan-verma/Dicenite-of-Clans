using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class KingdomUIController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private PlaceableObjectDatabase placeableObjectDatabase;
    [SerializeField] private Material waterMaterial;
    private PlayerController playerController;
    private PlayerData playerData;

    [Header("UI References")]
    [SerializeField] private CanvasGroup kingdomHUD;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Button storeButton;
    [SerializeField] private CanvasGroup storeHUD;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text brickText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] private Button storeCloseButton;
    [SerializeField] private Transform storeContent;
    [SerializeField] private GameObject storeItemTemplate;
    [SerializeField] private Button diceButton;

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
    [SerializeField] private string nextSceneName;
    [SerializeField] private float loadingFadeDuration;
    [SerializeField] private float loadingFadeOpacity;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        foreach (PlayerController playerController in FindObjectsOfType<PlayerController>()) {

            if (playerController.photonView.IsMine) {

                this.playerController = playerController;

            }
        }

        playerData = FindObjectOfType<PlayerData>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        loadingScreen.gameObject.SetActive(true);

        storeButton.onClick.AddListener(OpenStoreHUD);
        storeCloseButton.onClick.AddListener(CloseStoreHUD);

        storeHUD.alpha = 0f;
        storeHUD.gameObject.SetActive(false);

        diceButton.onClick.AddListener(LoadDiceScene);

        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

        foreach (ObjectData objectData in placeableObjectDatabase.objectData) {

            Instantiate(storeItemTemplate, storeContent).GetComponent<StoreItemButton>().InitializeButton(objectData);

        }

        if (!playerController.photonView.Owner.CustomProperties.ContainsKey("ReadyStart")) {

            playerController.ReadyPlayer();

        } else {

            StartFadeOutLoadingScreen();

        }

        waterMaterial.EnableKeyword("_SHADERACTIVE");

    }

    private void LoadDiceScene() {

        StartFadeOutKingdomHUD(0f);

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

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f), false, ""));

    }

    private IEnumerator FadeLoadingScreen(Color startColor, Color targetColor, bool loadScene, string sceneName) {

        float currentTime = 0f;
        loadingScreen.gameObject.SetActive(true);

        while (currentTime < loadingFadeDuration) {

            currentTime += Time.deltaTime;
            loadingScreen.color = Color.Lerp(startColor, targetColor, currentTime / loadingFadeDuration);
            yield return null;

        }

        loadingScreen.color = targetColor;
        loadingFadeCoroutine = null;

        if (loadScene) {

            SceneManager.LoadSceneAsync(sceneName);

        } else {

            loadingScreen.gameObject.SetActive(false);

        }
    }

    private void OpenStoreHUD() {

        storeButton.GetComponent<SlidingButton>().DisableSlideIn();
        diceButton.GetComponent<SlidingButton>().DisableSlideIn();

        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

        StartFadeInStoreHud();

    }

    public void CloseStoreHUD() {

        storeButton.GetComponent<SlidingButton>().EnableSlideIn();
        diceButton.GetComponent<SlidingButton>().EnableSlideIn();
        StartFadeOutStoreHUD();

    }

    public void StartFadeInKingdomHUD() {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        fadeKingdomHUDCoroutine = StartCoroutine(FadeKingdomHUD(kingdomHUD.alpha, 1f, true));

    }

    public void StartFadeOutKingdomHUD(float targetOpacity) {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        storeButton.interactable = false;
        diceButton.interactable = false;
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
            diceButton.interactable = true;

        }
    }

    public void StartFadeInStoreHud() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeHUD.gameObject.SetActive(true);
        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, storeOpacity, true));

    }

    public void StartFadeOutStoreHUD() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, 0f, false));

    }

    private IEnumerator FadeStoreHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;

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
}
