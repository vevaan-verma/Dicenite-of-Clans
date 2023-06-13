using UnityEngine;

public class GridPlacementController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private ObjectManager objectManager;
    private GameManager gameManager;
    private IBuildingState buildingState;
    private InputManager inputManager;

    [Header("Grid Data")]
    private GridData stackableData, nonStackableData;

    [Header("Placement Visuals")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridOverlay;
    [SerializeField] private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    [SerializeField] private PlaceableObjectDatabase objectDatabase;
    private Vector3Int lastPosition;

    [Header("Audio")]
    private AudioManager audioManager;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        inputManager = FindObjectOfType<InputManager>();
        audioManager = FindObjectOfType<AudioManager>();

        stackableData = new GridData();
        nonStackableData = new GridData();

        lastPosition = Vector3Int.zero;

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

    public void StartPlacement(int ID) {

        StopPlacement();
        gridOverlay.gameObject.SetActive(true);
        buildingState = new PlacementState(gameManager, objectManager, stackableData, nonStackableData, grid, previewSystem, objectDatabase, ID, audioManager);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }

    private void PlaceObject() {

        if (inputManager.IsPointerOverUI()) {

            return;

        }

        buildingState.OnAction(grid.WorldToCell(objectManager.previewSystem.previewObject.position));

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
        buildingState = new RemovingState(objectManager, stackableData, nonStackableData, grid, previewSystem, audioManager);
        inputManager.OnClick += PlaceObject;
        inputManager.OnExit += StopPlacement;

    }
}
