using UnityEngine;

public class MoveIndicatorController : MonoBehaviour {

    [Header("References")]
    private PlayerData playerData;
    private GameManager gameManager;
    private Grid grid;
    private GridInputManager inputManager;

    private void Start() {

        grid = FindObjectOfType<Grid>();
        gameManager = FindObjectOfType<GameManager>();
        inputManager = FindObjectOfType<GridInputManager>();
        inputManager.OnClick += CheckClick;

    }

    private void OnDestroy() {

        inputManager.OnClick -= CheckClick;

    }

    public void Initialize(PlayerData playerData) {

        this.playerData = playerData;

    }

    private void CheckClick() {

        Vector3Int gridPosition = grid.WorldToCell(inputManager.GetSelectedGridPosition());
        Vector3 movePosition = grid.WorldToCell(inputManager.GetSelectedGridPosition()) + new Vector3(gameManager.GetCellSize() / 2f, 0f, gameManager.GetCellSize() / 2f);

        if (gridPosition == transform.position) {

            playerData.GetPieceController().StartMovePlayer(movePosition);

        }
    }
}
