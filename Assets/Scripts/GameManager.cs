using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float diceStillTime;
    [SerializeField] private int buildersDice;
    [SerializeField] private int attackDice;

    public void ClearAllDice() {

        foreach (DiceController diceController in FindObjectsOfType<DiceController>()) {

            Destroy(diceController.gameObject);

        }
    }

    public float GetDiceStillTime() {

        return diceStillTime;

    }

    public int GetBuildersDice() {

        return buildersDice;

    }

    public int GetAttackDice() {

        return attackDice;

    }
}
