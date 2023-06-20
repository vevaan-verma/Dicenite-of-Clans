using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject dice;

    [Header("Roll Settings")]
    [SerializeField] private float diceVelocity;

    public void RollBuildersDice(DiceRotation rotation, float diceVelocity) {

        Quaternion newRotation = new Quaternion(rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW());
        Transform newDice = Instantiate(dice, transform.position, newRotation).transform;

        newDice.tag = "BuildersDice";
        newDice.GetComponent<Rigidbody>().AddForce(newDice.forward * diceVelocity, ForceMode.VelocityChange);

    }

    public RollData RollTestingBuildersDice(RollData rollData) {

        int diceRollerIndex = int.Parse(name[name.Length - 1] + "");
        Quaternion rotation = Random.rotation;

        rollData.SetDiceRoller(diceRollerIndex);
        rollData.SetDiceRotation(new DiceRotation(rotation.x, rotation.y, rotation.z, rotation.w));
        rollData.SetDiceVelocity(diceVelocity);

        Transform newDice = Instantiate(dice, transform.position, rotation).transform;

        newDice.tag = "BuildersDice";
        newDice.GetComponent<Rigidbody>().AddForce(newDice.forward * diceVelocity, ForceMode.VelocityChange);

        return rollData;

    }

    public void RollAttackDice() {

        Transform newDice = Instantiate(dice, transform.position, Random.rotation).transform;

        newDice.tag = "AttackDice";
        newDice.GetComponent<Rigidbody>().AddForce(newDice.forward * diceVelocity, ForceMode.VelocityChange);

    }
}

public class RollRootObject {

    public List<List<RollData>> rollData {

        get; set;

    }

    public RollRootObject() {

        rollData = new List<List<RollData>>();

    }
}

public class RollData {

    public int diceRoller {

        get; set;

    }

    public DiceRotation diceRotation {

        get; set;

    }

    public float diceVelocity {

        get; set;

    }

    public int GetDiceRoller() {

        return diceRoller;

    }

    public void SetDiceRoller(int diceRollerIndex) {

        diceRoller = diceRollerIndex;

    }

    public DiceRotation GetDiceRotation() {

        return diceRotation;

    }

    public void SetDiceRotation(DiceRotation rotation) {

        diceRotation = rotation;

    }

    public float GetDiceVelocity() {

        return diceVelocity;

    }

    public void SetDiceVelocity(float velocity) {

        diceVelocity = velocity;

    }
}

public class DiceRotation {

    public float x {

        get; set;

    }

    public float y {

        get; set;

    }

    public float z {

        get; set;

    }

    public float w {

        get; set;

    }

    public DiceRotation(float x, float y, float z, float w) {

        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;

    }

    public float GetX() {

        return x;

    }

    public float GetY() {

        return y;

    }

    public float GetZ() {

        return z;

    }

    public float GetW() {

        return w;

    }
}