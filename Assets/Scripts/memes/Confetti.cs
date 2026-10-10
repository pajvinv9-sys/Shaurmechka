using UnityEngine;

/// <summary>
/// Зона финиша / сдачи, которая взрывает конфетти при попадании интерактивного объекта.
/// 
/// Вешается на: GameObject триггер-зоны (например, в конце конвейера или на столе заказов).
/// Требует: Collider (Is Trigger = true).
/// 
/// Особенности:
/// - Реагирует строго на InteractiveObject (без тегов).
/// - Запускает салют конфетти и праздничный звук.
/// - Имеет кулдаун от спама и опцию уничтожения приехавшего предмета.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ConfettiTriggerZone : MonoBehaviour
{
    [Header("Эффект конфетти")]
    /// <summary>
    /// Система частиц конфетти (готовая на сцене или дочерняя).
    /// Если указана, вызывается ее запуск .Play().
    /// </summary>
    [field: SerializeField]
    public ParticleSystem ConfettiEffect { get; private set; }

    /// <summary>
    /// Альтернатива: префаб конфетти для спавна, если нет готовой системы частиц на сцене.
    /// </summary>
    [field: SerializeField]
    public ParticleSystem ConfettiPrefab { get; private set; }

    /// <summary>
    /// Точка, из которой вылетает конфетти.
    /// Если не указана, вылетает из центра триггера.
    /// </summary>
    public Transform ConfettiSpawnPoint { get; private set; }
    
    
    [Header("Звук")]
    /// <summary>
    /// Звук хлопушки / фанфар / победы.
    /// </summary>
    [field: SerializeField]
    public AudioClip[] CelebrateSounds { get; private set; }
    
    private float lastTriggerTime;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        lastTriggerTime = Time.time;

        // 1. Выстрел конфетти
        TriggerConfetti();

        // 2. Звук хлопушки
        if (CelebrateSounds != null && CelebrateSounds.Length > 0)
        {
            Vector3 soundPos = ConfettiSpawnPoint != null ? ConfettiSpawnPoint.position : transform.position;
    
            // Выбираем случайный индекс
            int randomIndex = Random.Range(0, CelebrateSounds.Length);
    
            // Передаем конкретный случайный клип из массива по индексу
            AudioSource.PlayClipAtPoint(CelebrateSounds[randomIndex], soundPos, 1.0f);
        }
    }

    /// <summary>
    /// Запуск эффекта салюта конфетти.
    /// </summary>
    private void TriggerConfetti()
    {
        Vector3 spawnPos = ConfettiSpawnPoint != null ? ConfettiSpawnPoint.position : transform.position;
        Quaternion spawnRot = ConfettiSpawnPoint != null ? ConfettiSpawnPoint.rotation : Quaternion.identity;

        // Если привязана система частиц на сцене
        if (ConfettiEffect != null)
        {
            ConfettiEffect.transform.position = spawnPos;
            ConfettiEffect.Play();
        }
        // Либо спавним копию префаба частиц
        else if (ConfettiPrefab != null)
        {
            ParticleSystem vfx = Instantiate(ConfettiPrefab, spawnPos, spawnRot);
            Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Отрисовка зоны триггера и точки вылета в окне Scene
        Gizmos.color = new Color(1f, 0.84f, 0f, 0.4f); // Золотистый цвет финиша

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        Vector3 spawnPos = ConfettiSpawnPoint != null ? ConfettiSpawnPoint.position : transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(spawnPos, 0.25f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
#endif
}