using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class BowlingTargetArea : MonoBehaviour
{
    private HashSet<Rigidbody> countedTomatoes = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            // Проверяем, не является ли упавший объект шаром для боулинга.
            // Шар мы не засчитываем.
            if (rb.GetComponent<BoulingBall>() != null)
                return;

            if (!countedTomatoes.Contains(rb))
            {
                countedTomatoes.Add(rb);

                Debug.Log($"[Боулинг] Засчитано падение: {rb.gameObject.name}");
                
                if (BowlingScore.Instance != null)
                {
                    BowlingScore.Instance.RegisterPinHit();
                }
                else
                {
                    Debug.LogError("[Боулинг] Ошибка: Объект с BowlingScore не найден на сцене!");
                }
            }
        }
    }

    public void ResetArea()
    {
        countedTomatoes.Clear();
    }
}
