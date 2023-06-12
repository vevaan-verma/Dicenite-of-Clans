using System.Collections;
using UnityEngine;

public class ObjectPreviewSystem : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject cellIndicator;
    private Transform cellIndicatorChild;
    private InputManager inputManager;
    private Renderer cellIndicatorRenderer;

    [Header("Grid Settings")]
    [SerializeField] private Transform grid;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;

    [Header("Preview Materials")]
    [SerializeField] private Material previewMaterial;
    private Material newPreviewMaterial;

    [Header("Offset Settings")]
    [SerializeField] private float previewYOffset;

    [Header("Object Data")]
    [HideInInspector] public Transform previewObject;
    [HideInInspector] public Vector2Int size;
    private GameObject prefab;

    [Header("Object Rotation")]
    private float yRotation;

    [Header("Audio")]
    private AudioManager audioManager;

    private void Start() {

        inputManager = FindObjectOfType<InputManager>();
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        audioManager = FindObjectOfType<AudioManager>();

        newPreviewMaterial = new Material(previewMaterial);
        cellIndicator.SetActive(false);

        cellIndicator.transform.position = Vector3.zero;
        cellIndicator.transform.rotation = Quaternion.identity;

        cellIndicatorChild = cellIndicator.transform.GetChild(0);
        cellIndicatorChild.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
        cellIndicatorChild.localRotation = Quaternion.Euler(90f, 0f, 0f);
        cellIndicatorChild.localScale = new Vector3(1f, 1f, 1f);

    }

    public void ShowPlacementPreview(GameObject prefab, Vector2Int size, bool newPrefab) {

        this.prefab = prefab;
        this.size = size;

        if (newPrefab) {

            yRotation = 0f;

        }

        PrepareCellIndicator(size);
        previewObject = Instantiate(prefab, cellIndicator.transform.position, Quaternion.Euler(0f, yRotation, 0f)).transform;
        PreparePreview();
        cellIndicator.SetActive(true);

        inputManager.OnRotate -= RotatePreview;
        inputManager.OnRotate += RotatePreview;

    }

    private void PreparePreview() {

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers) {

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++) {

                materials[i] = newPreviewMaterial;

            }

            renderer.materials = materials;

        }
    }

    private void PrepareCellIndicator(Vector2Int size) {

        Vector2Int newSize = size;

        if (size.x > 0 || size.y > 0) {

            switch (yRotation) {

                case 0f:

                cellIndicator.transform.localScale = new Vector3(size.x, 1f, size.y);

                cellIndicatorChild = cellIndicator.transform.GetChild(0);
                cellIndicatorChild.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
                cellIndicatorChild.localRotation = Quaternion.Euler(90f, 0f, 0f);
                cellIndicatorChild.localScale = new Vector3(1f, 1f, 1f);

                cellIndicator.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                cellIndicatorRenderer.material.mainTextureScale = size;
                break;

                case 90f:

                cellIndicator.transform.localScale = new Vector3(size.x, 1f, size.y);

                if (size.x != size.y) {

                    cellIndicatorChild.localPosition -= new Vector3(Mathf.Ceil(newSize.x / 2f), 0f, Mathf.Ceil(newSize.x / 2f));
                    cellIndicator.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                } else {

                    cellIndicatorChild.localPosition -= new Vector3(1f, 0f, 0f);
                    cellIndicator.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                }

                cellIndicatorRenderer.material.mainTextureScale = size;
                break;

                case 180f:

                newSize.x = size.y;
                newSize.y = size.x;

                cellIndicator.transform.localScale = new Vector3(newSize.x, 1f, newSize.y);

                if (size.x != size.y) {

                    cellIndicatorChild.localPosition += new Vector3(Mathf.Ceil(newSize.x / 2f), 0f, 0f);
                    cellIndicator.transform.rotation = Quaternion.Euler(0f, 270f, 0f);

                } else {

                    cellIndicatorChild.localPosition -= new Vector3(0f, 0f, 1f);
                    cellIndicator.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                }

                cellIndicatorRenderer.material.mainTextureScale = newSize;
                break;

                case 270f:

                cellIndicator.transform.localScale = new Vector3(size.x, 1f, size.y);

                if (size.x == size.y) {

                    cellIndicatorChild.localPosition += new Vector3(1f, 0f, 0f);

                }

                cellIndicator.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                cellIndicatorRenderer.material.mainTextureScale = size;
                break;

            }
        }
    }

    public void HidePlacementPreview() {

        if (previewObject != null) {

            Destroy(previewObject.gameObject);

        }

        cellIndicator.SetActive(false);

        inputManager.OnRotate -= RotatePreview;

    }

    public void RotatePreview() {

        Vector2Int newSize = size;

        audioManager.PlaySound(AudioManager.SoundType.Rotate);

        switch (yRotation) {

            case 0f:

            newSize.x = size.y;
            newSize.y = size.x;

            yRotation = 90f;

            HidePlacementPreview();
            ShowPlacementPreview(prefab, newSize, false);

            previewObject.transform.GetChild(0).localPosition -= new Vector3(size.y, 0f, 0f);
            break;

            case 90f:

            newSize.x = size.y;
            newSize.y = size.x;

            yRotation = 180f;

            HidePlacementPreview();
            ShowPlacementPreview(prefab, newSize, false);

            previewObject.transform.GetChild(0).localPosition -= new Vector3(newSize.x, 0f, newSize.y);
            break;

            case 180f:

            yRotation = 270f;

            HidePlacementPreview();
            ShowPlacementPreview(prefab, newSize, false);

            previewObject.transform.GetChild(0).localPosition -= new Vector3(0f, 0f, newSize.y);
            break;

            case 270f:

            yRotation = 0f;

            HidePlacementPreview();
            ShowPlacementPreview(prefab, newSize, false);

            break;

        }

        previewObject.transform.position = new Vector3(cellIndicator.transform.position.x, cellIndicator.transform.position.y + previewYOffset, cellIndicator.transform.position.z);

    }

    public void UpdatePosition(Vector3 position, bool valid) {

        if (grid.position.x + position.x < grid.position.x - Mathf.Floor(gridWidth / 2f)) {

            position.x = grid.position.x - Mathf.Floor(gridWidth / 2f);

        }

        if (grid.position.x + position.x > grid.position.x + Mathf.Floor(gridWidth / 2f) - size.x) {

            position.x = grid.position.x + Mathf.Floor(gridWidth / 2f) - size.x;

        }

        if (grid.position.z + position.z < grid.position.z - Mathf.Floor(gridHeight / 2f)) {

            position.z = grid.position.z - Mathf.Floor(gridHeight / 2f);

        }

        if (grid.position.z + position.z > grid.position.z + Mathf.Floor(gridHeight / 2f) - size.y) {

            position.z = grid.position.z + Mathf.Floor(gridHeight / 2f) - size.y;

        }

        position.y = 0f;

        if (previewObject != null) {

            previewObject.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
            ApplyFeedbackToPreview(valid);

        }

        cellIndicator.transform.position = position;

        ApplyFeedbackToCellIndicator(valid);

    }

    private void ApplyFeedbackToPreview(bool valid) {

        Color color = valid ? Color.white : Color.red;
        color.a = previewMaterial.color.a;
        newPreviewMaterial.color = color;

    }

    private void ApplyFeedbackToCellIndicator(bool valid) {

        Color color = valid ? Color.white : Color.red;
        cellIndicatorRenderer.material.color = color;

    }

    public void ShowPlacementRemovePreview() {

        yRotation = 0f;
        cellIndicator.SetActive(true);
        PrepareCellIndicator(Vector2Int.one);
        ApplyFeedbackToCellIndicator(false);

    }

    public Transform GetPreviewObject() {

        return previewObject;

    }
}
