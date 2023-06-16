using UnityEngine;

public class RemovingState : IBuildingState {

    [Header("References")]
    private ObjectManager objectManager;

    [Header("Grid Data")]
    private GridData stackableData;
    private GridData nonStackableData;

    [Header("Placement Visuals")]
    private Grid grid;
    private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    private int objectIndex = -1;

    [Header("Audio")]
    private AudioManager audioManager;

    public RemovingState(ObjectManager objectPlacer, GridData stackableData, GridData nonStackableData, Grid grid, ObjectPreviewSystem previewSystem, AudioManager audioManager) {

        this.objectManager = objectPlacer;
        this.stackableData = stackableData;
        this.nonStackableData = nonStackableData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.audioManager = audioManager;

        previewSystem.ShowPlacementRemovePreview();

    }

    public void EndState() {

        previewSystem.HidePlacementPreview();

    }

    public void OnAction(Vector3Int gridPosition) {

        GridData selectedData = null;
        Transform previewObject = objectManager.previewSystem.previewObject;
        float yRotation = previewObject == null ? 0f : previewObject.rotation.eulerAngles.y;

        if (!nonStackableData.CanPlaceObjectAt(gridPosition, Vector2Int.one, yRotation, false)) {

            selectedData = nonStackableData;

        } else if (!stackableData.CanPlaceObjectAt(gridPosition, Vector2Int.one, yRotation, false)) {

            selectedData = stackableData;

        }

        if (selectedData == null) {

            audioManager.PlaySound(AudioManager.SoundType.Error);
            return;

        } else {

            audioManager.PlaySound(AudioManager.SoundType.Remove);

            objectIndex = selectedData.GetRepresentationIndex(gridPosition);

            if (objectIndex == -1) {

                return;

            }

            selectedData.RemoveObjectAt(gridPosition);
            objectManager.RemoveObjectAt(objectIndex);

        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckSelectionCompletelyEmpty(gridPosition));

    }

    private bool CheckSelectionCompletelyEmpty(Vector3Int gridPosition) {

        Transform previewObject = objectManager.previewSystem.previewObject;
        float yRotation = previewObject == null ? 0f : previewObject.rotation.eulerAngles.y;

        return !(stackableData.CanPlaceObjectAt(gridPosition, Vector2Int.one, yRotation, false) && nonStackableData.CanPlaceObjectAt(gridPosition, Vector2Int.one, yRotation, false));

    }

    public void UpdateState(Vector3Int gridPosition) {

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), CheckSelectionCompletelyEmpty(gridPosition));

    }
}
