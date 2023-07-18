using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GridPlacementController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private ObjectManager objectManager;
    private GameManager gameManager;
    private KingdomUIController kingdomUIController;
    private GridInputManager inputManager;
    private KingdomAudioManager audioManager;
    private IBuildingState buildingState;

    [Header("Grid Data")]
    private GridData gridData;

    [Header("Placement Visuals")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridOverlay;
    [SerializeField] private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    [SerializeField] private PlaceableObjectDatabase objectDatabase;
    private Vector3Int lastPosition;
    private bool randomizingObjects;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        inputManager = FindObjectOfType<GridInputManager>();
        audioManager = FindObjectOfType<KingdomAudioManager>();
        gridData = FindObjectOfType<GridData>();

        for (int i = 0; i < objectDatabase.objectData.Count; i++) {

            objectDatabase.objectData[i].SetID(i);

        }

        lastPosition = Vector3Int.zero;

        StopPlacement();

    }

    private void Update() {

        if (buildingState == null) {

            return;

        }

        Vector3Int gridPosition = grid.WorldToCell(inputManager.GetSelectedGridPosition());

        if (lastPosition != gridPosition && !randomizingObjects) {

            buildingState.UpdateState(gridPosition);
            lastPosition = gridPosition;

        }
    }

    public IEnumerator RandomizeGridObjects(PhotonView masterView) {

        randomizingObjects = true;

        for (int x = -(gameManager.GetGridWidth() / 2); x < gameManager.GetGridWidth() / 2; x++) {

            for (int y = -(gameManager.GetGridHeight() / 2); y < gameManager.GetGridHeight() / 2; y++) {

                List<ObjectData> usableObjects = new List<ObjectData>();
                float probability;
                float rotation = 0f;

                foreach (ObjectData data in objectDatabase.objectData) {

                    probability = Random.Range(0f, 100f);

                    if (probability < data.spawnProbability) {

                        usableObjects.Add(data);

                    }
                }

                if (usableObjects.Count == 0) {

                    continue;

                }

                ObjectData objData = usableObjects[Random.Range(0, usableObjects.Count)];

                buildingState = new PlacementState(gameManager, objectManager, gridData, grid, previewSystem, objectDatabase, objData.ID, audioManager, false);
                buildingState.UpdateState(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)));

                switch (Random.Range(0, 4)) {

                    case 1:

                    rotation = 90f;
                    previewSystem.RotatePreview();
                    break;

                    case 2:

                    rotation = 180f;

                    for (int i = 0; i < 2; i++) {

                        previewSystem.RotatePreview();

                    }

                    break;

                    case 3:

                    rotation = 270f;

                    for (int i = 0; i < 3; i++) {

                        previewSystem.RotatePreview();

                    }

                    break;

                }

                if (!gridData.CanPlaceObjectAt(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)), Vector2Int.one, 0f, true, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, true)) {

                    buildingState.EndState();
                    continue;

                }

                bool allObjectsUsed = false;

                while (!gridData.CanPlaceObjectAt(grid.WorldToCell(previewSystem.GetPreviewObject().position), objData.size, rotation, true, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid, true)) {

                    usableObjects.Remove(objData);

                    if (usableObjects.Count == 0) {

                        buildingState.EndState();
                        allObjectsUsed = true;
                        break;

                    }

                    objData = usableObjects[Random.Range(0, usableObjects.Count)];
                    rotation = 0f;

                    if (buildingState != null) {

                        buildingState.EndState();

                    }

                    buildingState = new PlacementState(gameManager, objectManager, gridData, grid, previewSystem, objectDatabase, objData.ID, audioManager, false);
                    buildingState.UpdateState(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)));

                    switch (Random.Range(0, 4)) {

                        case 1:

                        rotation = 90f;
                        previewSystem.RotatePreview();
                        break;

                        case 2:

                        rotation = 180f;

                        for (int i = 0; i < 2; i++) {

                            previewSystem.RotatePreview();

                        }

                        break;

                        case 3:

                        rotation = 270f;

                        for (int i = 0; i < 3; i++) {

                            previewSystem.RotatePreview();

                        }

                        break;

                    }

                    yield return null;

                }

                if (!allObjectsUsed) {

                    PlaceObject();

                    while (previewSystem.GetPreviewObject() != null) {

                        yield return null;

                    }
                }
            }
        }

        Dictionary<Vector3Int, PlacementData> placedObjects = gridData.GetPlacedObjects();
        string[] text = new string[placedObjects.Count];
        int index = 0;

        foreach (KeyValuePair<Vector3Int, PlacementData> entry in placedObjects) {

            text[index] = entry.Key.x + " " + entry.Key.y + " " + entry.Key.z + " " + JsonConvert.SerializeObject(entry.Value);
            index++;

        }

        masterView.RPC("SyncGridData", RpcTarget.Others, text.Length, text);
        StopPlacement();
        randomizingObjects = false;

    }

    public void UpdateGridData() {

        gridData = FindObjectOfType<GridData>();

    }

    public void StartPlacement(int ID) {

        StopPlacement();
        gridOverlay.gameObject.SetActive(true);
        buildingState = new PlacementState(gameManager, objectManager, gridData, grid, previewSystem, objectDatabase, ID, audioManager, true);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }

    public void StartForcedPlacement(int ID) {

        StopPlacement();
        gridOverlay.gameObject.SetActive(true);
        buildingState = new PlacementState(gameManager, objectManager, gridData, grid, previewSystem, objectDatabase, ID, audioManager, true);
        inputManager.OnClick += PlaceObject;
        kingdomUIController.StartFadeOutKingdomHUD(0f);

    }

    private void PlaceObject() {

        if (inputManager.IsPointerOverUI() && !randomizingObjects) {

            return;

        }

        kingdomUIController.StartFadeInKingdomHUD();
        buildingState.OnAction(grid.WorldToCell(previewSystem.GetPreviewObject().position));
        StopPlacement();

    }

    public void StopPlacement() {

        gridOverlay.gameObject.SetActive(false);

        if (buildingState == null) {

            return;

        }

        buildingState.EndState();

        inputManager.OnClick -= PlaceObject;
        inputManager.OnExit -= StopPlacement;

        lastPosition = Vector3Int.zero;
        buildingState = null;

    }

    public void StartRemoving() {

        StopPlacement();
        gridOverlay.SetActive(true);
        buildingState = new RemovingState(gameManager, objectManager, gridData, grid, previewSystem, audioManager);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }
}
