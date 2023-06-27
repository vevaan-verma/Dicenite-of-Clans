using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

    [Header("Grid Data")]
    private List<GameObject> placedObjects;

    [Header("Grid Preview")]
    public ObjectPreviewSystem previewSystem;

    private void Awake() {

        placedObjects = new List<GameObject>();

    }

    public int PlaceObject(GameObject prefab, Vector3 position, Quaternion rotation) {

        Vector2Int size = previewSystem.GetObjectSize();
        GameObject newObject = Instantiate(prefab, position, rotation);

        switch (rotation.eulerAngles.y) {

            case 90f:

            newObject.transform.GetChild(0).localPosition -= new Vector3(size.y, 0f, 0f);
            break;

            case 180f:

            newObject.transform.GetChild(0).localPosition -= new Vector3(size.x, 0f, size.y);
            break;

            case 270f:

            newObject.transform.GetChild(0).localPosition -= new Vector3(0f, 0f, size.y);
            break;

        }

        placedObjects.Remove(newObject);
        placedObjects.Add(newObject);

        return placedObjects.Count - 1;

    }

    public void RemoveObjectAt(int objectIndex) {

        if (placedObjects.Count <= objectIndex || placedObjects[objectIndex] == null) {

            return;

        }

        Destroy(placedObjects[objectIndex]);
        placedObjects[objectIndex] = null;

    }
}
