using System;
using UnityEngine;

[Serializable]
public class Timer
{
    public event Action OnTimerStarted;  // Event for when the timer starts
    public event Action<float> OnTimerTick;  // Event for each tick of the timer
    public event Action OnTimerFinished;  // Event for when the timer finishes
    public event Action OnTimerPaused;  // Event for when the timer is paused
    public event Action OnTimerResumed;  // Event for when the timer resumes

    [SerializeField] private float InitialDuration;  // The initial duration of the timer
    [SerializeField] private float Duration;  // The current duration of the timer
    [SerializeField] private float TimeRemaining;  // How much time is left
    [SerializeField] public bool IsRunning;  // Is the timer currently running?
    [SerializeField] private bool IsPaused;  // Is the timer currently paused?

    public Timer(float initialDuration)
    {
        InitialDuration = initialDuration;
        Duration = initialDuration;
        TimeRemaining = initialDuration;
    }

    public void StartTimer(float duration)
    {
        Duration = duration;
        TimeRemaining = duration;
        IsRunning = true;
        IsPaused = false;

        OnTimerStarted?.Invoke();  // Notify that the timer has started
    }

    public void StartTimer()
    {
        StartTimer(InitialDuration);  // Start timer with initial duration
    }

    public void StopTimer()
    {
        IsRunning = false;  // Stop the timer
    }

    public void PauseTimer()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            OnTimerPaused?.Invoke();  // Notify that the timer has been paused
        }
    }

    public void ResumeTimer()
    {
        if (IsRunning && IsPaused)
        {
            IsPaused = false;
            OnTimerResumed?.Invoke();  // Notify that the timer has resumed
        }
    }

    public void ResetTimer()
    {
        TimeRemaining = InitialDuration;  // Reset time to initial duration
        IsRunning = false;  // Stop the timer
        IsPaused = false;  // Make sure timer is not paused
    }

    public void SetDuration(float newDuration)
    {
        Duration = newDuration;
        if (IsRunning && !IsPaused)
        {
            TimeRemaining = newDuration;  // Update time remaining if timer is running
        }
    }

    public void Update(float deltaTime)
    {
        if (IsRunning && !IsPaused)
        {
            TimeRemaining -= deltaTime;  // Decrease time remaining by deltaTime

            OnTimerTick?.Invoke(TimeRemaining);  // Notify about the current time remaining

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning = false;  // Stop the timer if time is up

                OnTimerFinished?.Invoke();  // Notify that the timer has finished
            }
        }
    }

    public override string ToString()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(TimeRemaining);
        return timeSpan.ToString(@"hh\:mm\:ss");  // Format time as hh:mm:ss
    }
}
