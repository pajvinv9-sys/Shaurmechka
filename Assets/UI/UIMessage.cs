using UnityEngine;
using TMPro;
using System.Collections;

public class UIMessage : MonoBehaviour
{
    public static UIMessage Instance;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text helpText;
    private Coroutine coroutine;
    private bool isInfinityMessage;
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

    public static void ShowHelp(string message)
    {
        Instance.helpText.text = message;
        Instance.helpText.gameObject.SetActive(true);
    }

    private IEnumerator ShowCoroutine(string message, float showTime)
    {
        if (Instance == null) yield break;

        Instance.messageText.text = message;
        Instance.messageText.gameObject.SetActive(true);

        if (showTime >= 0)
        {
            isInfinityMessage = false;
            yield return new WaitForSeconds(showTime);

            Hide();
        }
        else isInfinityMessage = true;
    }

    public static void Hide()
    {
        if (Instance == null) return;

        Instance.messageText.gameObject.SetActive(false);
    }

    public static void HideHelp()
    {
        if (Instance == null) return;

        Instance.helpText.gameObject.SetActive(false);
    }

    public static void ShowAndReturn(string message, float time)
    {
        if (Instance == null) return;
        
        Instance.StartCoroutine(Instance.ShowEndReturnCoroutine(message, time));
    }

    private IEnumerator ShowEndReturnCoroutine(string message, float showTime)
    {
        var nowMessage = Instance.messageText.text;

        if (Instance == null) yield break;

        Instance.messageText.text = message;
        Instance.messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(showTime > 0 ? showTime : 0);

        Hide();

        if (isInfinityMessage) Show(nowMessage, -1);

    }
}
