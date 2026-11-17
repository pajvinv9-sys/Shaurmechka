using System;
using UnityEngine;

/// <summary>
/// Игровое время. Ведёт отсчёт от StartHour до EndHour за DayLengthSeconds реальных секунд.
/// Другие системы (часы, уникальные NPC) только читают время или подписываются на события.
///
/// Крепится на: отдельный объект сцены (GameClock).
/// </summary>
public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }

    [Header("Длина дня")]
    [field: SerializeField] public float StartHour { get; private set; } = 9f;
    [field: SerializeField] public float EndHour { get; private set; } = 21f;
    [field: SerializeField] public float DayLengthSeconds { get; private set; } = 600f;

    /// <summary>Текущее время в часах (например, 13.5 = 13:30).</summary>
    public float CurrentHour { get; private set; }

    /// <summary>Идёт ли время. Можно ставить на паузу: GameClock.Instance.IsRunning = false.</summary>
    public bool IsRunning { get; set; } = true;

    public int Hours => Mathf.FloorToInt(CurrentHour);
    public int Minutes => Mathf.FloorToInt((CurrentHour % 1f) * 60f);

    /// <summary>Время текстом, например "09:05".</summary>
    public string TimeText => $"{Hours:00}:{Minutes:00}";

    /// <summary>Каждая игровая минута. Параметр: минут с начала суток (13:30 = 810).</summary>
    public event Action<int> MinutePassed;

    /// <summary>День закончился (время дошло до EndHour).</summary>
    public event Action DayEnded;

    private int lastMinute;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResetDay();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Начинает день заново.</summary>
    public void ResetDay()
    {
        CurrentHour = StartHour;
        lastMinute = Mathf.FloorToInt(StartHour * 60f);
        IsRunning = true;
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        float hoursPerSecond = (EndHour - StartHour) / DayLengthSeconds;
        CurrentHour += hoursPerSecond * Time.deltaTime;

        if (CurrentHour >= EndHour)
        {
            CurrentHour = EndHour;
            IsRunning = false;
            DayEnded?.Invoke();
            return;
        }

        // Цикл на случай, если за кадр прошло больше одной минуты.
        int minute = Mathf.FloorToInt(CurrentHour * 60f);
        while (lastMinute < minute)
        {
            lastMinute++;
            MinutePassed?.Invoke(lastMinute);
        }
    }
}