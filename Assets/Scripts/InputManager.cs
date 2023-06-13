using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;

    [Header("Placement Detection")]
    [SerializeField] private Camera placementCamera;
    [SerializeField] private LayerMask gridMask;
    private Vector3 lastPosition;

    public event Action OnClick, OnRotate, OnExit;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();

    }

    private void Update() {

        if (Input.GetMouseButtonDown(0)) {

            OnClick?.Invoke();

        }

        if (Input.GetKeyDown(KeyCode.R)) {

            OnRotate?.Invoke();

        }

        if (Input.GetKeyDown(KeyCode.Escape)) {

            OnExit?.Invoke();

        }
    }

    public bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedGridPosition() {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = placementCamera.nearClipPlane;

        RaycastHit hit;

        if (Physics.Raycast(placementCamera.ScreenPointToRay(mousePosition), out hit, 25f, gridMask)) {

            lastPosition = hit.point;

        }

        return lastPosition;

    }
}
