using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Радио: Е включает и выключает, отдельная клавиша переключает песню.
/// Песни играются по кругу, после конца трека включается следующий.
///
/// Крепится на: объект радио с обычным (не триггерным) коллайдером и компонентом AudioSource.
/// </summary>
public class Radio : MonoBehaviour, IInteractable
{
    [Header("Звук")]
    [field: SerializeField] public AudioSource Source { get; private set; }
    [field: SerializeField] public AudioClip[] Tracks { get; private set; }

    [Header("Управление")]
    [Tooltip("Клавиша следующей песни. Работает, только пока игрок смотрит на радио.")]
    [field: SerializeField] public InputAction NextTrackKey { get; private set; }
    [field: SerializeField] public PlayerInteractor Interactor { get; private set; }

    public bool IsOn { get; private set; }

    public string InteractionPrompt => IsOn ? "Выключить радио" : "Включить радио";

    private int index;

    // Был ли трек уже запущен (отличает первый запуск от продолжения после паузы).
    private bool started;

    // Пауза после запуска трека: пока Unity подгружает данные, isPlaying может быть false.
    private float checkAfter;

    // Кадр последнего срабатывания: одно нажатие Е приходит дважды в одном кадре.
    private int lastInteractFrame = -1;

    // Есть ли у игры фокус. Без фокуса Unity паузит звук, и это не конец трека.
    private bool hasFocus = true;

    private void Awake()
    {
        if (Source == null)
        {
            Debug.LogError("Radio: не назначен Source", this);
            enabled = false;
            return;
        }

        Source.playOnAwake = false;
        Source.loop = false;
    }

    private void OnEnable()
    {
        NextTrackKey.started += OnNextTrack;
        NextTrackKey.Enable();
    }

    private void OnDisable()
    {
        NextTrackKey.started -= OnNextTrack;
        NextTrackKey.Disable();
    }

    private void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;

        // После возврата фокуса даём звуку время возобновиться, прежде чем проверять конец трека.
        if (focus)
            checkAfter = Time.unscaledTime + 0.5f;
    }

    private void Update()
    {
        // Трек закончился сам: включаем следующий (но не сразу после запуска и не при потере фокуса).
        if (IsOn && hasFocus && Time.unscaledTime > checkAfter && !Source.isPlaying)
            NextTrack();
    }

    public void Interact()
    {
        // Одно нажатие Е приходит дважды в одном кадре: второй вызов пропускаем.
        if (Time.frameCount == lastInteractFrame)
            return;

        lastInteractFrame = Time.frameCount;

        if (Tracks == null || Tracks.Length == 0)
            return;

        IsOn = !IsOn;

        if (!IsOn)
        {
            // Пауза, а не стоп: песня продолжится с того же места.
            Source.Pause();
        }
        else if (!started)
        {
            PlayCurrent();
        }
        else
        {
            Source.UnPause();
            checkAfter = Time.unscaledTime + 0.5f;
        }
    }

    private void OnNextTrack(InputAction.CallbackContext context)
    {
        // Реагируем, только если игрок смотрит именно на это радио.
        if (!IsOn || Interactor == null || Interactor.CurrentTarget != (IInteractable)this)
            return;

        NextTrack();
    }

    private void NextTrack()
    {
        index = (index + 1) % Tracks.Length;
        PlayCurrent();
    }

    private void PlayCurrent()
    {
        Source.clip = Tracks[index];
        Source.Play();
        started = true;
        checkAfter = Time.unscaledTime + 0.5f;
    }
}