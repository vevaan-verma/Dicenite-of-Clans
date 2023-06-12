using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KingdomUIController : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private CanvasGroup kingdomHUD;
    [SerializeField] private Image kingdomLoadingPanel;
    [SerializeField] private Button storeButton;
    [SerializeField] private CanvasGroup storeHUD;
    [SerializeField] private Button storeCloseButton;

    [Header("Animations")]
    [SerializeField] private float kingdomFadeDuration;
    [SerializeField] private float storeFadeDuration;
    [SerializeField][Range(0f, 1f)] private float storeOpacity;
    private Coroutine kingdomFadeCoroutine;
    private Coroutine fadeKingdomHUDCoroutine;
    private Coroutine storeFadeCoroutine;

    private void Start() {

        if (kingdomFadeCoroutine != null) {

            StopCoroutine(kingdomFadeCoroutine);

        }

        kingdomLoadingPanel.color = new Color(kingdomLoadingPanel.color.r, kingdomLoadingPanel.color.g, kingdomLoadingPanel.color.b, 1f);

        kingdomFadeCoroutine = StartCoroutine(FadeLoadingScreen(kingdomLoadingPanel.color, new Color(kingdomLoadingPanel.color.r, kingdomLoadingPanel.color.g, kingdomLoadingPanel.color.b, 0f)));

        storeButton.onClick.AddListener(OpenStoreHUD);
        storeCloseButton.onClick.AddListener(CloseStoreHUD);

        storeHUD.alpha = 0f;
        storeHUD.gameObject.SetActive(false);

    }

    private IEnumerator FadeLoadingScreen(Color startColor, Color targetColor) {

        float currentTime = 0f;
        kingdomLoadingPanel.gameObject.SetActive(true);

        while (currentTime < kingdomFadeDuration) {

            currentTime += Time.deltaTime;
            kingdomLoadingPanel.color = Color.Lerp(startColor, targetColor, currentTime / kingdomFadeDuration);
            yield return null;

        }

        kingdomLoadingPanel.color = targetColor;
        kingdomLoadingPanel.gameObject.SetActive(false);
        kingdomFadeCoroutine = null;

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
