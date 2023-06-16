using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DiceUIController : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;
    private PlayerData playerData;
    private DiceRoller[] diceRollers;
    private List<DiceRoller> rollersLeft;

    [Header("UI References")]
    [SerializeField] private CanvasGroup diceHUD;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text brickText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] private Button buildButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button kingdomButton;

    [Header("Animations")]
    [SerializeField] private float healthLerpDuration;
    [SerializeField] private float materialLerpDuration;
    [SerializeField] private float diceHUDFadeDuration;
    [SerializeField][Range(0f, 1f)] private float diceHUDFadeOpacity;
    private Coroutine diceHUDFadeCoroutine;
    private Coroutine healthLerpCoroutine;
    private Coroutine woodLerpCoroutine;
    private Coroutine brickLerpCoroutine;
    private Coroutine metalLerpCoroutine;

    [Header("Scene Transitions")]
    [SerializeField] private string kingdomSceneName;
    [SerializeField] private float loadingFadeDuration;
    [SerializeField] private float loadingFadeOpacity;
    private Coroutine loadingFadeCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        playerData = FindObjectOfType<PlayerData>();
        diceRollers = FindObjectsOfType<DiceRoller>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        StartFadeOutLoadingScreen(new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f));

        healthSlider.maxValue = playerData.GetMaxHealth();
        UpdateHealthSlider();

        buildButton.GetComponentInChildren<TMP_Text>().text = "Build x" + gameManager.GetBuildersDice();
        buildButton.onClick.AddListener(RollBuildersDice);

        attackButton.GetComponentInChildren<TMP_Text>().text = "Attack x" + gameManager.GetAttackDice();
        attackButton.onClick.AddListener(RollAttackDice);

        kingdomButton.onClick.AddListener(LoadKingdomScene);

        UpdateHealthSlider();
        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

    }

    private void RollBuildersDice() {

        DisableRollButtons();

        rollersLeft = new List<DiceRoller>();

        foreach (DiceRoller roller in diceRollers) {

            rollersLeft.Add(roller);

        }

        gameManager.ClearAllDice();
        StartFadeOutDiceHUD(diceHUDFadeOpacity);

        int randInt;

        for (int i = 0; i < gameManager.GetBuildersDice(); i++) {

            randInt = Random.Range(0, rollersLeft.Count - 1);
            rollersLeft[randInt].RollBuildersDice();
            rollersLeft.RemoveAt(randInt);

        }
    }

    private void RollAttackDice() {

        DisableRollButtons();

        rollersLeft = new List<DiceRoller>();

        foreach (DiceRoller roller in diceRollers) {

            rollersLeft.Add(roller);

        }

        gameManager.ClearAllDice();
        StartFadeOutDiceHUD(diceHUDFadeOpacity);

        int randInt;

        for (int i = 0; i < gameManager.GetAttackDice(); i++) {

            randInt = Random.Range(0, rollersLeft.Count - 1);
            rollersLeft[randInt].RollAttackDice();
            rollersLeft.RemoveAt(randInt);

        }
    }

    private void LoadKingdomScene() {

        StartFadeOutDiceHUD(0f);

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f);

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, loadingFadeOpacity), true, kingdomSceneName));

    }

    protected void StartFadeOutLoadingScreen(Color targetColor) {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, targetColor, false, ""));

    }

    private IEnumerator FadeLoadingScreen(Color startColor, Color targetColor, bool fadeIn, string sceneName) {

        float currentTime = 0f;
        loadingScreen.gameObject.SetActive(true);

        while (currentTime < loadingFadeDuration) {

            currentTime += Time.deltaTime;
            loadingScreen.color = Color.Lerp(startColor, targetColor, currentTime / loadingFadeDuration);
            yield return null;

        }

        loadingScreen.color = targetColor;
        loadingFadeCoroutine = null;

        if (fadeIn) {

            SceneManager.LoadSceneAsync(sceneName);

        } else {

            loadingScreen.gameObject.SetActive(false);

        }
    }

    public void UpdateHealthSlider() {

        if (healthLerpCoroutine != null) {

            StopCoroutine(healthLerpCoroutine);

        }

        healthLerpCoroutine = StartCoroutine(LerpHealthSlider((int) healthSlider.value, playerData.GetHealth()));

    }

    private IEnumerator LerpHealthSlider(int startHealth, int targetHealth) {

        float currentTime = 0f;

        while (currentTime < healthLerpDuration) {

            currentTime += Time.deltaTime;
            healthSlider.value = Mathf.Lerp(startHealth, targetHealth, currentTime / healthLerpDuration);
            healthText.text = healthSlider.value + "";
            yield return null;

        }

        healthSlider.value = targetHealth;
        healthText.text = healthSlider.value + "";
        healthLerpCoroutine = null;

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

    public void EnableRollButtons() {

        buildButton.interactable = true;
        attackButton.interactable = true;

    }

    public void DisableRollButtons() {

        buildButton.interactable = false;
        attackButton.interactable = false;

    }

    public void StartFadeInDiceHud() {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

        diceHUDFadeCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, 1f, true));

    }

    public void StartFadeOutDiceHUD(float targetOpacity) {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

        kingdomButton.GetComponent<SlideUIButton>().DisableSlideIn();
        diceHUDFadeCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, targetOpacity, false));

    }

    private IEnumerator FadeDiceHUD(float startOpacity, float targetOpacity, bool fadeIn) {

        float currentTime = 0f;

        while (currentTime < diceHUDFadeDuration) {

            currentTime += Time.deltaTime;
            diceHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / diceHUDFadeDuration);
            yield return null;

        }

        diceHUD.alpha = targetOpacity;
        diceHUDFadeCoroutine = null;

        if (fadeIn) {

            kingdomButton.GetComponent<SlideUIButton>().EnableSlideIn();

        }
    }
}
