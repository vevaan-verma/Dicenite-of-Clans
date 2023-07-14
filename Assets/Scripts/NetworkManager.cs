using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPun {

    [Header("References")]
    private GameManager gameManager;
    private GridPlacementController gridPlacementController;
    private KingdomUIController kingdomUIController;
    private PieceController pieceController;
    private bool readyToStart;

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

    }

    private void Update() {

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers() && gameManager.GetGameState() == GameManager.GameState.Waiting && !readyToStart) {

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
                gameManager.ChooseFirstTurn();

            }
        }
    }

    private IEnumerator WaitForRandomizeObjects() {

        gridPlacementController = FindObjectOfType<GridPlacementController>();
        yield return StartCoroutine(gridPlacementController.RandomizeGridObjects());
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

            List<Vector3> spawns = gameManager.GetPlayerSpawns();
            Vector3 position = spawns[Random.Range(0, spawns.Count)];
            pieceController = view.gameObject.AddComponent<PieceController>();
            view.transform.position = position + new Vector3(0f, view.transform.localScale.y, 0f);

            gridPlacementController = FindObjectOfType<GridPlacementController>();
            gridPlacementController.CalculatePlayerMoves(pieceController);

        }
    }

    [PunRPC]
    public void ClearAllDiceRPC() {

        foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

            Destroy(diceController.gameObject);

        }
    }
}
