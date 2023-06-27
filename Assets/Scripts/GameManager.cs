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
    [SerializeField] private int buildersDice;
    [SerializeField] private int attackDice;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private string diceRollFileName;
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

    public int GetBuildersDice() {

        return buildersDice;

    }

    public int GetAttackDice() {

        return attackDice;

    }

    public int GetGridWidth() {

        return gridWidth;

    }

    public int GetGridHeight() {

        return gridHeight;

    }

    public string GetDiceRollFilePath() {

        return Application.persistentDataPath + Path.DirectorySeparatorChar + diceRollFileName;

    }

    public int GetMaxPlayers() {

        return maxPlayers;

    }
}