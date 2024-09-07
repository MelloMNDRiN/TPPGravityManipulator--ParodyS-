using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Refrences")]
    [SerializeField] private TMP_Text TimerText;  // UI text for displaying the timer
    [SerializeField] private TMP_Text ScoreText;  // UI text for displaying the score
    [field: SerializeField] public List<GameObject> CollectableCubes { get; private set; } = new List<GameObject>();  // List of collectable items

    public GameObject[] Panels = new GameObject[0];  // Panels for different game states

    [Space(5)]
    public Timer GameTimer;  // Timer to track game duration

    private int Collected = 0;  // Number of items collected
    private int RemainingCollectable => CollectableCubes.Count;  // Remaining collectables

    [Space(5)]
    [Header("Game-Events")]
    public UnityEvent OnGameOver;  // Event triggered on game over
    public UnityEvent OnGameVictory;  // Event triggered on game victory
    public UnityEvent OnGameRestart;  // Event triggered on game restart

    void Start()
    {
        GameTimer = new Timer(120f);  // Initialize timer with 120 seconds
        GameTimer.StartTimer();
        AddScore(0);  // Initialize score
        EnablePanel(1);  // Show the main game panel
    }

    void Update()
    {
        GameTimer.Update(Time.deltaTime);  // Update timer each frame
        TimerText.text = GameTimer.ToString();  // Update timer display
    }

    public void AddScore(int score = 1)
    {
        this.Collected += score;  // Increase collected score
        ScoreText.text = $"Score : {this.Collected} / {RemainingCollectable}";  // Update score display

        if (RemainingCollectable == 0)
        {
            GameVictory();  // Check for game victory
        }
    }

    public void EnablePanel(int index = 0)
    {
        // Activate the panel at the given index and deactivate others
        for (int i = 1; i < Panels.Length; i++)
        {
            Panels[i].SetActive(i == index);
        }
    }

    public void GameVictory()
    {
        Debug.Log("Game Victory");  // Log victory
        OnGameVictory?.Invoke();  // Trigger victory event

        EnablePanel(2);  // Show victory panel
    }

    public void GameOver()
    {
        Debug.Log("Game Over");  // Log game over
        GameTimer.StopTimer();  // Stop the timer
        OnGameOver?.Invoke();  // Trigger game over event

        EnablePanel(3);  // Show game over panel
    }

    public void RestartGame()
    {
        OnGameRestart?.Invoke();  // Trigger restart event

        string currentSceneName = SceneManager.GetActiveScene().name;  // Get current scene name
        SceneManager.LoadScene(currentSceneName);  // Reload current scene
    }
}
