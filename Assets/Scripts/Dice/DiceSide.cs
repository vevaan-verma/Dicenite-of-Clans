using Photon.Pun;
using UnityEngine;

public class DiceSide : MonoBehaviour {

    [Header("References")]
    private PlayerData playerData;
    private DiceController diceController;
    private DiceUIController diceUIController;

    private void Awake() {

        playerData = FindObjectOfType<PlayerData>();
        diceController = transform.parent.GetComponent<DiceController>();
        diceUIController = FindObjectOfType<DiceUIController>();

    }

    private void OnTriggerStay(Collider collider) {

        if (!diceUIController.GetTestingModeState()) {

            if (diceController.diceStill && collider.CompareTag("DiceGround")) {

                diceController.UIController.StartFadeInDiceHUD();
                diceController.rollNumber = 7 - (transform.name[transform.name.Length - 1] - '0');

                if (transform.parent.CompareTag("BuildersDice")) {

                    switch (diceController.rollNumber) {

                        case 1:

                        case 2:

                        case 3:

                        playerData.AddWood(diceController.rollNumber);
                        diceUIController.UpdateWoodCount();
                        break;

                        case 4:

                        case 5:

                        playerData.AddBrick(diceController.rollNumber);
                        diceUIController.UpdateBrickCount();
                        break;

                        case 6:

                        playerData.AddMetal(diceController.rollNumber);
                        diceUIController.UpdateMetalCount();
                        break;

                    }
                } else {

                    switch (diceController.rollNumber) {

                        case 1:

                        playerData.RemoveHealth(1);
                        diceUIController.UpdateHealthSlider();
                        break;

                        case 2:

                        playerData.RemoveHealth(2);
                        diceUIController.UpdateHealthSlider();
                        break;

                        case 3:

                        playerData.RemoveHealth(3);
                        diceUIController.UpdateHealthSlider();
                        break;

                        case 4:

                        playerData.RemoveHealth(4);
                        diceUIController.UpdateHealthSlider();
                        break;

                        case 5:

                        playerData.RemoveHealth(5);
                        diceUIController.UpdateHealthSlider();
                        break;

                        case 6:

                        playerData.RemoveHealth(6);
                        diceUIController.UpdateHealthSlider();
                        break;

                    }
                }

                diceController.diceUsed = true;

                Transform parent = transform.parent;
                Transform child;
                DiceSide diceSide;

                for (int i = 0; i < parent.childCount; i++) {

                    child = parent.GetChild(i);
                    diceSide = child.GetComponent<DiceSide>();

                    if (diceSide != null) {

                        Destroy(child.GetComponent<SphereCollider>());
                        Destroy(diceSide);

                    }
                }

                bool found = false;

                foreach (DiceController dice in FindObjectsOfType<DiceController>()) {

                    if (!dice.diceUsed) {

                        found = true;
                        break;

                    }
                }

                if (!found) {

                    foreach (DiceController dice in FindObjectsOfType<DiceController>()) {

                        dice.ShowDicePopup();

                    }

                    diceController.UIController.EnableRollButtons();

                }
            }
        } else {

            if (diceController.diceStill && collider.CompareTag("DiceGround")) {

                diceController.rollNumber = 7 - (transform.name[transform.name.Length - 1] - '0');
                diceController.diceUsed = true;

                Transform parent = transform.parent;
                Transform child;
                DiceSide diceSide;

                for (int i = 0; i < parent.childCount; i++) {

                    child = parent.GetChild(i);
                    diceSide = child.GetComponent<DiceSide>();

                    if (diceSide != null) {

                        Destroy(child.GetComponent<SphereCollider>());
                        Destroy(diceSide);

                    }
                }

                bool found = false;

                foreach (DiceController dice in FindObjectsOfType<DiceController>()) {

                    if (!dice.diceUsed) {

                        found = true;
                        break;

                    }
                }

                if (!found) {

                    foreach (DiceController dice in FindObjectsOfType<DiceController>()) {

                        dice.ShowDicePopup();

                    }
                }

                diceUIController.FinishTestingRolls();

            }
        }
    }
}
