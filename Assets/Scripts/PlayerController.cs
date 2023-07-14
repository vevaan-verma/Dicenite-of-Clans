using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPun {

    [Header("References")]
    private GameManager gameManager;
    private KingdomUIController kingdomUIController;
    private PieceController pieceController;
    private GridPlacementController gridPlacementController;

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

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
