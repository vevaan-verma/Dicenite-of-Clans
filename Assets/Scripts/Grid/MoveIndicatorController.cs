using UnityEngine;

public class MoveIndicatorController : MonoBehaviour {

    [Header("References")]
    private PieceController pieceController;
    private GameManager gameManager;
    private Grid grid;
    private GridInputManager inputManager;

    public void Initialize(PieceController pieceController) {

        grid = FindObjectOfType<Grid>();
        gameManager = FindObjectOfType<GameManager>();
        inputManager = FindObjectOfType<GridInputManager>();
        inputManager.OnClick += OnClick;
        this.pieceController = pieceController;

    }

    private void OnDestroy() {

        inputManager.OnClick -= OnClick;

    }

    private void OnClick() {

        Vector3Int gridPosition = grid.WorldToCell(inputManager.GetSelectedGridPosition());
        Vector3 movePosition = grid.WorldToCell(inputManager.GetSelectedGridPosition()) + new Vector3(gameManager.GetCellSize() / 2f, 0f, gameManager.GetCellSize() / 2f);

        if (gridPosition == transform.position) {

            pieceController.StartMovePlayer(movePosition);

        }
    }
}
