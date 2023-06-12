using UnityEngine;

public class DiceRoller : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject dice;

    [Header("Roll Settings")]
    [SerializeField] private float diceVelocity;
    [SerializeField][Range(-90f, 90f)] private float minAngle;
    [SerializeField][Range(-90f, 90f)] private float maxAngle;

    public void RollBuildersDice() {

        Transform newDice = Instantiate(dice, transform.position, Random.rotation).transform;

        newDice.tag = "BuildersDice";
        newDice.GetComponent<Rigidbody>().AddForce(new Vector3(Random.value, Random.value, Random.value) * diceVelocity, ForceMode.Force);

    }

    public void RollAttackDice() {

        Transform newDice = Instantiate(dice, transform.position, Random.rotation).transform;

        newDice.tag = "AttackDice";
        newDice.GetComponent<Rigidbody>().AddForce(new Vector3(Random.value, Random.value, Random.value) * diceVelocity, ForceMode.Force);

    }
}
