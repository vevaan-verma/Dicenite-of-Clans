using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Newtonsoft.Json;
using Photon.Pun;

public class DiceUIController : MonoBehaviourPunCallbacks {

    [Header("References")]
    [SerializeField] private List<DiceRoller> diceRollers;
    private PlayerController playerController;
    private GameManager gameManager;
    private PlayerData playerData;
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
    [SerializeField] private CanvasGroup testingHUD;
    [SerializeField] private GameObject testBuildRollButton;
    [SerializeField] private GameObject testAttackRollButton;
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject declineButton;

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

    [Header("Testing Mode")]
    private BuildRollRootObject importedBuildRollData;
    private BuildRollRootObject newBuildRollData;
    private AttackRollRootObject importedAttackRollData;
    private AttackRollRootObject newAttackRollData;
    private List<RollData> currDiceRollData;
    private bool testingModeEnabled;

    public event Action OnTestingModeToggle;

    private void Start() {

        foreach (PlayerController playerController in FindObjectsOfType<PlayerController>()) {

            if (playerController.photonView.IsMine) {

                this.playerController = playerController;

            }
        }

        gameManager = FindObjectOfType<GameManager>();
        playerData = FindObjectOfType<PlayerData>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingScreen.color = new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 1f);
        StartFadeOutLoadingScreen();

        healthSlider.maxValue = playerData.GetMaxHealth();
        UpdateHealthSlider();

        buildButton.GetComponentInChildren<TMP_Text>().text = "Build x" + gameManager.GetBuildDiceAmount();
        buildButton.onClick.AddListener(RollBuildDice);

        attackButton.GetComponentInChildren<TMP_Text>().text = "Attack x" + gameManager.GetAttackDiceAmount();
        attackButton.onClick.AddListener(RollAttackDice);

        if (gameManager.GetMaxPlayers() > 1) {

            buildButton.interactable = false;
            attackButton.interactable = false;

        } else {

            buildButton.interactable = true;
            attackButton.interactable = true;

        }

        kingdomButton.onClick.AddListener(LoadKingdomScene);

        gameManager.OnTurnChange += ChangeTurn;

        UpdateHealthSlider();
        UpdateWoodCount();
        UpdateBrickCount();
        UpdateMetalCount();

        OnTestingModeToggle += ToggleTestingMode;

        testingHUD.gameObject.SetActive(false);

        testBuildRollButton.GetComponent<Button>().onClick.AddListener(RollTestingBuildDice);
        testAttackRollButton.GetComponent<Button>().onClick.AddListener(RollTestingAttackDice);
        declineButton.GetComponent<Button>().onClick.AddListener(DeclineTestingRoll);

        if (!File.Exists(gameManager.GetBuildDiceRollFilePath())) {

            File.Create(gameManager.GetBuildDiceRollFilePath()).Close();

        }

        if (!File.Exists(gameManager.GetAttackDiceRollFilePath())) {

            File.Create(gameManager.GetAttackDiceRollFilePath()).Close();

        }

        importedBuildRollData = new BuildRollRootObject();
        importedAttackRollData = new AttackRollRootObject();

        using (StreamReader sr = new StreamReader(gameManager.GetBuildDiceRollFilePath())) {

            if (Application.isEditor) {

                Debug.Log("File successfully opened at " + gameManager.GetBuildDiceRollFilePath());

            }

            if (sr.EndOfStream) {

                if (Application.isEditor) {

                    Debug.LogWarning("There are no build rolls to execute! Enter developer testing mode to add some!");

                }
            } else {

                importedBuildRollData = JsonConvert.DeserializeObject<BuildRollRootObject>(sr.ReadToEnd());

            }
        }

        using (StreamReader sr = new StreamReader(gameManager.GetAttackDiceRollFilePath())) {

            if (Application.isEditor) {

                Debug.Log("File successfully opened at " + gameManager.GetAttackDiceRollFilePath());

            }

            if (sr.EndOfStream) {

                if (Application.isEditor) {

                    Debug.LogWarning("There are no attack rolls to execute! Enter developer testing mode to add some!");

                }
            } else {

                importedAttackRollData = JsonConvert.DeserializeObject<AttackRollRootObject>(sr.ReadToEnd());

            }
        }

