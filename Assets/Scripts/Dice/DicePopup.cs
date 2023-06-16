using UnityEngine;
using TMPro;

public class DicePopup : MonoBehaviour {

    [Header("Popup Colors")]
    [SerializeField] private Color buildColor;
    [SerializeField] private Color attackColor;

    private void Start() {

        if (transform.parent.CompareTag("BuildersDice")) {

            GetComponent<TextMeshPro>().color = buildColor;

        } else if (transform.parent.CompareTag("AttackDice")) {

            GetComponent<TextMeshPro>().color = attackColor;

        }
    }
}
