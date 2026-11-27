
using UnityEngine;
using UnityEngine.InputSystem;

public class Radio : GrabbingItem, IActivate
{
    [Header("Звук")]
    [field: SerializeField] public AudioSource Source { get; private set; }
    [field: SerializeField] public AudioClip[] Tracks { get; private set; }

    [Header("Управление")]
    [Tooltip("Клавиша следующей песни.")]
    [field: SerializeField] public InputAction NextTrackKey { get; private set; }

    [Tooltip("Колёсико мыши для изменения громкости.")]
    [field: SerializeField] public InputAction VolumeScroll { get; private set; }

    [Header("Громкость")]
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.5f;

    [SerializeField, Range(0.01f, 0.5f)]
    private float volumeStep = 0.05f;

    public bool IsOn { get; private set; }

    private int index;
    private bool started;
    private float checkAfter;
    private bool hasFocus = true;
    private float masterVolume;
    private float currentVolume;

    private void UpdateParams()
    {
        masterVolume = UIController.LoadFloat(
            "",
            "settings",
            "MasterVolume",
            50f
        );

        currentVolume = volume * masterVolume;
        Source.volume = currentVolume;
    }

    protected override void Awake()
    {
        base.Awake();

        UpdateParams();

        if (Source == null)
        {
            Debug.LogError("Radio: не назначен Source", this);
            enabled = false;
            return;
        }

        Source.playOnAwake = false;
        Source.loop = false;
        Source.volume = volume;

        UIController.UpdateParams += UpdateParams;
    }

    private void OnEnable()
    {
        NextTrackKey.started += OnNextTrack;
        NextTrackKey.Enable();

        VolumeScroll.performed += OnVolumeScroll;
        VolumeScroll.Enable();
    }

    private void OnDisable()
    {
        NextTrackKey.started -= OnNextTrack;
        NextTrackKey.Disable();

        VolumeScroll.performed -= OnVolumeScroll;
        VolumeScroll.Disable();
    }

    private void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;

        if (focus)
            checkAfter = Time.unscaledTime + 0.5f;
    }

    private void Update()
    {
        if (IsOn && hasFocus && Time.unscaledTime > checkAfter && !Source.isPlaying)
            NextTrack();
    }

    private void OnNextTrack(InputAction.CallbackContext context)
    {
        if (!IsOn || LaunchBeam.CurrentTarget != this)
            return;

        NextTrack();
    }

    private void OnVolumeScroll(InputAction.CallbackContext context)
    {
        if (!IsOn || LaunchBeam.CurrentTarget != this)
            return;

        float scroll = context.ReadValue<Vector2>().y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        volume = Mathf.Clamp01(volume + Mathf.Sign(scroll) * volumeStep);
        currentVolume = volume * masterVolume;

        Source.volume = currentVolume;
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

    public void Activate()
    {
        if (Tracks == null || Tracks.Length == 0)
            return;

        IsOn = !IsOn;

        if (!IsOn)
        {
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

    public void StopActivate()
    {
        return;
    }
}
