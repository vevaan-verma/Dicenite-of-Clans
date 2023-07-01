using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    [Header("References")]
    private GameManager gameManager;
    private KingdomUIController kingdomUIController;
    private GridPlacementController gridPlacementController;

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameManager = FindObjectOfType<GameManager>();

    }

    private void Update() {

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == gameManager.GetMaxPlayers() && gameManager.GetGameState() == GameManager.GameState.Waiting) {

            bool allReady = true;

            foreach (Player player in PhotonNetwork.PlayerList) {

                if (player.CustomProperties["ReadyStart"] == null) {

                    allReady = false;
                    break;

                }
            }

            if (allReady) {

                gridPlacementController = FindObjectOfType<GridPlacementController>();
                StartCoroutine(gridPlacementController.RandomizeGridObjects());

                photonView.RPC("StartGameRPC", RpcTarget.All);

            }
        }
    }

    public void ReadyPlayer() {

        ExitGames.Client.Photon.Hashtable properties = photonView.Owner.CustomProperties;
        properties.Add("ReadyStart", true);
        photonView.Owner.SetCustomProperties(properties);

    }

    #region RPCs

    [PunRPC]
    public void StartGameRPC() {

        gameManager.UpdateGameState(GameManager.GameState.Live);
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        kingdomUIController.StartFadeOutLoadingScreen();

    }

    [PunRPC]
    public void ClearAllDiceRPC() {

        foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

            Destroy(diceController.gameObject);

        }
    }

    #endregion
}
