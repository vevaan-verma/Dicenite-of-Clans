using UnityEngine;

public class PlayerData : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float moveDuration;

    [Header("Data")]
    private int health;
    private int wood;
    private int brick;
    private int metal;

    private void Awake() {

        DontDestroyOnLoad(gameObject);

        health = maxHealth;

    }

    public int GetMaxHealth() {

        return maxHealth;

    }

    public float GetMoveDuration() {

        return moveDuration;

    }

    public int GetHealth() {

        return health;

    }

    public void AddHealth(int health) {

        this.health += health;

    }

    public void RemoveHealth(int health) {

        this.health -= health;

    }

    public int GetWoodCount() {

        return wood;

    }

    public void AddWood(int wood) {

        this.wood += wood;

    }

    public void RemoveWood(int wood) {

        this.wood -= wood;

    }

    public int GetBrickCount() {

        return brick;

    }

    public void AddBrick(int brick) {

        this.brick += brick;

    }

    public void RemoveBrick(int brick) {

        this.brick -= brick;

    }

    public int GetMetalCount() {

        return metal;

    }

    public void AddMetal(int metal) {

        this.metal += metal;

    }

    public void RemoveMetal(int metal) {

        this.metal -= metal;

    }
}
