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

        gameManager.UpdateGameState(GameManager.GameState.Live);
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        kingdomUIController.StartFadeOutLoadingScreen();

        List<Vector3> spawns = gameManager.GetPlayerSpawns();
        Vector3 position = spawns[Random.Range(0, spawns.Count)];
        pieceController = gameObject.AddComponent<PieceController>();
        transform.position = position + new Vector3(0f, transform.localScale.y, 0f);

        gridPlacementController = FindObjectOfType<GridPlacementController>();
        gridPlacementController.CalculatePlayerMoves(pieceController);

    }
}
