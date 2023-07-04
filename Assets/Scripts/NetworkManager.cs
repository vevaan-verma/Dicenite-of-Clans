using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    [Header("References")]
    [SerializeField] private GameObject playerPiecePrefab;
    private PlayerData playerData;
    private GameManager gameManager;
    private KingdomUIController kingdomUIController;

    private void Start() {

        DontDestroyOnLoad(gameObject);
        playerData = GetComponent<PlayerData>();
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

                GridPlacementController gridPlacementController = FindObjectOfType<GridPlacementController>();
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

    public PlayerData GetPlayerData() {

        return playerData;

    }

    #region RPCs

    [PunRPC]
    public void StartGameRPC() {

        gameManager.UpdateGameState(GameManager.GameState.Live);
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        kingdomUIController.StartFadeOutLoadingScreen();

        Vector3[] spawns = gameManager.GetPlayerSpawns();
        Vector3 position = spawns[Random.Range(0, spawns.Length)];
        GameObject newPiece = PhotonNetwork.Instantiate(playerPiecePrefab.name, position + new Vector3(0f, playerPiecePrefab.transform.localScale.y, 0f), Quaternion.identity);
        playerData.SetPieceController(newPiece.GetComponent<PieceController>());
        playerData.GetPieceController().Initialize(this);

        GridPlacementController gridPlacementController = FindObjectOfType<GridPlacementController>();
        gridPlacementController.CalculatePlayerMoves(this);

    }

    [PunRPC]
    public void ClearAllDiceRPC() {

        foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

            Destroy(diceController.gameObject);

        }
    }

    #endregion

}
