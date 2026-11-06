using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Главный менеджер системы очков стиля и комбо.
/// Накапливает общий счёт, отслеживает интервал между трюками и увеличивает комбо-множитель.
/// </summary>
public class StyleManager : MonoBehaviour
{
    public static StyleManager Instance { get; private set; }

    [Header("Настройки комбо")]
    [Tooltip("Максимальное время (в секундах) между трюками для поддержания серии комбо.")]
    [field: SerializeField] public float ComboTimeout { get; private set; } = 3.5f;

    [Tooltip("Шаг увеличения множителя за каждый уровень комбо.")]
    [field: SerializeField] public float MultiplierStep { get; private set; } = 0.25f;

    [Tooltip("Максимальный возможный множитель комбо.")]
    [field: SerializeField] public float MaxMultiplier { get; private set; } = 4.0f;

    [Header("Текущие показатели")]
    public int TotalScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public float CurrentMultiplier { get; private set; } = 1.0f;

    [Header("События UI")]
    // Событие для обновления UI (передаем общий счёт и текущий множитель)
    public UnityEvent<int, float> OnStylePointsChanged;

    private float comboTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (CurrentCombo <= 0)
            return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            ResetCombo();
        }
    }

    /// <summary>
    /// Принимает базовые очки от мини-игр (баскетбол, боулинг),
    /// применяет комбо-множитель и обновляет UI.
    /// </summary>
    /// <param name="baseScore">Базовые очки за трюк.</param>
    /// <param name="trickName">Название трюка для текстового уведомления.</param>
    public void AddStylePoints(int baseScore, string trickName = "Попадание!")
    {
        if (baseScore <= 0)
            return;

        // Если это не первый трюк в серии, увеличиваем множитель.
        if (CurrentCombo > 0)
        {
            CurrentMultiplier = Mathf.Min(
                MaxMultiplier,
                CurrentMultiplier + MultiplierStep
            );
        }
        else
        {
            CurrentMultiplier = 1.0f;
        }

        CurrentCombo++;
        comboTimer = ComboTimeout;

        int finalScore = Mathf.RoundToInt(baseScore * CurrentMultiplier);
        TotalScore += finalScore;

        OnStylePointsChanged?.Invoke(TotalScore, CurrentMultiplier);

        string comboInfo = CurrentCombo > 1
            ? $"\nКомбо x{CurrentCombo} (Множитель x{CurrentMultiplier:0.##})"
            : string.Empty;

        UIMessage.Show($"{trickName} \nПолучено {finalScore} очков стиля{comboInfo}", 1);
    }

    /// <summary>
    /// Сбрасывает серию комбо и возвращает множитель к 1.
    /// </summary>
    public void ResetCombo()
    {
        CurrentCombo = 0;
        CurrentMultiplier = 1.0f;
        comboTimer = 0f;

        OnStylePointsChanged?.Invoke(TotalScore, CurrentMultiplier);
    }
}
