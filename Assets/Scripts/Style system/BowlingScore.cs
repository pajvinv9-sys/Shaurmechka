using UnityEngine;

/// <summary>
/// Рассчитывает базовые очки стиля за сбивание целей шаром для боулинга
/// и передаёт их в StyleManager.
/// </summary>
public class BowlingScore : MonoBehaviour
{
    public static BowlingScore Instance { get; private set; }

    private struct ThrowData
    {
        public bool inJump;
        public float linearVelovity;
        public float angularVelovity;
        public Vector3 playerPosition;
        public Vector3 targetPosition;
        public float multiplier;

        public float distance => (playerPosition - targetPosition).magnitude;

        public void RegisterThrow(Rigidbody player)
        {
            inJump = player.GetComponent<Jump>().InJump;
            playerPosition = player.transform.position;
            linearVelovity = player.linearVelocity.magnitude;
            angularVelovity = player.angularVelocity.magnitude;
        }

        public void RegisterTarget(Transform target)
        {
            targetPosition = target.transform.position;
        }

        public void Clear()
        {
            inJump = false;
            playerPosition = Vector3.zero;
            targetPosition = Vector3.zero;
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

    public void AddScore(Transform target, BoulingBall ball)
    {
        throwData.RegisterTarget(target);
        int score = 0;

        float ballSpeed = ball != null && ball.Rigidbody != null
            ? ball.Rigidbody.linearVelocity.magnitude
            : 0f;

        if (Grab.Instance.Item != ball)
        {
            score = CalculateScore(ballSpeed);
        }
        else
        {
            SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
            score = Mathf.Max(1, CalculateScore(ballSpeed) / 5);
        }

        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.AddStylePoints(score, "Страйк!");
        }
        else
        {
            UIMessage.Show($"Страйк! \nПолучено {score} очков стиля");
        }
    }

    public static void SetValuesOnThrow(Rigidbody player, float multyply = 1f)
    {
        if (Instance == null || player == null)
            return;

        Instance.throwData.RegisterThrow(player);
        Instance.throwData.multiplier = multyply;
    }

    public int CalculateScore(float ballSpeed = 0f)
    {
        float calc =
            throwData.linearVelovity * 2f +
            throwData.angularVelovity +
            throwData.distance * 2.5f +
            ballSpeed * 1.5f;

        if (throwData.inJump)
            calc *= 1.5f;

        float currentMultiplier = throwData.multiplier;
        throwData.Clear();

        return (int)Mathf.Round(calc * currentMultiplier);
    }
}
