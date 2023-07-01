using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviourPunCallbacks {

    [Header("References")]
    [SerializeField] private GameObject dice;
    private GameManager gameManager;

    [Header("Roll Settings")]
    [SerializeField] private float diceVelocity;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();

    }

    [PunRPC]
    public void RollBuildDiceRPC(Quaternion rotation, float diceVelocity) {

        DiceController newDice = Instantiate(dice, transform.position, rotation).GetComponent<DiceController>();

        foreach (PlayerData playerData in FindObjectsOfType<PlayerData>()) {

            PhotonView photonView = playerData.GetComponent<PhotonView>();

            if (photonView.IsMine) {

                newDice.SetRoller(photonView);

            }
        }

        newDice.tag = gameManager.GetBuildDiceTag();
        newDice.GetComponent<Rigidbody>().AddForce(newDice.transform.forward * diceVelocity, ForceMode.VelocityChange);

    }

    public RollData RollTestingBuildDice(RollData rollData) {

        int diceRollerIndex = int.Parse(name[name.Length - 1] + "");
        Quaternion rotation = Random.rotation;

        rollData.SetDiceRoller(diceRollerIndex);
        rollData.SetDiceRotation(new DiceRotation(rotation.x, rotation.y, rotation.z, rotation.w));
        rollData.SetDiceVelocity(diceVelocity);

        Transform newDice = Instantiate(dice, transform.position, rotation).transform;

        newDice.tag = gameManager.GetBuildDiceTag();
        newDice.GetComponent<Rigidbody>().AddForce(newDice.forward * diceVelocity, ForceMode.VelocityChange);

        return rollData;

    }

    [PunRPC]
    public void RollAttackDiceRPC(Quaternion rotation, float diceVelocity) {

        DiceController newDice = Instantiate(dice, transform.position, rotation).GetComponent<DiceController>();

        foreach (PlayerData playerData in FindObjectsOfType<PlayerData>()) {

            PhotonView photonView = playerData.GetComponent<PhotonView>();

            if (photonView.IsMine) {

                newDice.SetRoller(photonView);

            }
        }

        newDice.tag = gameManager.GetAttackDiceTag();
        newDice.GetComponent<Rigidbody>().AddForce(newDice.transform.forward * diceVelocity, ForceMode.VelocityChange);

    }

    public RollData RollTestingAttackDice(RollData rollData) {

        int diceRollerIndex = int.Parse(name[name.Length - 1] + "");
        Quaternion rotation = Random.rotation;

        rollData.SetDiceRoller(diceRollerIndex);
        rollData.SetDiceRotation(new DiceRotation(rotation.x, rotation.y, rotation.z, rotation.w));
        rollData.SetDiceVelocity(diceVelocity);

        Transform newDice = Instantiate(dice, transform.position, rotation).transform;

        newDice.tag = gameManager.GetAttackDiceTag();
        newDice.GetComponent<Rigidbody>().AddForce(newDice.forward * diceVelocity, ForceMode.VelocityChange);

        return rollData;

    }
}

public class BuildRollRootObject {

    public List<List<RollData>> rollData {

        get; set;

    }

    public BuildRollRootObject() {

        rollData = new List<List<RollData>>();

    }
}

public class AttackRollRootObject {

    public List<List<RollData>> rollData {

        get; set;

    }

    public AttackRollRootObject() {

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