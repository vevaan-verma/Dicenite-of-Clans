using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KingdomUIController : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private CanvasGroup kingdomHUD;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Button storeButton;
    [SerializeField] private CanvasGroup storeHUD;
    [SerializeField] private Button storeCloseButton;
    [SerializeField] private Button diceButton;

    [Header("Animations")]
    [SerializeField] private float kingdomFadeDuration;
    [SerializeField] private float storeFadeDuration;
    [SerializeField][Range(0f, 1f)] private float storeOpacity;
    private Coroutine fadeKingdomHUDCoroutine;
    private Coroutine storeFadeCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string diceSceneName;
    [SerializeField] private float loadingFadeDuration;
    [SerializeField] private float loadingFadeOpacity;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        StartFadeOutLoadingScreen(new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f));

        storeButton.onClick.AddListener(OpenStoreHUD);
        storeCloseButton.onClick.AddListener(CloseStoreHUD);

        storeHUD.alpha = 0f;
        storeHUD.gameObject.SetActive(false);

        diceButton.onClick.AddListener(LoadDiceScene);

    }

    private void LoadDiceScene() {

        StartFadeOutKingdomHUD(0f);

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f);

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, loadingFadeOpacity), true, diceSceneName));

    }

    private void StartFadeOutLoadingScreen(Color targetColor) {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, targetColor, false, ""));

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

        StartFadeOutKingdomHUD(0f);
        StartFadeInStoreHud();

    }

    private void CloseStoreHUD() {

        StartFadeOutStoreHUD();
        StartFadeInKingdomHud();

    }

    public void StartFadeInKingdomHud() {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        fadeKingdomHUDCoroutine = StartCoroutine(FadeKingdomHUD(kingdomHUD.alpha, 1f));

    }

    public void StartFadeOutKingdomHUD(float targetOpacity) {

        if (fadeKingdomHUDCoroutine != null) {

            StopCoroutine(fadeKingdomHUDCoroutine);

        }

        fadeKingdomHUDCoroutine = StartCoroutine(FadeKingdomHUD(kingdomHUD.alpha, targetOpacity));

    }

    private IEnumerator FadeKingdomHUD(float startOpacity, float targetOpacity) {

        float currentTime = 0f;

        while (currentTime < kingdomFadeDuration) {

            currentTime += Time.deltaTime;
            kingdomHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / kingdomFadeDuration);
            yield return null;

        }

        kingdomHUD.alpha = targetOpacity;
        fadeKingdomHUDCoroutine = null;

    }

    public void StartFadeInStoreHud() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeHUD.gameObject.SetActive(true);
        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, storeOpacity));

    }

    public void StartFadeOutStoreHUD() {

        if (storeFadeCoroutine != null) {

            StopCoroutine(storeFadeCoroutine);

        }

        storeFadeCoroutine = StartCoroutine(FadeStoreHUD(storeHUD.alpha, 0f));

    }

    private IEnumerator FadeStoreHUD(float startOpacity, float targetOpacity) {

        float currentTime = 0f;

        while (currentTime < storeFadeDuration) {

            currentTime += Time.deltaTime;
            storeHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / storeFadeDuration);
            yield return null;

        }

        storeHUD.alpha = targetOpacity;
        storeFadeCoroutine = null;

    }
}
