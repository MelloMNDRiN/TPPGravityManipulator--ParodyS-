using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    [Header("Refrences")]
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private TMP_Text ScoreText;
    [field: SerializeField] public List<GameObject> CollectableCubes { get; private set; } = new List<GameObject>();

    public GameObject[] Panels = new GameObject[0];

    [Space(5)]
    public Timer GameTimer;

    private int Collected = 0;
    private int RemainingCollectable => CollectableCubes.Count;

    
    [Space(5)]
    [Header("Game-Events")]
    public UnityEvent OnGameOver;
    public UnityEvent OnGameVictory;
    public UnityEvent OnGameRestart;

    void Start()
    {
        GameTimer = new Timer(120f);
        GameTimer.StartTimer();
        AddScore(0);
        EnablePanel(1);
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

    public void EnablePanel(int index = 0)
    {
        for(int i = 1; i < Panels.Length; i++)
        {
            Panels[i].SetActive(i == index);
        }
    }
    public void GameVictory()
    {
        Debug.Log("Game Victory");
        OnGameVictory?.Invoke();

        EnablePanel(2);
    }
    public void GameOver()
    {
        Debug.Log("Game Over");
        GameTimer.StopTimer();
        OnGameOver?.Invoke();

        EnablePanel(3);
    }
    public void RestartGame()
    {
        OnGameRestart?.Invoke();

        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }
}