        newBuildRollData = new BuildRollRootObject();
        newAttackRollData = new AttackRollRootObject();

    }

    private void Update() {

        if ((Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.T)) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.T)) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) || (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.T)) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) || (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) && Application.isEditor) {

            OnTestingModeToggle?.Invoke();

        }
    }

    private void OnApplicationQuit() {

        if (testingModeEnabled && (importedBuildRollData.rollData.Count > 0 || newBuildRollData.rollData.Count > 0 || importedAttackRollData.rollData.Count > 0 || newAttackRollData.rollData.Count > 0)) {

            SaveRollData();

        }
    }

    private void ChangeTurn() {

        if (((int) PhotonNetwork.CurrentRoom.CustomProperties["Turn"]) == playerController.photonView.OwnerActorNr) {

            buildButton.interactable = true;
            attackButton.interactable = true;

        }
    }

    private void RollBuildDice() {

        if (importedBuildRollData.rollData.Count == 0) {

            if (Application.isEditor) {

                Debug.LogWarning("There are no build rolls to execute! Enter developer testing mode to add some!");
                return;

            }
        }

        if (gameManager.GetGameState() != GameManager.GameState.Live) {

            return;

        }

        DisableRollButtons();
        StartFadeOutDiceHUD(diceHUDFadeOpacity);

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        int rollIndex = UnityEngine.Random.Range(0, importedBuildRollData.rollData.Count);
        DiceRotation rotation;

        for (int i = 0; i < gameManager.GetBuildDiceAmount(); i++) {

            rotation = importedBuildRollData.rollData[rollIndex][i].GetDiceRotation();
            diceRollers[importedBuildRollData.rollData[rollIndex][i].GetDiceRoller()].photonView.RPC("RollBuildDiceRPC", RpcTarget.All, new Quaternion(rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW()), importedBuildRollData.rollData[rollIndex][i].GetDiceVelocity());

        }
    }

    private void RollAttackDice() {

        if (importedAttackRollData.rollData.Count == 0) {

            if (Application.isEditor) {

                Debug.LogWarning("There are no attack rolls to execute! Enter developer testing mode to add some!");
                return;

            }
        }

        if (gameManager.GetGameState() != GameManager.GameState.Live) {

            return;

        }

        DisableRollButtons();
        StartFadeOutDiceHUD(diceHUDFadeOpacity);

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        int rollIndex = UnityEngine.Random.Range(0, importedAttackRollData.rollData.Count);
        DiceRotation rotation;

        for (int i = 0; i < gameManager.GetAttackDiceAmount(); i++) {

            rotation = importedAttackRollData.rollData[rollIndex][i].GetDiceRotation();
            diceRollers[importedAttackRollData.rollData[rollIndex][i].GetDiceRoller()].photonView.RPC("RollAttackDiceRPC", RpcTarget.All, new Quaternion(rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW()), importedAttackRollData.rollData[rollIndex][i].GetDiceVelocity());

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

    public void StartFadeOutLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.color, new Color(loadingScreen.color.r, loadingScreen.color.g, loadingScreen.color.b, 0f), false, ""));

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

    public void StartFadeInDiceHUD() {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

        diceHUDFadeCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, 1f, true));

    }

    public void StartFadeOutDiceHUD(float targetOpacity) {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

        kingdomButton.GetComponent<SlidingButton>().DisableSlideIn();
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

            kingdomButton.GetComponent<SlidingButton>().EnableSlideIn();

        }
    }

    public bool GetTestingModeState() {

        return testingModeEnabled;

    }

    private void ToggleTestingMode() {

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        testingModeEnabled = !testingModeEnabled;

        if (testingModeEnabled) {

            Debug.LogWarning("Developer testing mode enabled!");
            diceHUD.gameObject.SetActive(false);
            testingHUD.gameObject.SetActive(true);
            testBuildRollButton.SetActive(true);
            acceptButton.SetActive(false);
            declineButton.SetActive(false);

        } else {

            SaveRollData();
            Debug.LogWarning("Developer testing mode disabled!");
            testingHUD.gameObject.SetActive(false);
            diceHUD.gameObject.SetActive(true);
            EnableRollButtons();
            StartFadeInDiceHUD();

        }
    }

    private void RollTestingBuildDice() {

        rollersLeft = new List<DiceRoller>();

        foreach (DiceRoller roller in diceRollers) {

            rollersLeft.Add(roller);

        }

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        currDiceRollData = new List<RollData>();
        int randInt;

        for (int i = 0; i < gameManager.GetBuildDiceAmount(); i++) {

            RollData rollData = new RollData();
            randInt = UnityEngine.Random.Range(0, rollersLeft.Count);
            rollData = rollersLeft[randInt].RollTestingBuildDice(rollData);
            currDiceRollData.Add(rollData);
            rollersLeft.RemoveAt(randInt);

        }

        testBuildRollButton.SetActive(false);
        testAttackRollButton.SetActive(false);
        acceptButton.GetComponent<Button>().onClick.AddListener(AcceptBuildTestingRoll);
        acceptButton.GetComponent<Button>().interactable = false;
        declineButton.GetComponent<Button>().interactable = false;
        acceptButton.SetActive(true);
        declineButton.SetActive(true);

    }

    private void RollTestingAttackDice() {

        rollersLeft = new List<DiceRoller>();

        foreach (DiceRoller roller in diceRollers) {

            rollersLeft.Add(roller);

        }

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        currDiceRollData = new List<RollData>();
        int randInt;

        for (int i = 0; i < gameManager.GetAttackDiceAmount(); i++) {

            RollData rollData = new RollData();
            randInt = UnityEngine.Random.Range(0, rollersLeft.Count);
            rollData = rollersLeft[randInt].RollTestingAttackDice(rollData);
            currDiceRollData.Add(rollData);
            rollersLeft.RemoveAt(randInt);

        }

        testBuildRollButton.SetActive(false);
        testAttackRollButton.SetActive(false);
        acceptButton.GetComponent<Button>().onClick.AddListener(AcceptAttackTestingRoll);
        acceptButton.GetComponent<Button>().interactable = false;
        declineButton.GetComponent<Button>().interactable = false;
        acceptButton.SetActive(true);
        declineButton.SetActive(true);

    }

    public void FinishTestingRolls() {

        acceptButton.GetComponent<Button>().interactable = true;
        declineButton.GetComponent<Button>().interactable = true;

    }

    private void AcceptBuildTestingRoll() {

        newBuildRollData.rollData.Add(currDiceRollData);

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        acceptButton.GetComponent<Button>().onClick.RemoveListener(AcceptBuildTestingRoll);
        acceptButton.SetActive(false);
        declineButton.SetActive(false);
        testBuildRollButton.SetActive(true);
        testAttackRollButton.SetActive(true);

    }

    private void AcceptAttackTestingRoll() {

        newAttackRollData.rollData.Add(currDiceRollData);

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        acceptButton.GetComponent<Button>().onClick.RemoveListener(AcceptAttackTestingRoll);
        acceptButton.SetActive(false);
        declineButton.SetActive(false);
        testBuildRollButton.SetActive(true);
        testAttackRollButton.SetActive(true);

    }

    private void DeclineTestingRoll() {

        playerController.photonView.RPC("ClearAllDiceRPC", RpcTarget.All);

        acceptButton.SetActive(false);
        declineButton.SetActive(false);
        testBuildRollButton.SetActive(true);
        testAttackRollButton.SetActive(true);

    }

    private void SaveRollData() {

        using (StreamWriter sw = new StreamWriter(gameManager.GetBuildDiceRollFilePath())) {

            if (Application.isEditor) {

                Debug.Log("Serializing build roll data to file!");

            }

            importedBuildRollData.rollData.AddRange(newBuildRollData.rollData);
            newBuildRollData.rollData.Clear();

            sw.Write(JsonConvert.SerializeObject(importedBuildRollData, Formatting.Indented, new JsonSerializerSettings {

                ReferenceLoopHandling = ReferenceLoopHandling.Ignore

            }));
        }

        using (StreamWriter sw = new StreamWriter(gameManager.GetAttackDiceRollFilePath())) {

            if (Application.isEditor) {

                Debug.Log("Serializing attack roll data to file!");

            }

            importedAttackRollData.rollData.AddRange(newAttackRollData.rollData);
            newAttackRollData.rollData.Clear();

            sw.Write(JsonConvert.SerializeObject(importedAttackRollData, Formatting.Indented, new JsonSerializerSettings {

                ReferenceLoopHandling = ReferenceLoopHandling.Ignore

            }));
        }
    }
}
