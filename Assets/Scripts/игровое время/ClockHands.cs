using UnityEngine;

/// <summary>
/// Одна стрелка часов, которая движется "тиками" по игровому времени GameClock.
///
/// Крепится на: объект часов. Стрелка должна вращаться вокруг центра циферблата,
/// поэтому кладите её в пустой объект-«ось» в центре, а сюда назначайте ось.
/// В стартовом положении стрелка должна смотреть на 12.
/// </summary>
public class ClockHands : MonoBehaviour
{
    [field: SerializeField] public Transform Hand { get; private set; }

    [Header("Тиканье")]
    [Tooltip("Размер одного тика в игровых минутах. 1 = стрелка прыгает каждую минуту.")]
    [field: SerializeField] public int TickMinutes { get; private set; } = 5;

    [Tooltip("Время самого прыжка в секундах. 0 = мгновенный щелчок.")]
    [field: SerializeField] public float TickDuration { get; private set; } = 0.08f;

    [Header("Настройка под модель")]
    [Tooltip("Локальная ось, перпендикулярная циферблату. Если стрелка вращается не в той плоскости, поменяйте.")]
    [field: SerializeField] public Vector3 Axis { get; private set; } = Vector3.forward;
    [Tooltip("Включите, если стрелка идёт против часовой.")]
    [field: SerializeField] public bool Reverse { get; private set; }

    private Quaternion startRotation;
    private float currentAngle;
    private float targetAngle;
    private int lastTick = -1;

    private void Awake()
    {
        startRotation = Hand.localRotation;
    }

    private void Update()
    {
        if (GameClock.Instance == null)
            return;

        GameClock clock = GameClock.Instance;

        // Сколько игровых минут прошло с начала дня, округлённо вниз до шага тика.
        float minutesPassed = (clock.CurrentHour - clock.StartHour) * 60f;
        int tick = Mathf.FloorToInt(minutesPassed / TickMinutes);

        if (tick != lastTick)
        {
            lastTick = tick;

            // Полный круг = весь игровой день.
            // Для обычных часов по времени суток замените dayMinutes на 12f * 60f.
            float dayMinutes = (clock.EndHour - clock.StartHour) * 60f;
            float sign = Reverse ? -1f : 1f;

            targetAngle = tick * TickMinutes / dayMinutes * 360f * sign;

            // Мгновенный щелчок, если анимация прыжка отключена.
            if (TickDuration <= 0f)
                currentAngle = targetAngle;
        }

        if (TickDuration > 0f)
        {
            // Быстрый прыжок к новому углу: скорость подобрана так, чтобы уложиться в TickDuration.
            float step = Mathf.Abs(targetAngle - currentAngle) / TickDuration * Time.deltaTime;
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Mathf.Max(step, 0.01f));
        }

        Hand.localRotation = startRotation * Quaternion.AngleAxis(currentAngle, Axis);
    }
}