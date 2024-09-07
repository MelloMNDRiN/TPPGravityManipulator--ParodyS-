using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private TMP_Text ScoreText;

    [field: SerializeField] public List<GameObject> CollectableCubes { get; private set; } = new List<GameObject>();

    public Timer GameTimer;

    private int Collected = 0;
    private int RemainingCollectable => CollectableCubes.Count;


    public UnityEvent OnGameOver;
    public UnityEvent OnGameVictory;
    public UnityEvent OnGameRestart;

    void Start()
    {
        GameTimer = new Timer(120f);
        GameTimer.StartTimer();
        AddScore(0);
    }


    void Update()
    {
        GameTimer.Update(Time.deltaTime);

        TimerText.text = GameTimer.ToString();
    }
    public void AddScore(int score = 1)
    {
        this.Collected += score;
        ScoreText.text =  $"Score : {this.Collected} / {RemainingCollectable}";

        if(RemainingCollectable == 0)
        {
            GameVictory();
        }
    }

    public void GameVictory()
    {
        Debug.Log("Game Victory");
        OnGameVictory?.Invoke();
    }
    public void GameOver()
    {
        Debug.Log("Game Over");
        GameTimer.StopTimer();

        OnGameOver?.Invoke();
    }
    public void RestartGame()
    {


        OnGameRestart?.Invoke();
    }
}
