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
    [SerializeField] private float fadeDuration;
    [SerializeField][Range(0f, 1f)] private float fadeOpacity;
    private Coroutine fadeDiceHUDCoroutine;
    private Coroutine healthLerpCoroutine;
    private Coroutine woodLerpCoroutine;
    private Coroutine brickLerpCoroutine;
    private Coroutine metalLerpCoroutine;

    [Header("Kingdom Scene")]
    [SerializeField] private string kingdomSceneName;
    [SerializeField] private float kingdomFadeOpacity;
    [SerializeField] private float kingdomFadeDuration;
    [SerializeField] private Image kingdomLoadingPanel;
    private Coroutine kingdomFadeCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        playerData = FindObjectOfType<PlayerData>();
        diceRollers = FindObjectsOfType<DiceRoller>();

        healthSlider.maxValue = playerData.GetMaxHealth();
        UpdateHealthSlider(playerData.GetMaxHealth());

        buildButton.GetComponentInChildren<TMP_Text>().text = "Build x" + gameManager.GetBuildersDice();
        buildButton.onClick.AddListener(RollBuildersDice);

        attackButton.GetComponentInChildren<TMP_Text>().text = "Attack x" + gameManager.GetAttackDice();
        attackButton.onClick.AddListener(RollAttackDice);

        kingdomButton.onClick.AddListener(LoadKingdomScene);

        kingdomLoadingPanel.color = new Color(kingdomLoadingPanel.color.r, kingdomLoadingPanel.color.g, kingdomLoadingPanel.color.b, 0f);

    }

    private void RollBuildersDice() {

        DisableRollButtons();

        rollersLeft = new List<DiceRoller>();

        foreach (DiceRoller roller in diceRollers) {

            rollersLeft.Add(roller);

        }

        gameManager.ClearAllDice();
        StartFadeOutDiceHUD(fadeOpacity);

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
        StartFadeOutDiceHUD(fadeOpacity);

        int randInt;

        for (int i = 0; i < gameManager.GetAttackDice(); i++) {

            randInt = Random.Range(0, rollersLeft.Count - 1);
            rollersLeft[randInt].RollAttackDice();
            rollersLeft.RemoveAt(randInt);

        }
    }

    private void LoadKingdomScene() {

        StartFadeOutDiceHUD(0f);

        if (kingdomFadeCoroutine != null) {

            StopCoroutine(kingdomFadeCoroutine);

        }

        kingdomLoadingPanel.color = new Color(kingdomLoadingPanel.color.r, kingdomLoadingPanel.color.g, kingdomLoadingPanel.color.b, 0f);

        kingdomFadeCoroutine = StartCoroutine(FadeLoadingScreen(kingdomLoadingPanel.color, new Color(kingdomLoadingPanel.color.r, kingdomLoadingPanel.color.g, kingdomLoadingPanel.color.b, kingdomFadeOpacity)));

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
        kingdomFadeCoroutine = null;

        SceneManager.LoadSceneAsync(kingdomSceneName);

    }

    public void UpdateHealthSlider(int health) {

        if (healthLerpCoroutine != null) {

            StopCoroutine(healthLerpCoroutine);

        }

        healthLerpCoroutine = StartCoroutine(LerpHealthSlider((int) healthSlider.value, health));

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

    public void UpdateWoodCount(int wood) {

        if (woodLerpCoroutine != null) {

            StopCoroutine(woodLerpCoroutine);

        }

        int.TryParse(woodText.text, out int woodCount);
        woodLerpCoroutine = StartCoroutine(LerpWoodCount(woodCount, wood));

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

    public void UpdateBrickCount(int brick) {

        if (brickLerpCoroutine != null) {

            StopCoroutine(brickLerpCoroutine);

        }

        int.TryParse(brickText.text, out int brickCount);
        brickLerpCoroutine = StartCoroutine(LerpBrickCount(brickCount, brick));

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

    public void UpdateMetalCount(int metal) {

        if (metalLerpCoroutine != null) {

            StopCoroutine(metalLerpCoroutine);

        }

        int.TryParse(metalText.text, out int metalCount);
        metalLerpCoroutine = StartCoroutine(LerpMetalCount(metalCount, metal));

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

        if (fadeDiceHUDCoroutine != null) {

            StopCoroutine(fadeDiceHUDCoroutine);

        }

        fadeDiceHUDCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, 1f));

    }

    public void StartFadeOutDiceHUD(float targetOpacity) {

        if (fadeDiceHUDCoroutine != null) {

            StopCoroutine(fadeDiceHUDCoroutine);

        }

        fadeDiceHUDCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, targetOpacity));

    }

    private IEnumerator FadeDiceHUD(float startOpacity, float targetOpacity) {

        float currentTime = 0f;

        while (currentTime < fadeDuration) {

            currentTime += Time.deltaTime;
            diceHUD.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / fadeDuration);
            yield return null;

        }

        diceHUD.alpha = targetOpacity;
        fadeDiceHUDCoroutine = null;

    }
}
