using UnityEngine;
using TMPro;

public class DicePopup : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;

    [Header("Popup Colors")]
    [SerializeField] private Color buildColor;
    [SerializeField] private Color attackColor;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();

        if (transform.parent.CompareTag(gameManager.GetBuildDiceTag())) {

            GetComponent<TextMeshPro>().color = buildColor;

        } else if (transform.parent.CompareTag(gameManager.GetAttackDiceTag())) {

            GetComponent<TextMeshPro>().color = attackColor;

        }
    }
}
