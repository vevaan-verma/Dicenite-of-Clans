using UnityEngine;

public class PlacementState : IBuildingState {

    [Header("References")]
    private GameManager gameManager;
    private ObjectManager objectManager;

    [Header("Grid Data")]
    private GridData stackableData;
    private GridData nonStackableData;

    [Header("Placement Visuals")]
    private Grid grid;
    private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    private PlaceableObjectDatabase objectDatabase;
    private int selectedObjectIndex = -1;
    private int ID;

    [Header("Audio")]
    private AudioManager audioManager;

    public PlacementState(GameManager gameManager, ObjectManager objectManager, GridData stackableData, GridData nonStackableData, Grid grid, ObjectPreviewSystem previewSystem, PlaceableObjectDatabase objectDatabase, int ID, AudioManager audioManager) {

        this.gameManager = gameManager;
        this.objectManager = objectManager;
        this.stackableData = stackableData;
        this.nonStackableData = nonStackableData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.objectDatabase = objectDatabase;
        this.ID = ID;
        this.audioManager = audioManager;

        selectedObjectIndex = objectDatabase.objectData.FindIndex(data => data.ID == ID);

        if (selectedObjectIndex > -1) {

            audioManager.PlaySound(AudioManager.SoundType.Click);
            previewSystem.ShowPlacementPreview(objectDatabase.objectData[selectedObjectIndex].prefab, objectDatabase.objectData[selectedObjectIndex].size);

        } else {

            audioManager.PlaySound(AudioManager.SoundType.Error);
            throw new System.Exception($"No object with ID {ID}");

        }

        this.audioManager = audioManager;

    }

    public void OnAction(Vector3Int gridPosition) {

        if (!CheckPlacementValidity(gridPosition)) {

            audioManager.PlaySound(AudioManager.SoundType.Error);
            return;

        }

        audioManager.PlaySound(AudioManager.SoundType.Place);

        int index = objectManager.PlaceObject(objectDatabase.objectData[selectedObjectIndex].prefab);

        (objectDatabase.objectData[selectedObjectIndex].stackable ? stackableData : nonStackableData).AddObjectAt(gridPosition, objectDatabase.objectData[selectedObjectIndex].size, objectDatabase.objectData[selectedObjectIndex].ID, index, objectManager.previewSystem.previewObject.rotation.eulerAngles.y);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);

    }

    private bool CheckPlacementValidity(Vector3Int gridPosition) {

        return (objectDatabase.objectData[selectedObjectIndex].stackable ? stackableData : nonStackableData).CanPlaceObjectAt(gridPosition, objectDatabase.objectData[selectedObjectIndex].size, objectManager.previewSystem.previewObject.rotation.eulerAngles.y, true);

    }

    public void EndState() {

        previewSystem.HidePlacementPreview();

    }

    public void UpdateState(Vector3Int gridPosition) {

        int gridWidth = gameManager.GetGridWidth();
        int gridHeight = gameManager.GetGridHeight();

        if (grid.transform.position.x + gridPosition.x < grid.transform.position.x - Mathf.Floor(gridWidth / 2f)) {

            gridPosition.x = (int) (grid.transform.position.x - Mathf.Floor(gridWidth / 2f));

        }

        if (grid.transform.position.x + gridPosition.x > grid.transform.position.x + Mathf.Floor(gridWidth / 2f) - previewSystem.size.x) {

            gridPosition.x = (int) (grid.transform.position.x + Mathf.Floor(gridWidth / 2f) - previewSystem.size.x);

        }

        if (grid.transform.position.z + gridPosition.z < grid.transform.position.z - Mathf.Floor(gridHeight / 2f)) {

            gridPosition.z = (int) (grid.transform.position.z - Mathf.Floor(gridHeight / 2f));

        }

        if (grid.transform.position.z + gridPosition.z > grid.transform.position.z + Mathf.Floor(gridHeight / 2f) - previewSystem.size.y) {

            gridPosition.z = (int) (grid.transform.position.z + Mathf.Floor(gridHeight / 2f) - previewSystem.size.y);

        }

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), CheckPlacementValidity(gridPosition));

    }
}
