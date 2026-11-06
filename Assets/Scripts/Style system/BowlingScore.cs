using System.Collections;
using UnityEngine;

public class BowlingScore : MonoBehaviour
{
    public static BowlingScore Instance { get; private set; }

    [Header("Настройки боулинга")]
    [Tooltip("Количество помидоров, необходимое для страйка.")]
    [field: SerializeField] public int PinsForStrike { get; private set; } = 5; // Изменено на 5 для страйка

    [Tooltip("Время (в секундах) на падение остальных помидоров после первого сбитого.")]
    [field: SerializeField] public float CalculationWindow { get; private set; } = 2.5f;

    private struct ThrowData
    {
        public bool inJump;
        public float linearVelovity;
        public float angularVelovity;
        public float multiplier;
        private Jump cachedJump;

        public void RegisterThrow(Rigidbody player)
        {
            if (cachedJump == null && player != null)
                cachedJump = player.GetComponent<Jump>();

            inJump = cachedJump != null && cachedJump.InJump;
            linearVelovity = player.linearVelocity.magnitude;
            angularVelovity = player.angularVelocity.magnitude;
        }

        public void Clear()
        {
            inJump = false;
            linearVelovity = 0f;
            angularVelovity = 0f;
            multiplier = 1f;
        }
    }

    private ThrowData throwData = new();
    private int knockedPinsThisThrow = 0;
    private bool isCalculating = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void SetValuesOnThrow(Rigidbody player, float multiplier = 1f)
    {
        if (Instance == null || player == null) return;
        
        Instance.throwData.RegisterThrow(player);
        Instance.throwData.multiplier = multiplier;
    }

    // Этот метод теперь будет вызываться из триггера
    public void RegisterPinHit()
    {
        knockedPinsThisThrow++;
        
        // Выводим количество упавших помидоров в консоль/UI (временно или постоянно)
        Debug.Log($"Упало помидоров: {knockedPinsThisThrow}"); 
        
        if (!isCalculating)
        {
            StartCoroutine(CalculateScoreRoutine());
        }
    }

    private IEnumerator CalculateScoreRoutine()
    {
        isCalculating = true;
        yield return new WaitForSeconds(CalculationWindow);

        bool isStrike = knockedPinsThisThrow == PinsForStrike;
        float baseScore = throwData.linearVelovity * 2f + throwData.angularVelovity;
        
        if (throwData.inJump) baseScore *= 1.5f;
        
        // Пересчет финального счета. 
        int finalScore = (int)Mathf.Round(baseScore * throwData.multiplier) + (knockedPinsThisThrow * 10);
        
        if (isStrike) finalScore *= 2; 

        // Обновленное сообщение, учитывающее помидоры
        string trickName = isStrike ? "СТРАЙК!" : $"Сбито помидоров: {knockedPinsThisThrow}";
        
        if (StyleManager.Instance != null)
            StyleManager.Instance.AddStylePoints(finalScore, trickName);
        else
            UIMessage.Show($"{trickName} \nПолучено {finalScore} очков стиля", 1);

        knockedPinsThisThrow = 0;
        isCalculating = false;
        throwData.Clear();
    }
}
