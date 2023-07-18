using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridData : MonoBehaviourPun {

    [Header("References")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    private Grid grid;
    private GameManager gameManager;
    private Dictionary<Vector3Int, PlacementData> placedObjects;
    private Dictionary<PhotonView, Vector3Int> playerPositions;

    private void Awake() {

        DontDestroyOnLoad(gameObject);
        grid = FindObjectOfType<Grid>();
        gameManager = FindObjectOfType<GameManager>();
        placedObjects = new Dictionary<Vector3Int, PlacementData>();
        playerPositions = new Dictionary<PhotonView, Vector3Int>();

    }

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int objectIndex, float yRotation) {

        List<Vector3Int> occupiedPositions = CalculatePositions(gridPosition, objectSize, yRotation);
        PlacementData data = new PlacementData(occupiedPositions, ID, objectIndex);

        foreach (Vector3Int position in occupiedPositions) {

            if (placedObjects.ContainsKey(position)) {

                throw new Exception($"Dictionary already contains cell position {position}");

            }

            placedObjects[position] = data;

        }
    }

    public Dictionary<Vector3Int, PlacementData> GetPlacedObjects() {

        return placedObjects;

    }

    public void SetPlacedObjects(Dictionary<Vector3Int, PlacementData> placedObjects) {

        this.placedObjects = placedObjects;

    }

    public void MovePlayerTo(PhotonView photonView, Vector3 spawnPosition, bool spawn) {

        if (playerPositions.ContainsKey(photonView)) {

            playerPositions.Remove(photonView);

        }

        photonView.transform.position = spawnPosition + new Vector3(0f, photonView.transform.localScale.y, 0f);
        playerPositions.Add(photonView, grid.WorldToCell(spawnPosition));

        string[] text = new string[playerPositions.Count];
        int index = 0;

        foreach (KeyValuePair<PhotonView, Vector3Int> entry in playerPositions) {

            text[index] = entry.Key.ViewID + " " + entry.Value.x + " " + entry.Value.y + " " + entry.Value.z;
            index++;

        }

        if (!spawn) {

            photonView.RPC("UpdatePlayerPositionsRPC", RpcTarget.Others, NetworkManager.UpdateType.Reset, text.Length, text);

        }

        Debug.LogError("Own Size: " + text);
        CalculatePlayerMoves();

    }

    public Dictionary<PhotonView, Vector3Int> GetPlayerPositions() {

        return playerPositions;

    }

    public void SetPlayerPositions(Dictionary<PhotonView, Vector3Int> playerPositions) {

        this.playerPositions = playerPositions;

    }

    public void CalculatePlayerMoves() {

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView view = null;

        for (int viewID = actorNum * PhotonNetwork.MAX_VIEW_IDS + 1; viewID < (actorNum + 1) * PhotonNetwork.MAX_VIEW_IDS; viewID++) {

            view = PhotonView.Find(viewID);

            if (view && view.IsMine) {

                break;

            }
        }

        if (view.IsMine) {

            foreach (MoveIndicatorController moveIndicatorController in FindObjectsOfType<MoveIndicatorController>()) {

                Destroy(moveIndicatorController.gameObject);

            }

            Vector3Int playerPosition = grid.WorldToCell(view.transform.position);
            HashSet<Vector3Int> validMoves = new HashSet<Vector3Int>();
            Vector3Int position;

            for (int x = -1; x < 2; x++) {

                position = playerPosition + new Vector3Int(x, 0, 1);

                if (CanPlaceObjectAt(position, Vector2Int.one, 0f, false, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, false)) {

                    validMoves.Add(position);

                }
            }

            for (int y = 0; y > -2; y--) {

                position = playerPosition + new Vector3Int(1, 0, y);

                if (CanPlaceObjectAt(position, Vector2Int.one, 0f, false, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, false)) {

                    validMoves.Add(position);

                }
            }

            for (int x = 0; x > -2; x--) {

                position = playerPosition + new Vector3Int(x, 0, -1);

                if (CanPlaceObjectAt(position, Vector2Int.one, 0f, false, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, false)) {

                    validMoves.Add(position);

                }
            }

            for (int y = 0; y < 2; y++) {

                position = playerPosition + new Vector3Int(-1, 0, y);

                if (CanPlaceObjectAt(position, Vector2Int.one, 0f, false, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, false)) {

                    validMoves.Add(position);

                }
            }

            foreach (Vector3Int pos in validMoves) {

                PieceController pieceController = view.GetComponent<PieceController>();
                pieceController.enabled = true;
                GameObject moveIndicator = Instantiate(moveIndicatorPrefab, pos, Quaternion.identity);
                moveIndicator.GetComponent<MoveIndicatorController>().Initialize(pieceController);

            }
        }
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize, float yRotation, bool placementState, int gridWidth, int gridHeight, List<Vector3> playerSpawns, Grid grid, bool randomizingObjects) {

        List<Vector3Int> occupiedPositions;

        if (placementState) {

            occupiedPositions = CalculatePositions(gridPosition, objectSize, yRotation);

        } else {

            occupiedPositions = CalculatePositions(gridPosition, objectSize, 0f);

        }

        bool isSpawn;

        foreach (Vector3Int position in occupiedPositions) {

            if (randomizingObjects) {

                isSpawn = false;

                foreach (Vector3 spawn in playerSpawns) {

                    if (position == grid.WorldToCell(spawn)) {

                        isSpawn = true;
                        break;

                    }
                }

                if (isSpawn) {

                    return false;

                }
            }

            Debug.Log("Positions");

            foreach (KeyValuePair<PhotonView, Vector3Int> pos in playerPositions) {

                Debug.Log(pos);

            }

            if (placedObjects.ContainsKey(position) || playerPositions.ContainsValue(position) || position.x < -(gridWidth / 2) || position.x > gridWidth / 2 - 1 || position.z < -(gridHeight / 2) || position.z > gridHeight / 2 - 1) {

                return false;

            }
        }

        return true;

    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize, float yRotation) {

        List<Vector3Int> occupiedPositions = new List<Vector3Int>();

        switch (yRotation) {

            case 0f:

            for (int x = 0; x < objectSize.x; x++) {

                for (int y = 0; y < objectSize.y; y++) {

                    occupiedPositions.Add(gridPosition + new Vector3Int(x, 0, y));

                }
            }

            break;

            case 90f:

            for (int x = 0; x < objectSize.y; x++) {

                for (int y = 0; y < objectSize.x; y++) {

                    occupiedPositions.Add(gridPosition + new Vector3Int(x, 0, y));

                }
            }

            break;

            case 180f:

            for (int x = 0; x < objectSize.x; x++) {

                for (int y = 0; y < objectSize.y; y++) {

                    occupiedPositions.Add(gridPosition + new Vector3Int(x, 0, y));

                }
            }

            break;

            case 270f:

            for (int x = 0; x < objectSize.y; x++) {

                for (int y = 0; y < objectSize.x; y++) {

                    occupiedPositions.Add(gridPosition + new Vector3Int(x, 0, y));

                }
            }

            break;

        }

        return occupiedPositions;

    }

    public int GetRepresentationIndex(Vector3Int gridPosition) {

        if (!placedObjects.ContainsKey(gridPosition)) {

            return -1;

        }

        return placedObjects[gridPosition].index;

    }

    public void RemoveObjectAt(Vector3Int gridPosition) {

        foreach (Vector3Int pos in placedObjects[gridPosition].occupiedPositions) {

            placedObjects.Remove(pos);

        }
    }
}

public class PlacementData {

    public List<Vector3Int> occupiedPositions;

    public int ID {

        get; private set;

    }

    public int index {

        get; private set;

    }

    public float yRotation {

        get; private set;

    }

    public PlacementData(List<Vector3Int> occupiedPositions, int ID, int index) {

        this.occupiedPositions = occupiedPositions;
        this.ID = ID;
        this.index = index;

    }
}
