using TMPro;
using UnityEngine;

/// <summary>
/// Отображает окно диалога. Не содержит логики листания: ей занимается DialogueManager.
///
/// Крепится на: объект, который не выключается (родитель панели диалога на Canvas).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [field: SerializeField] public GameObject Panel { get; private set; }
    [field: SerializeField] public TMP_Text SpeakerText { get; private set; }
    [field: SerializeField] public TMP_Text BodyText { get; private set; }

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        Panel.SetActive(true);
    }

    public void Hide()
    {
        Panel.SetActive(false);
    }

    /// <summary>
    /// Выводит реплику. Пустое имя скрывает поле имени.
    /// </summary>
    public void SetLine(string speaker, string text)
    {
        SpeakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        SpeakerText.text = speaker;
        BodyText.text = text;
    }
}