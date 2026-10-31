using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Проигрывает любой DialogueData: показывает реплики в UI и листает их по клавише.
/// Блокирует управление игроком на время диалога.
///
/// Крепится на: отдельный объект сцены (например, DialogueManager).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Управление")]
    [field: SerializeField] public InputAction NextKey { get; private set; }

    [Header("UI")]
    [field: SerializeField] public DialogueUI UI { get; private set; }

    /// <summary>
    /// Идёт ли диалог в данный момент.
    /// </summary>
    public bool IsPlaying { get; private set; }

    private DialogueLine[] currentLines;
    private int lineIndex;
    private Action onFinished;

    // Кадр начала диалога: защита от того, что то же нажатие Е сразу пролистнёт первую реплику.
    private int startFrame;
     // Кадр окончания диалога: то же нажатие Е не должно сразу запустить его снова.
    private int endFrame = -1;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        NextKey.started += OnNext;
        NextKey.Enable();
    }

    private void OnDisable()
    {
        NextKey.started -= OnNext;
        NextKey.Disable();
    }

    /// <summary>
    /// Запускает диалог из ассета. Если другой диалог уже идёт, вызов игнорируется.
    /// </summary>
    public bool StartDialogue(DialogueData data, Action onFinished = null)
    {
        return data != null && StartDialogue(data.Lines, onFinished);
    }

    /// <summary>
    /// Запускает диалог из реплик, собранных в коде (например, текст случайного заказа).
    /// </summary>
    /// <returns>true, если диалог запущен.</returns>
    public bool StartDialogue(DialogueLine[] lines, Action onFinished = null)
    {
        if (IsPlaying || Time.frameCount == endFrame || lines == null || lines.Length == 0)
            return false;

        currentLines = lines;
        this.onFinished = onFinished;
        lineIndex = 0;
        startFrame = Time.frameCount;
        IsPlaying = true;

        if (PlayerInputLock.Instance != null)
            PlayerInputLock.Instance.Lock();

        UI.Show();
        ShowCurrentLine();
        return true;
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        if (!IsPlaying || Time.frameCount == startFrame)
            return;

        lineIndex++;

        if (lineIndex >= currentLines.Length)
            EndDialogue();
        else
            ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = currentLines[lineIndex];
        UI.SetLine(line.Speaker, line.Text);
    }

    private void EndDialogue()
    {
        IsPlaying = false;
        endFrame = Time.frameCount;
        UI.Hide();

        // Сначала снимаем блокировку, затем вызываем колбэк: он может сразу запустить новый диалог.
        if (PlayerInputLock.Instance != null)
            PlayerInputLock.Instance.Unlock();

        Action callback = onFinished;
        onFinished = null;
        currentLines = null;
        callback?.Invoke();
    }
}