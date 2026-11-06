using UnityEngine;
using TMPro;
using System.Collections;

public class UIMessage : MonoBehaviour
{
    public static UIMessage Instance;

    [SerializeField] private TMP_Text messageText;
    private Coroutine coroutine;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void Show(string message, float showTime)
    {
        if (Instance.coroutine != null) Instance.StopCoroutine(Instance.coroutine);
        Hide();
        Instance.coroutine = Instance.StartCoroutine(Instance.ShowCoroutine(message, showTime));
    }

    private IEnumerator ShowCoroutine(string message, float showTime)
    {
        if (Instance == null) yield break;

        Instance.messageText.text = message;
        Instance.messageText.gameObject.SetActive(true);

        if (showTime >= 0)
        {
            yield return new WaitForSeconds(showTime);

            Hide();
        }
        
    }

    public static void Hide()
    {
        if (Instance == null) return;

        Instance.messageText.gameObject.SetActive(false);
    }
}
