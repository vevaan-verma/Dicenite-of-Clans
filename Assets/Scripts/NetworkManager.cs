using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class NetworkManager : MonoBehaviourPun {

    [Header("References")]
    private GameManager gameManager;
    private GridPlacementController gridPlacementController;
    private GridData gridData;
    private KingdomUIController kingdomUIController;
    private bool readyToStart;

    public enum UpdateType {

        Reset, Add

    }

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

    }

    private void Update() {

        if (PhotonNetwork.IsMasterClient && photonView.IsMine && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers() && gameManager.GetGameState() == GameManager.GameState.Waiting && !readyToStart) {

            bool allReady = true;

            foreach (Player player in PhotonNetwork.PlayerList) {

                if (player.CustomProperties["ReadyStart"] == null) {

                    allReady = false;
                    break;

                }
            }

            if (allReady) {

                readyToStart = true;
                StartCoroutine(WaitForRandomizeObjects());

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
        photonView.RPC("StartGameRPC", RpcTarget.All);

    }

    public void ReadyPlayer() {

        ExitGames.Client.Photon.Hashtable properties = photonView.Owner.CustomProperties;
        properties.Add("ReadyStart", true);
        photonView.Owner.SetCustomProperties(properties);

    }

    [PunRPC]
    public void StartGameRPC() {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            gameManager.UpdateGameState(GameManager.GameState.Live);
            kingdomUIController = FindObjectOfType<KingdomUIController>();
            kingdomUIController.StartFadeOutLoadingScreen();

            SpawnPlayer(view);

            gridPlacementController = FindObjectOfType<GridPlacementController>();
            gridPlacementController.UpdateGridData();

        }
    }

    private void SpawnPlayer(PhotonView photonView) {

        List<Vector3> spawns = JsonConvert.DeserializeObject<List<Vector3>>((string) PhotonNetwork.CurrentRoom.CustomProperties["Spawns"]);
        Vector3 spawnPosition = spawns[Random.Range(0, spawns.Count)];

        if (gridData == null) {

            gridData = FindObjectOfType<GridData>();

        }

        gridData.MovePlayerTo(photonView, spawnPosition, true);

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

        photonView.RPC("UpdatePlayerPositionsRPC", RpcTarget.Others, UpdateType.Add, playerPositions.Count, text);

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
    public void UpdatePlayerPositionsRPC(UpdateType updateType, int size, string[] text) {

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
            Debug.LogError("Received Size: " + size);

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
            Debug.LogError("Other Size: " + playerPositions.Count);
            gridData.CalculatePlayerMoves();

        }
    }

    [PunRPC]
    public void ClearAllDiceRPC() {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

                Destroy(diceController.gameObject);

            }
        }
    }
}
