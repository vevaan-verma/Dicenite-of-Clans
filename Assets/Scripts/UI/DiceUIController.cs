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

public class DiceUIController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private List<DiceRoller> diceRollers;
    private PhotonView photonView;
    private GameManager gameManager;
    private PlayerData playerData;
    private List<DiceRoller> rollersLeft;

    [Header("Dice UI")]
    [SerializeField] private CanvasGroup diceHUD;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text brickText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] private Button buildButton;
    [SerializeField] private Button attackButton;

    [Header("Testing UI")]
    [SerializeField] private CanvasGroup testingHUD;
    [SerializeField] private GameObject testBuildRollButton;
    [SerializeField] private GameObject testAttackRollButton;
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject declineButton;

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingScreen;

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

        foreach (NetworkManager networkManager in FindObjectsOfType<NetworkManager>()) {

            if (networkManager.photonView.IsMine) {

                photonView = networkManager.photonView;

            }
        }

        gameManager = FindObjectOfType<GameManager>();
        playerData = FindObjectOfType<PlayerData>();

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        CloseLoadingScreen();

        healthSlider.maxValue = playerData.GetMaxHealth();
        UpdateHealthSlider();

        buildButton.GetComponentInChildren<TMP_Text>().text = "Build x" + gameManager.GetBuildDiceAmount();
        buildButton.onClick.AddListener(RollBuildDice);
        buildButton.interactable = false;

        attackButton.GetComponentInChildren<TMP_Text>().text = "Attack x" + gameManager.GetAttackDiceAmount();
        attackButton.onClick.AddListener(RollAttackDice);
        attackButton.interactable = false;

        if (gameManager.GetMaxPlayers() > 1) {

            buildButton.interactable = false;
            attackButton.interactable = false;

        } else {

            buildButton.interactable = true;
            attackButton.interactable = true;

        }

        TurnChanged();

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

    public void TurnChanged() {

        if (((int) PhotonNetwork.CurrentRoom.CustomProperties["Turn"]) == photonView.OwnerActorNr) {

            buildButton.interactable = true;
            attackButton.interactable = true;

        } else {

            buildButton.interactable = false;
            attackButton.interactable = false;

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

        gameManager.ClearAllDice();

        int rollIndex = UnityEngine.Random.Range(0, importedBuildRollData.rollData.Count);
        DiceRotation rotation;

        for (int i = 0; i < gameManager.GetBuildDiceAmount(); i++) {

            rotation = importedBuildRollData.rollData[rollIndex][i].GetDiceRotation();
            diceRollers[importedBuildRollData.rollData[rollIndex][i].GetDiceRoller()].photonView.RPC("RollBuildDice", RpcTarget.MasterClient, new Quaternion(rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW()), importedBuildRollData.rollData[rollIndex][i].GetDiceVelocity());

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

        gameManager.ClearAllDice();

        int rollIndex = UnityEngine.Random.Range(0, importedAttackRollData.rollData.Count);
        DiceRotation rotation;

        for (int i = 0; i < gameManager.GetAttackDiceAmount(); i++) {

            rotation = importedAttackRollData.rollData[rollIndex][i].GetDiceRotation();
            diceRollers[importedAttackRollData.rollData[rollIndex][i].GetDiceRoller()].photonView.RPC("RollAttackDice", RpcTarget.MasterClient, new Quaternion(rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW()), importedAttackRollData.rollData[rollIndex][i].GetDiceVelocity());

        }
    }

    public void LoadKingdomScene() {

        PhotonNetwork.LoadLevel(kingdomSceneName);
        OpenLoadingScreen();

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

    private void EnableRollButtons() {

        buildButton.interactable = true;
        attackButton.interactable = true;

    }

    private void DisableRollButtons() {

        buildButton.interactable = false;
        attackButton.interactable = false;

    }

    public void StartFadeInDiceHUD() {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

        diceHUDFadeCoroutine = StartCoroutine(FadeDiceHUD(diceHUD.alpha, 1f, true));

    }

    private void StartFadeOutDiceHUD(float targetOpacity) {

        if (diceHUDFadeCoroutine != null) {

            StopCoroutine(diceHUDFadeCoroutine);

        }

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

    }

    private void OpenLoadingScreen() {

        if (loadingFadeCoroutine != null) {

            StopCoroutine(loadingFadeCoroutine);

        }

        StartFadeOutDiceHUD(0f);
        loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(loadingScreen.alpha, 1f, true));

    }

    private void CloseLoadingScreen() {

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

    public bool GetTestingModeState() {

        return testingModeEnabled;

    }

    private void ToggleTestingMode() {

        gameManager.ClearAllDice();

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

        gameManager.ClearAllDice();

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

        gameManager.ClearAllDice();

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

        gameManager.ClearAllDice();

        acceptButton.GetComponent<Button>().onClick.RemoveListener(AcceptBuildTestingRoll);
        acceptButton.SetActive(false);
        declineButton.SetActive(false);
        testBuildRollButton.SetActive(true);
        testAttackRollButton.SetActive(true);

    }

    private void AcceptAttackTestingRoll() {

        newAttackRollData.rollData.Add(currDiceRollData);

        gameManager.ClearAllDice();

        acceptButton.GetComponent<Button>().onClick.RemoveListener(AcceptAttackTestingRoll);
        acceptButton.SetActive(false);
        declineButton.SetActive(false);
        testBuildRollButton.SetActive(true);
        testAttackRollButton.SetActive(true);

    }

    private void DeclineTestingRoll() {

        gameManager.ClearAllDice();

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
