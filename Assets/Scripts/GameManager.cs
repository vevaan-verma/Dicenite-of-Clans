using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("State")]
    private GameState gameState;

    [Header("Settings")]
    [SerializeField] private float diceStillTime;
    [SerializeField] private string diceGroundTag;
    [SerializeField] private int buildDiceAmount;
    [SerializeField] private int attackDiceAmount;
    [SerializeField] private string buildDiceTag;
    [SerializeField] private string attackDiceTag;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private string buildDiceRollFileName;
    [SerializeField] private string attackDiceRollFileName;
    [SerializeField] private int maxPlayers;

    public enum GameState {

        None, Waiting, Live

    }

    public enum MaterialType {

        Wood, Brick, Metal

    }

    private void Start() {

        DontDestroyOnLoad(gameObject);
        gameState = GameState.None;

    }

    public void UpdateGameState(GameState state) {

        gameState = state;

    }

    public GameState GetGameState() {

        return gameState;

    }

    public float GetDiceStillTime() {

        return diceStillTime;

    }

    public string GetDiceGroundTag() {

        return diceGroundTag;

    }

    public int GetBuildDiceAmount() {

        return buildDiceAmount;

    }

    public int GetAttackDiceAmount() {

        return attackDiceAmount;

    }

    public string GetBuildDiceTag() {

        return buildDiceTag;

    }

    public string GetAttackDiceTag() {

        return attackDiceTag;

    }

    public int GetGridWidth() {

        return gridWidth;

    }

    public int GetGridHeight() {

        return gridHeight;

    }

    public string GetBuildDiceRollFilePath() {

        return Application.persistentDataPath + Path.DirectorySeparatorChar + buildDiceRollFileName;

    }

    public string GetAttackDiceRollFilePath() {

        return Application.persistentDataPath + Path.DirectorySeparatorChar + attackDiceRollFileName;

    }

    public int GetMaxPlayers() {

        return maxPlayers;

    }
}