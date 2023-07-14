using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PieceController : MonoBehaviourPun {

    [Header("References")]
    private PlayerData playerData;
    private PieceController pieceController;
    private GridPlacementController gridPlacementController;

    [Header("Movement")]
    private Coroutine moveCoroutine;

    private void Start() {

        gridPlacementController = FindObjectOfType<GridPlacementController>();
        pieceController = GetComponent<PieceController>();
        playerData = GetComponent<PlayerData>();

    }

    public void StartMovePlayer(Vector3 targetPosition) {

        if (!photonView.IsMine) {

            return;

        }

        if (moveCoroutine != null) {

            StopCoroutine(moveCoroutine);

        }

        moveCoroutine = StartCoroutine(MovePlayer(transform.position, targetPosition));

    }

    private IEnumerator MovePlayer(Vector3 startPosition, Vector3 targetPosition) {

        float currentTime = 0f;
        float duration = playerData.GetMoveDuration();

        while (currentTime < duration) {

            currentTime += Time.deltaTime;
            transform.position = new Vector3(Mathf.Lerp(startPosition.x, targetPosition.x, currentTime / duration), startPosition.y, Mathf.Lerp(startPosition.z, targetPosition.z, currentTime / duration));
            yield return null;

        }

        transform.position = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
        moveCoroutine = null;
        gridPlacementController.CalculatePlayerMoves(pieceController);

    }
}
