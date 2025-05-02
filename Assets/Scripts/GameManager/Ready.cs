using System;
using UnityEngine;

public class Ready : MonoBehaviour
{
    [SerializeField]
    private GameObject CountDownTimer;
    [SerializeField]
    private CountDownTimerController countDownTimerController;

    private void Start()
    {
        countDownTimerController = CountDownTimer.GetComponent<CountDownTimerController>();
    }

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }

    private void GameManagerOnGameStateChanged(GameState state)
    {
        StartCoroutine(countDownTimerController.CountDownToStart());
    }
}
