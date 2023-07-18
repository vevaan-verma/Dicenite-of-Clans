using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviourPun {

    [Header("References")]
    private PlayerData playerData;
    private GridData gridData;

    [Header("Movement")]
    private Coroutine moveCoroutine;

    private void Start() {

        playerData = GetComponent<PlayerData>();

    }

    public void StartMovePlayer(Vector3 targetPosition) {

        if (!photonView.IsMine) {

            return;

        }

        if (moveCoroutine != null) {

            StopCoroutine(moveCoroutine);

        }

        if (gridData == null) {

            gridData = FindObjectOfType<GridData>();

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

        string text = "";
        Dictionary<PhotonView, Vector3Int> playerPositions = gridData.GetPlayerPositions();

        foreach (KeyValuePair<PhotonView, Vector3Int> entry in playerPositions) {

            text += entry.Key.ViewID + " " + entry.Value.x + " " + entry.Value.y + " " + entry.Value.z + " ";

        }

        gridData.MovePlayerTo(photonView, targetPosition, false);

    }
}
