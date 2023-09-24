using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPun {

    [Header("References")]
    private GameManager gameManager;
    private GridPlacementController gridPlacementController;
    private GridData gridData;
    private KingdomUIController kingdomUIController;
    private bool startReady;
    private bool countdownReady;

    [Header("Scene Settings")]
    [SerializeField] private string diceSceneName;
    [SerializeField] private string kingdomSceneName;

    public enum UpdateType {

        Reset, Add

    }

    public enum PlayerProperty {

        Loaded, Spawned

    }

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

    }

    private void Update() {

        if (PhotonNetwork.IsMasterClient && photonView.IsMine && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers() && gameManager.GetGameState() == GameManager.GameState.Waiting && !startReady) {

            bool allLoaded = true;

            foreach (Player player in PhotonNetwork.PlayerList) {

                if (player.CustomProperties["Loaded"] == null) {

                    allLoaded = false;
                    break;

                }
            }

            if (allLoaded) {

                startReady = true;
                StartCoroutine(WaitForRandomizeObjects());

            }
        } else if (PhotonNetwork.IsMasterClient && photonView.IsMine && gameManager.GetGameState() == GameManager.GameState.Setup && !countdownReady) {

            bool allSpawned = true;

            foreach (Player player in PhotonNetwork.PlayerList) {

                if (player.CustomProperties["Spawned"] == null) {

                    allSpawned = false;
                    break;

                }
            }

            if (allSpawned) {

                countdownReady = true;
                photonView.RPC("StartGame", RpcTarget.All);

            }
        }
    }

    private IEnumerator WaitForRandomizeObjects() {

        gridPlacementController = FindObjectOfType<GridPlacementController>();
        yield return StartCoroutine(gridPlacementController.RandomizeGridObjects(photonView));

        Room currRoom = PhotonNetwork.CurrentRoom;
        ExitGames.Client.Photon.Hashtable properties = currRoom.CustomProperties;

        properties.Add("Spawns", JsonConvert.SerializeObject(gameManager.GetPlayerSpawns(), Formatting.Indented, new JsonSerializerSettings {

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore

        }));

        currRoom.SetCustomProperties(properties);
        gameManager.ChooseFirstTurn(photonView);
        photonView.RPC("SetupGame", RpcTarget.AllViaServer);

    }

    public void SetPlayerProperty(PhotonView view, PlayerProperty playerProperty, bool status) {

        if (view.IsMine) {

            ExitGames.Client.Photon.Hashtable properties = view.Owner.CustomProperties;

            switch (playerProperty) {

                case PlayerProperty.Loaded:

                properties.Add("Loaded", status);
                break;

                case PlayerProperty.Spawned:

                properties.Add("Spawned", status);
                break;

                default:

                Debug.LogError("Player property not specified.");
                return;

            }

            view.Owner.SetCustomProperties(properties);

        }
    }

    [PunRPC]
    public void SetupGame() {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            gameManager.UpdateGameState(GameManager.GameState.Setup);
            kingdomUIController = FindObjectOfType<KingdomUIController>();
            kingdomUIController.CloseLoadingScreen();

            SpawnPlayer(view);

            gridPlacementController = FindObjectOfType<GridPlacementController>();
            gridPlacementController.UpdateGridData();

        }
    }

    private void SpawnPlayer(PhotonView view) {

        List<Vector3> spawns = JsonConvert.DeserializeObject<List<Vector3>>((string) PhotonNetwork.CurrentRoom.CustomProperties["Spawns"]);
        Vector3 spawnPosition = spawns[Random.Range(0, spawns.Count)];

        if (gridData == null) {

            gridData = FindObjectOfType<GridData>();

        }

        gridData.MovePlayerTo(view, spawnPosition, true);

        Room currRoom = PhotonNetwork.CurrentRoom;
        ExitGames.Client.Photon.Hashtable properties = currRoom.CustomProperties;
        properties.Remove(spawnPosition);
        currRoom.SetCustomProperties(properties);

        Dictionary<PhotonView, Vector3Int> playerPositions = gridData.GetPlayerPositions();
        string[] text = new string[playerPositions.Count];
        int index = 0;

        foreach (KeyValuePair<PhotonView, Vector3Int> entry in playerPositions) {

            text[index] = entry.Key.ViewID + " " + entry.Value.x + " " + entry.Value.y + " " + entry.Value.z;
            index++;

        }

        view.RPC("UpdatePlayerPositions", RpcTarget.Others, UpdateType.Add, playerPositions.Count, text);
        SetPlayerProperty(view, PlayerProperty.Spawned, true);

    }

    [PunRPC]
    public IEnumerator StartGame() {

        if (kingdomUIController != null) {

            kingdomUIController = FindObjectOfType<KingdomUIController>();

        }

        for (int i = 3; i > 0; i--) {

            kingdomUIController.SetCountdownText(i + "");
            yield return new WaitForSeconds(1);

        }

        kingdomUIController.LoadDiceScene();
        gameManager.UpdateGameState(GameManager.GameState.Live);
        kingdomUIController.SetCountdownText(0 + "");

    }

    [PunRPC]
    public void SyncGridData(int size, string[] text) {

        Dictionary<Vector3Int, PlacementData> placedObjects = new Dictionary<Vector3Int, PlacementData>();
        string[] data;

        for (int i = 0; i < size; i++) {

            data = text[i].Split(' ');
            placedObjects.Add(new Vector3Int(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2])), JsonConvert.DeserializeObject<PlacementData>(data[3]));

        }

        if (gridData == null) {

            gridData = FindObjectOfType<GridData>();

        }

        gridData.SetPlacedObjects(placedObjects);

    }

    [PunRPC]
    public void UpdatePlayerPositions(UpdateType updateType, int size, string[] text) {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            Dictionary<PhotonView, Vector3Int> playerPositions;

            if (gridData == null) {

                gridData = FindObjectOfType<GridData>();

            }

            if (updateType == UpdateType.Reset) {

                playerPositions = new Dictionary<PhotonView, Vector3Int>();

            } else {

                playerPositions = gridData.GetPlayerPositions();

            }

            string[] data;

            for (int i = 0; i < size; i++) {

                data = text[i].Split(' ');
                playerPositions.Add(PhotonView.Find(int.Parse(data[0])), new Vector3Int(int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3])));

            }

            gridData.SetPlayerPositions(playerPositions);
            gridData.CalculatePlayerMoves();

        }
    }

    [PunRPC]
    public void OnTurnChange() {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            if (SceneManager.GetActiveScene().name == diceSceneName) {

                FindObjectOfType<DiceUIController>().TurnChanged();

            }
        }
    }
}
