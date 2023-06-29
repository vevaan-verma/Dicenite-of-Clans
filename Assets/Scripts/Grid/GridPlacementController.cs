using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlacementController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private ObjectManager objectManager;
    private GameManager gameManager;
    private KingdomUIController kingdomUIController;
    private AudioManager audioManager;
    private IBuildingState buildingState;
    private GridInputManager inputManager;

    [Header("Grid Data")]
    private GridData stackableData, nonStackableData;

    [Header("Placement Visuals")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridOverlay;
    [SerializeField] private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    [SerializeField] private PlaceableObjectDatabase objectDatabase;
    private Vector3Int lastPosition;
    private bool randomizingObjects;
    private int x;
    private int y;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        inputManager = FindObjectOfType<GridInputManager>();
        audioManager = FindObjectOfType<AudioManager>();

        stackableData = new GridData();
        nonStackableData = new GridData();

        for (int i = 0; i < objectDatabase.objectData.Count; i++) {

            objectDatabase.objectData[i].SetID(i);

        }

        lastPosition = Vector3Int.zero;

        StopPlacement();

        //if (PhotonNetwork.IsMasterClient) {

        StartCoroutine(RandomizeGridObjects());

        //}
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

    public IEnumerator RandomizeGridObjects() {

        randomizingObjects = true;

        for (x = -(gameManager.GetGridWidth() / 2); x < gameManager.GetGridWidth() / 2; x++) {

            for (y = -(gameManager.GetGridHeight() / 2); y < gameManager.GetGridHeight() / 2; y++) {

                ObjectData objData = objectDatabase.objectData[Random.Range(0, objectDatabase.objectData.Count)];
                float probability = Random.Range(0f, 1f);
                float rotation = 0f;

                buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, objData.ID, audioManager, false);
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

                if (!(objData.stackable ? stackableData : nonStackableData).CanPlaceObjectAt(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)), Vector2Int.one, 0f, true, gameManager.GetGridWidth(), gameManager.GetGridHeight())) {

                    buildingState.EndState();
                    continue;

                }

                while (probability >= objData.spawnProbability || !(objData.stackable ? stackableData : nonStackableData).CanPlaceObjectAt(grid.WorldToCell(previewSystem.GetPreviewObject().position), objData.size, rotation, true, gameManager.GetGridWidth(), gameManager.GetGridHeight())) {

                    objData = objectDatabase.objectData[Random.Range(0, objectDatabase.objectData.Count)];
                    probability = Random.Range(0f, 1f);
                    rotation = 0f;

                    if (buildingState != null) {

                        buildingState.EndState();

                    }

                    buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, objData.ID, audioManager, false);
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

                PlaceObject();

                while (previewSystem.GetPreviewObject() != null) {

                    yield return null;

                }
            }
        }

        randomizingObjects = false;

    }

    public void StartPlacement(int ID) {

        StopPlacement();
        gridOverlay.gameObject.SetActive(true);
        buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, ID, audioManager, true);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }

    public void StartForcedPlacement(int ID) {

        StopPlacement();
        gridOverlay.gameObject.SetActive(true);
        buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, ID, audioManager, true);
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

    private void StopPlacement() {

        if (buildingState == null) {

            return;

        }

        gridOverlay.gameObject.SetActive(false);

        buildingState.EndState();

        inputManager.OnClick -= PlaceObject;
        inputManager.OnExit -= StopPlacement;

        lastPosition = Vector3Int.zero;
        buildingState = null;

    }

    public void StartRemoving() {

        StopPlacement();
        gridOverlay.SetActive(true);
        buildingState = new RemovingState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, audioManager);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }
}
