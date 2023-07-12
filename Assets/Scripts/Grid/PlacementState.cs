using Photon.Pun;
using UnityEngine;

public class PlacementState : IBuildingState {

    [Header("References")]
    private GameManager gameManager;
    private ObjectManager objectManager;

    [Header("Grid Data")]
    private GridData gridData;

    [Header("Placement Visuals")]
    private Grid grid;
    private ObjectPreviewSystem previewSystem;

    [Header("Placeable Objects")]
    private PlaceableObjectDatabase objectDatabase;
    private int selectedObjectIndex = -1;
    private int ID = -1;
    private bool userPlacing;

    [Header("Audio")]
    private KingdomAudioManager audioManager;

    public PlacementState(GameManager gameManager, ObjectManager objectManager, GridData gridData, Grid grid, ObjectPreviewSystem previewSystem, PlaceableObjectDatabase objectDatabase, int ID, KingdomAudioManager audioManager, bool userPlacing) {

        this.gameManager = gameManager;
        this.objectManager = objectManager;
        this.gridData = gridData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.objectDatabase = objectDatabase;
        this.ID = ID;
        this.audioManager = audioManager;
        this.userPlacing = userPlacing;

        selectedObjectIndex = objectDatabase.objectData.FindIndex(data => data.ID == ID);

        if (selectedObjectIndex > -1) {

            if (userPlacing) {

                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Click);

            }

            previewSystem.ShowPlacementPreview(objectDatabase.objectData[selectedObjectIndex].prefab, objectDatabase.objectData[selectedObjectIndex].size, userPlacing);

        } else {

            if (userPlacing) {

                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Error);

            }

            throw new System.Exception($"No object with ID {ID}");

        }
    }

    public void OnAction(Vector3Int gridPosition) {

        if (!CheckPlacementValidity(gridPosition)) {

            if (userPlacing) {

                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Error);

            }

            return;

        }

        if (userPlacing) {

            audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Place);

        }

        int index = objectManager.PlaceObject(objectDatabase.objectData[selectedObjectIndex].prefab, previewSystem.GetPreviewObject().position, previewSystem.GetPreviewObject().rotation);

        gridData.AddObjectAt(grid.WorldToCell(previewSystem.GetPreviewObject().position), objectDatabase.objectData[selectedObjectIndex].size, objectDatabase.objectData[selectedObjectIndex].ID, index, previewSystem.GetPreviewObject().rotation.eulerAngles.y);

        previewSystem.UpdatePosition(previewSystem.GetPreviewObject().position, false);

    }

    private bool CheckPlacementValidity(Vector3Int gridPosition) {

        if (previewSystem.GetPreviewObject() == null) {

            return false;

        }

        return gridData.CanPlaceObjectAt(gridPosition, objectDatabase.objectData[selectedObjectIndex].size, previewSystem.GetPreviewObject().rotation.eulerAngles.y, true, gameManager.GetGridWidth(), gameManager.GetGridHeight(), gameManager.GetPlayerSpawns(), grid);

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

        if (grid.transform.position.x + gridPosition.x > grid.transform.position.x + Mathf.Floor(gridWidth / 2f) - previewSystem.GetObjectSize().x) {

            gridPosition.x = (int) (grid.transform.position.x + Mathf.Floor(gridWidth / 2f) - previewSystem.GetObjectSize().x);

        }

        if (grid.transform.position.z + gridPosition.z < grid.transform.position.z - Mathf.Floor(gridHeight / 2f)) {

            gridPosition.z = (int) (grid.transform.position.z - Mathf.Floor(gridHeight / 2f));

        }

        if (grid.transform.position.z + gridPosition.z > grid.transform.position.z + Mathf.Floor(gridHeight / 2f) - previewSystem.GetObjectSize().y) {

            gridPosition.z = (int) (grid.transform.position.z + Mathf.Floor(gridHeight / 2f) - previewSystem.GetObjectSize().y);

        }

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), CheckPlacementValidity(gridPosition));

    }
}
