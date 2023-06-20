using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private int maxHealth;
    private int health;
    public int wood;
    public int brick;
    public int metal;

    private void Awake() {

        DontDestroyOnLoad(gameObject);

        health = maxHealth;

    }

    public int GetMaxHealth() {

        return maxHealth;

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
