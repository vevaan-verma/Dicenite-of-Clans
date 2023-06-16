using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData {

    private Dictionary<Vector3Int, PlacementData> placedObjects = new Dictionary<Vector3Int, PlacementData>();

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

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize, float yRotation, bool placementState) {

        List<Vector3Int> occupiedPositions;

        if (placementState) {

            occupiedPositions = CalculatePositions(gridPosition, objectSize, yRotation);

        } else {

            occupiedPositions = CalculatePositions(gridPosition, objectSize, 0f);

        }

        foreach (Vector3Int position in occupiedPositions) {

            if (placedObjects.ContainsKey(position)) {

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
