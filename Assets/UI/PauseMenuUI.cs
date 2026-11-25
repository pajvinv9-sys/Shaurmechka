using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    private static readonly Color ActiveLangColor = new Color(225f / 255f, 225f / 255f, 225f / 255f, 1f);
    private static readonly Color InactiveLangColor = new Color(140f / 255f, 144f / 255f, 96f / 255f, 1f);

    [Header("Навигация по сценам")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Слайдеры чувствительности")]
    [SerializeField] private float minSensitivity = 1f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private float defaultSensitivity = 5f;

    [Header("Слайдер громкости")]
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 100f;
    [SerializeField] private float defaultVolume = 80f;

    [Header("Язык диалогов по умолчанию")]
    [SerializeField] private string defaultDialogueLanguage = "RU";

    public static bool IsPaused { get; private set; }
    public string SelectedDialogueLanguage { get; private set; } = "RU";

    public event Action OnPauseOpened;
    public event Action OnPauseClosed;
    public event Action<string> OnDialogueLanguageChanged;

    private UIDocument document;
    private UIController ui;
    private Button langRuButton;
    private Button langEnButton;
    private bool isInitialized;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("PauseMenuUI: UIDocument не найден!");
            enabled = false;
            return;
        }

        ui = new UIController(document);
    }

    private void OnEnable()
    {
        if (ui == null)
            return;

        SelectedDialogueLanguage = string.Equals(defaultDialogueLanguage, "EN", StringComparison.OrdinalIgnoreCase)
            ? "EN"
            : "RU";

        ui.Clear();
        RegisterPanels();
        RegisterPausePanel();
        RegisterOptionsPanel();
        RegisterControlsPanel();

        ui.HideAllPanels();
        SetPausedState(false, force: true);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPausedState(!IsPaused);
    }

    public void ResumeGame()
    {
        SetPausedState(false);
    }

    public void SetPausedState(bool pause, bool force = false)
    {
        if (!force && IsPaused == pause)
            return;

        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;

        if (pause)
        {
            ui.SetPanel("PausePanel");
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            OnPauseOpened?.Invoke();
        }
        else
        {
            ui.HideAllPanels();
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            OnPauseClosed?.Invoke();
        }
    }

    public void SetDialogueLanguage(string languageCode, bool invokeEvent = true)
    {
        string normalized = string.Equals(languageCode, "EN", StringComparison.OrdinalIgnoreCase)
            ? "EN"
            : "RU";

        SelectedDialogueLanguage = normalized;
        UpdateLanguageSwitchVisuals();

        if (invokeEvent)
        {
            OnDialogueLanguageChanged?.Invoke(SelectedDialogueLanguage);
        }
    }

    private void UpdateLanguageSwitchVisuals()
    {
        bool isRu = SelectedDialogueLanguage == "RU";

        if (langRuButton != null)
        {
            langRuButton.style.backgroundColor = isRu ? ActiveLangColor : InactiveLangColor;
            langRuButton.style.borderLeftWidth = isRu ? 4f : 2f;
            langRuButton.style.borderRightWidth = isRu ? 4f : 2f;
            langRuButton.style.borderTopWidth = isRu ? 4f : 2f;
            langRuButton.style.borderBottomWidth = isRu ? 4f : 2f;
        }

        if (langEnButton != null)
        {
            langEnButton.style.backgroundColor = !isRu ? ActiveLangColor : InactiveLangColor;
            langEnButton.style.borderLeftWidth = !isRu ? 4f : 2f;
            langEnButton.style.borderRightWidth = !isRu ? 4f : 2f;
            langEnButton.style.borderTopWidth = !isRu ? 4f : 2f;
            langEnButton.style.borderBottomWidth = !isRu ? 4f : 2f;
        }
    }

    // ================================================
    // PANELS
    // ================================================

    private void RegisterPanels()
    {
        ui.RegisterPanel("PausePanel");
        ui.RegisterPanel("OptionsPanel");
        ui.RegisterPanel("ControlsPanel");
    }

    // ================================================
    // PAUSE PANEL
    // ================================================

    private void RegisterPausePanel()
    {
        ui.RegisterButton("PausePanel", "ResumeButton", ResumeGame);
        ui.RegisterButton("PausePanel", "OptionsButton", () => ui.SetPanel("OptionsPanel"));
        ui.RegisterButton("PausePanel", "MainMenuButton", ExitToMainMenu);
    }

    // ================================================
    // OPTIONS PANEL
    // ================================================

    private void RegisterOptionsPanel()
    {
        ui.RegisterButton("OptionsPanel", "BackToPauseButton", () => ui.SetPanel("PausePanel"));
        ui.RegisterButton("OptionsPanel", "ControlsButton", () => ui.SetPanel("ControlsPanel"));

        ui.RegisterSlider(
            "OptionsPanel",
            "SensX",
            "",
            "settings",
            minSensitivity,
            maxSensitivity,
            defaultSensitivity
        );

        ui.RegisterSlider(
            "OptionsPanel",
            "SensY",
            "",
            "settings",
            minSensitivity,
            maxSensitivity,
            defaultSensitivity
        );

        ui.RegisterSlider(
            "OptionsPanel",
            "MasterVolume",
            "",
            "settings",
            minVolume,
            maxVolume,
            defaultVolume
        );

        VisualElement root = document.rootVisualElement;
        langRuButton = root?.Q<Button>("LangRuButton");
        langEnButton = root?.Q<Button>("LangEnButton");

        ui.RegisterButton("OptionsPanel", "LangRuButton", () => SetDialogueLanguage("RU"));
        ui.RegisterButton("OptionsPanel", "LangEnButton", () => SetDialogueLanguage("EN"));

        UpdateLanguageSwitchVisuals();
    }

    // ================================================
    // CONTROLS PANEL
    // ================================================

    private void RegisterControlsPanel()
    {
        ui.RegisterButton("ControlsPanel", "BackToOptionsButton", () => ui.SetPanel("OptionsPanel"));
    }

    // ================================================
    // EXIT TO MENU
    // ================================================

    private void ExitToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        ui.HideAllPanels();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ================================================
    // CLEANUP
    // ================================================

    private void OnDisable()
    {
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }

        langRuButton = null;
        langEnButton = null;
        ui?.Clear();
        isInitialized = false;
    }

    private void OnDestroy()
    {
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }

        langRuButton = null;
        langEnButton = null;
        ui?.Clear();
    }
}
