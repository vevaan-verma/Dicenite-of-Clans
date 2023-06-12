using UnityEngine;

public class PlayerData : MonoBehaviour {

    [Header("References")]
    private DiceUIController UIController;

    [Header("Data")]
    [SerializeField] private int maxHealth;
    private int health;
    private int wood;
    private int brick;
    private int metal;

    private void Start() {

        UIController = FindObjectOfType<DiceUIController>();
        health = 100;

    }

    public int GetMaxHealth() {

        return maxHealth;

    }

    public int GetHealth() {

        return health;

    }

    public void AddHealth(int health) {

        this.health += health;
        UIController.UpdateHealthSlider(this.health);

    }

    public void RemoveHealth(int health) {

        this.health -= health;
        UIController.UpdateHealthSlider(this.health);

    }

    public int GetWood() {

        return wood;

    }

    public void AddWood(int wood) {

        this.wood += wood;
        UIController.UpdateWoodCount(this.wood);

    }

    public int GetBrick() {

        return brick;

    }

    public void AddBrick(int brick) {

        this.brick += brick;
        UIController.UpdateBrickCount(this.brick);

    }

    public int GetMetal() {

        return metal;

    }

    public void AddMetal(int metal) {

        this.metal += metal;
        UIController.UpdateMetalCount(this.metal);

    }
}
