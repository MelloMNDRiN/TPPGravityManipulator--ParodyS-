using System;
using UnityEngine;

[Serializable]
public class Timer
{
    public event Action OnTimerStarted;
    public event Action<float> OnTimerTick;
    public event Action OnTimerFinished;
    public event Action OnTimerPaused;
    public event Action OnTimerResumed;

    [SerializeField] private float InitialDuration;
    [SerializeField] private float Duration;
    [SerializeField] private float TimeRemaining;
    [SerializeField] public bool IsRunning;
    [SerializeField] private bool IsPaused;

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

        OnTimerStarted?.Invoke();
    }

    public void StartTimer()
    {
        StartTimer(InitialDuration);
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public void PauseTimer()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            OnTimerPaused?.Invoke();
        }
    }

    public void ResumeTimer()
    {
        if (IsRunning && IsPaused)
        {
            IsPaused = false;
            OnTimerResumed?.Invoke();
        }
    }

    public void ResetTimer()
    {
        TimeRemaining = InitialDuration;
        IsRunning = false;
        IsPaused = false;
    }

    public void SetDuration(float newDuration)
    {
        Duration = newDuration;
        if (IsRunning && !IsPaused)
        {
            TimeRemaining = newDuration;
        }
    }

    public void Update(float deltaTime)
    {
        if (IsRunning && !IsPaused)
        {
            TimeRemaining -= deltaTime;

            OnTimerTick?.Invoke(TimeRemaining);

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning = false;

                OnTimerFinished?.Invoke();
            }
        }
    }

    public override string ToString()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(TimeRemaining);
        return timeSpan.ToString(@"hh\:mm\:ss");
    }
}
