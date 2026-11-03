using UnityEngine;

/// <summary>
/// Рассчитывает базовые очки стиля за броски котлеты в кольцо
/// и передаёт их в StyleManager.
/// </summary>
public class BasketballScore : MonoBehaviour
{
    public static BasketballScore Instance { get; private set; }

    private struct ThrowData
    {
        public bool inJump;
        public float linearVelovity;
        public float angularVelovity;
        public Vector3 playerPosition;
        public Vector3 hoopPosition;
        public float multiplier;

        public float distance => (playerPosition - hoopPosition).magnitude;

        public void RegisterThrow(Rigidbody player)
        {
            inJump = player.GetComponent<Jump>().InJump;
            playerPosition = player.transform.position;
            linearVelovity = player.linearVelocity.magnitude;
            angularVelovity = player.angularVelocity.magnitude;
        }

        public void RegisterBasket(Transform hoop)
        {
            hoopPosition = hoop.transform.position;
        }

        public void Clear()
        {
            inJump = false;
            playerPosition = Vector3.zero;
            linearVelovity = 0;
            angularVelovity = 0;
            multiplier = 1f;
        }
    }

    private ThrowData throwData = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddScore(Transform BasketballHoop, Cutlet cutlet)
    {
        throwData.hoopPosition = BasketballHoop.position;
        int score = 0;

        if (Grab.Instance.Item != cutlet)
        {
            score = CalculateScore();
        }
        else
        {
            SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
            score = Mathf.Max(1, CalculateScore() / 5);
        }

        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.AddStylePoints(score, "Попадание!");
        }
        else
        {
            UIMessage.Show($"Попадание! \nПолучено {score} очков стиля");
        }
    }

    public static void SetValuesOnThrow(Rigidbody player, float multyply = 1f)
    {
        if (Instance == null || player == null)
            return;

        Instance.throwData.RegisterThrow(player);
        Instance.throwData.multiplier = multyply;
    }

    public int CalculateScore()
    {
        float calc = throwData.linearVelovity * 2f + throwData.angularVelovity + throwData.distance * 3f;

        if (throwData.inJump)
            calc *= 1.5f;

        float currentMultiplier = throwData.multiplier;
        throwData.Clear();

        return (int)Mathf.Round(calc * currentMultiplier);
    }
}
