using System.Collections;
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

        StartCoroutine(RandomizeGridObjects());
        StopPlacement();

    }

    private void Update() {

        if (buildingState == null) {

            return;

        }

        Vector3Int gridPosition = grid.WorldToCell(inputManager.GetSelectedGridPosition());

        if (lastPosition != gridPosition) {

            buildingState.UpdateState(gridPosition);
            lastPosition = gridPosition;

        }
    }

    public IEnumerator RandomizeGridObjects() {

        for (int x = -(gameManager.GetGridWidth() / 2); x < gameManager.GetGridWidth() / 2; x++) {

            for (int y = -(gameManager.GetGridHeight() / 2); y < gameManager.GetGridHeight() / 2; y++) {

                randomizingObjects = true;

                ObjectData objData = null;
                float highestProbability = 0f;

                foreach (ObjectData data in objectDatabase.objectData) {

                    if ((data.stackable ? stackableData : nonStackableData).CanPlaceObjectAt(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)), data.size, 0f, true, gameManager.GetGridWidth(), gameManager.GetGridHeight())) {

                        float probability = Random.Range(0f, 1f);

                        if (probability < data.spawnProbability && probability > highestProbability) {

                            objData = data;
                            highestProbability = probability;

                        }
                    }
                }

                if (objData != null) {

                    int rotationNum = Random.Range(0, 4);

                    buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, objData.ID, audioManager, false);
                    buildingState.UpdateState(grid.WorldToCell(grid.transform.position + new Vector3(x, 0f, y)));
                    PlaceObject();
                    yield return new WaitForSeconds(0.05f);

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
