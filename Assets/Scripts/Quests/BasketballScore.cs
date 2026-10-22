using UnityEngine;

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
        var score = 0f;
        bool inHands = false;

        if (Grab.Instance.Item != cutlet)
        {
            score = CalculateScore();
        }
        else
        {
            inHands = true;

            SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
            score = Mathf.Max(1, CalculateScore() / 5);
        }

        UIMessage.Show($"Попадание! \nПолучено {score} очков стиля");
    }

    public static void SetValuesOnThrow(Rigidbody player, float multyply=1)
    {
        Instance.throwData.RegisterThrow(player);
        Instance.throwData.multiplier = multyply;
    }

    public int CalculateScore()
    {
        float calc = throwData.linearVelovity * 2f + throwData.angularVelovity + throwData.distance * 3f;

        if (throwData.inJump) calc *= 1.5f;
        
        throwData.Clear();

        return (int)Mathf.Round(calc * throwData.multiplier);
    }
}
