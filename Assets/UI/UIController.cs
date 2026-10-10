using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class UIController
{
    private readonly UIDocument document;
    private readonly Dictionary<Button, Action> buttons = new();
    private readonly Dictionary<string, VisualElement> panels = new();

    public UIController(UIDocument document)
    {
        this.document = document;
    }

    public void RegisterButton(string buttonName, Action action)
    {
        var button = document.rootVisualElement.Q<Button>(buttonName);

        if (button == null)
        {
            Debug.LogWarning($"Кнопка '{buttonName}' не найдена.");
            return;
        }

        button.clicked += action;
        buttons.Add(button, action);
    }

    public void RegisterPanel(string elementName)
    {
        var e = document.rootVisualElement.Q<VisualElement>(elementName);

        if (e == null)
        {
            Debug.LogWarning($"Панель '{elementName}' не найдена.");
            return;
        }

        panels.Add(elementName, e);
    }



    public void CloseAllPanels()
    {
        foreach (var e in panels)
        {
            e.Value.style.display = DisplayStyle.None;
        }
    }

    public void SetPanel(params string[] panelNames)
    {
        CloseAllPanels();

        foreach (var panelName in panelNames)
        {
            if (!panels.TryGetValue(panelName, out var panel))
            {
                Debug.LogWarning($"Панель '{panelName}' не зарегистрирована.");
                return;
            }

            panel.style.display = DisplayStyle.Flex;
        }
    }

    public void Clear()
    {
        foreach (var button in buttons)
        {
            button.Key.clicked -= button.Value;
        }

        buttons.Clear();
        panels.Clear();
    }
}
