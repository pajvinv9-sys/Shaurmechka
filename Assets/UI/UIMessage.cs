using UnityEngine;
using TMPro;

public class UIMessage : MonoBehaviour
{
    public static UIMessage Instance;

    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void Show(string message)
    {
        if (Instance == null) return;

        Instance.messageText.text = message;
        Instance.messageText.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (Instance == null) return;

        Instance.messageText.gameObject.SetActive(false);
    }
}
