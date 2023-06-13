using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

    [Header("Grid Data")]
    private List<GameObject> placedObjects;

    [Header("Grid Preview")]
    public ObjectPreviewSystem previewSystem;

    private void Start() {

        placedObjects = new List<GameObject>();

    }

    public int PlaceObject(GameObject prefab) {

        Transform previewObject = previewSystem.GetPreviewObject();
        Vector2Int size = previewSystem.size;

        GameObject newObject = Instantiate(prefab, previewObject.position, previewObject.rotation);

        switch (previewObject.rotation.eulerAngles.y) {

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
