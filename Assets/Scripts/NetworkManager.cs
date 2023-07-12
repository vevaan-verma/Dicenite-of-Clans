using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class NetworkManager : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;
    private PlayerController masterController;
    private GridPlacementController gridPlacementController;
    private bool readyToStart;

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

        foreach (PlayerController playerController in FindObjectsOfType<PlayerController>()) {

            if (playerController.photonView.OwnerActorNr == PhotonNetwork.MasterClient.ActorNumber) {

                masterController = playerController;

            }
        }
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

                Debug.LogError("Enter");
                readyToStart = true;
                StartCoroutine(WaitForRandomizeObjects());
                gameManager.ChooseFirstTurn();

            }
        }
    }

    private IEnumerator WaitForRandomizeObjects() {

        gridPlacementController = FindObjectOfType<GridPlacementController>();
        yield return StartCoroutine(gridPlacementController.RandomizeGridObjects());
        masterController.photonView.RPC("StartGameRPC", RpcTarget.All);

    }

    #region RPCs

    [PunRPC]
    public void ClearAllDiceRPC() {

        foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

            Destroy(diceController.gameObject);

        }
    }

    #endregion

}
