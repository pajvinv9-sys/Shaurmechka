
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private UIController ui;

    private void Awake()
    {
        UIDocument document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("MainMenuUI: UIDocument не найден!");
            enabled = false;
            return;
        }

        ui = new UIController(document);
    }

    private void OnEnable()
    {
        if (ui == null)
            return;

        RegisterPanels();

        RegisterMainPanel();
        RegisterOptionsPanel();
        RegisterAudioPanel();
        RegisterControlsPanel();

        ui.SetPanel("MainPanel");
    }

    // ================================================
    // PANELS
    // ================================================

    private void RegisterPanels()
    {
        ui.RegisterPanel("MainPanel");
        ui.RegisterPanel("OptionsPanel");
        ui.RegisterPanel("AudioPanel");
        ui.RegisterPanel("ControlsPanel");
    }

    // ================================================
    // MAIN PANEL
    // ================================================

    private void RegisterMainPanel()
    {
        ui.RegisterButton(
            "MainPanel",
            "StartButtonMain",
            () => SceneManager.LoadScene("Scene")
        );

        ui.RegisterButton(
            "MainPanel",
            "OptionsButtonMain",
            () => ui.SetPanel("OptionsPanel")
        );

        ui.RegisterButton(
            "MainPanel",
            "ExitButtonMain",
            ExitGame
        );
    }

    // ================================================
    // OPTIONS PANEL
    // ================================================

    private void RegisterOptionsPanel()
    {
        ui.RegisterButton(
            "OptionsPanel",
            "ToMain",
            () => ui.SetPanel("MainPanel")
        );

        ui.RegisterButton(
            "OptionsPanel",
            "AudioButton",
            () => ui.SetPanel("AudioPanel")
        );

        ui.RegisterButton(
            "OptionsPanel",
            "ControlsButton",
            () => ui.SetPanel("ControlsPanel")
        );

        ui.RegisterSlider(
            "OptionsPanel",
            "SensX",
            "",
            "settings",
            1f,
            10f,
            5f
        );

        ui.RegisterSlider(
            "OptionsPanel",
            "SensY",
            "",
            "settings",
            1f,
            10f,
            5f
        );

        ui.RegisterToggle(
            "OptionsPanel",
            "Fullscreen",
            "",
            "settings",
            true
        );
    }

    // ================================================
    // AUDIO PANEL
    // ================================================

    private void RegisterAudioPanel()
    {
        ui.RegisterButton(
            "AudioPanel",
            "BackButton",
            () => ui.SetPanel("OptionsPanel")
        );

        ui.RegisterSlider(
            "AudioPanel",
            "MasterVolume",
            "",
            "settings",
            0f,
            100f,
            80f
        );
    }

    // ================================================
    // CONTROLS PANEL
    // ================================================

    private void RegisterControlsPanel()
    {
        ui.RegisterButton(
            "ControlsPanel",
            "BackButton",
            () => ui.SetPanel("OptionsPanel")
        );

        ui.RegisterTextField(
            "ControlsPanel",
            "PlayerName",
            "",
            "settings",
            "Player"
        );

        ui.RegisterDropdown(
            "ControlsPanel",
            "Quality",
            "",
            "settings",
            new List<string>
            {
                "Low",
                "Medium",
                "High",
                "Ultra"
            },
            "High"
        );
    }

    // ================================================
    // EXIT
    // ================================================

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ================================================
    // CLEANUP
    // ================================================

    private void OnDisable()
    {
        ui?.Clear();
    }

    private void OnDestroy()
    {
        ui?.Clear();
    }
}
