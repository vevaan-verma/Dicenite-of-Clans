using UnityEngine;
using TMPro;
using Photon.Pun;

public class DiceController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform center;
    [HideInInspector] public DiceUIController diceUIController;
    private PhotonView roller;
    private GameManager gameManager;
    private Rigidbody rb;

    [Header("Dice Check")]
    [HideInInspector] public int rollNumber;
    [HideInInspector] public bool diceStill;
    [HideInInspector] public float diceStillTimer;
    [HideInInspector] public bool diceUsed;
    private Vector3 diceVelocity;

    [Header("Dice Popup")]
    [SerializeField] private GameObject dicePopup;
    [SerializeField] private float diceRadius;

    private void Start() {

        diceUIController = FindObjectOfType<DiceUIController>();
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();

    }

    private void FixedUpdate() {

        diceVelocity = rb.velocity;

        if (diceVelocity.x == 0f && diceVelocity.y == 0f && diceVelocity.z == 0f && !diceUsed) {

            diceStillTimer += Time.fixedDeltaTime;

            if (diceStillTimer >= gameManager.GetDiceStillTime()) {

                diceStillTimer = 0f;
                diceStill = true;

            }
        } else {

            diceStillTimer = 0f;
            diceStill = false;

        }
    }

    public void ShowDicePopup() {

        Instantiate(dicePopup, new Vector3(center.position.x, center.position.y + diceRadius, center.position.z), Quaternion.Euler(90f, 0f, 0f), transform).GetComponent<TextMeshPro>().text = rollNumber + "";

    }

    public void SetRoller(PhotonView roller) {

        this.roller = roller;

    }

    public PhotonView GetRoller() {

        return roller;

    }

    public GameManager GetGameManager() {

        return gameManager;

    }
}
