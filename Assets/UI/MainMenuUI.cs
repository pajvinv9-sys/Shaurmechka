using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    UIController ui;

    private void OnEnable()
    {
        RegisterMainPanel();
        RegisterOptionsPanel();
        
        ui.SetPanel("MainPanel");
    }

    private void RegisterMainPanel()
    {
        ui.RegisterPanel("MainPanel");

        ui.RegisterButton("StartButtonMain", () => SceneManager.LoadScene("SampleScene"));
        ui.RegisterButton("OptionsButtonMain", () => ui.SetPanel("OptionsPanel"));
        ui.RegisterButton("ExitButtonMain",
            #if UNITY_EDITOR
                () => UnityEditor.EditorApplication.isPlaying = false
            #else
                () => Application.Quit()
            #endif
        );
        
    }
    private void RegisterOptionsPanel()
    {
        ui.RegisterPanel("OptionsPanel");

        ui.RegisterButton("ToMain", () => ui.SetPanel("MainPanel"));
    }

    private void OnDisable()
    {
        ui.Clear();
    }

    private void Awake()
    {
        ui = new(GetComponent<UIDocument>());
    }
}


