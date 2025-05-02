using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState gameState;
    public static event Action<GameState> OnGameStateChanged;
    public TextMeshProUGUI Lose;
    public TextMeshProUGUI Win;
    [SerializeField]
    private TrackCivilians trackCivilians;
    [SerializeField]
    private TrackAliens trackAliens;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.Ready);
    }

    public void UpdateGameState(GameState newState)
    {
        gameState = newState;

        switch (newState)
        {
            case GameState.Ready:
                break;
            case GameState.StartWave:
                trackCivilians.RegisterAllCivilians();
                trackAliens.RegisterAllAliens();
                break;
            case GameState.Win:
                StateWin();
                break;
            case GameState.Lose:
                StateLose();
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    private void StateWin()
    {
        Win.gameObject.SetActive(true);
    }

    private void StateLose()
    {
        Lose.gameObject.SetActive(true);
    }
}

public enum GameState
{
    Ready,
    StartWave,
    Win,
    Lose
}