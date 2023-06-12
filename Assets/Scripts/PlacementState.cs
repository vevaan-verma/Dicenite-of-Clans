using UnityEngine;

public class PlacementState : IBuildingState {

    [Header("References")]
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

    public PlacementState(ObjectManager objectPlacer, GridData stackableData, GridData nonStackableData, Grid grid, ObjectPreviewSystem previewSystem, PlaceableObjectDatabase objectDatabase, int ID, AudioManager audioManager) {

        this.objectManager = objectPlacer;
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
            previewSystem.ShowPlacementPreview(objectDatabase.objectData[selectedObjectIndex].prefab, objectDatabase.objectData[selectedObjectIndex].size, true);

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

        int index = objectManager.PlaceObject(objectDatabase.objectData[selectedObjectIndex].prefab, grid.CellToWorld(gridPosition));

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

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), CheckPlacementValidity(gridPosition));

    }
}
