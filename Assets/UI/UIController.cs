using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController
{
    private readonly UIDocument document;
    private readonly VisualElement root;

    private readonly Dictionary<string, VisualElement> panels = new();
    private readonly List<Action> cleanupActions = new();

    public static Action UpdateParams;

    // ================================================
    // CONSTRUCTOR
    // ================================================

    public UIController(UIDocument document)
    {
        this.document = document;

        if (document == null)
        {
            Debug.LogError("UIController: UIDocument не найден!");
            return;
        }

        root = document.rootVisualElement;
    }

    // ================================================
    // PANELS
    // ================================================

    public void RegisterPanel(string panelName)
    {
        if (root == null)
            return;

        VisualElement panel = root.Q<VisualElement>(panelName);

        if (panel == null)
        {
            Debug.LogWarning(
                $"Панель '{panelName}' не найдена!"
            );

            return;
        }

        panels[panelName] = panel;

        panel.style.display = DisplayStyle.None;
    }

    public void SetPanel(string panelName)
    {
        if (!panels.TryGetValue(panelName, out VisualElement target))
        {
            Debug.LogWarning(
                $"Панель '{panelName}' не зарегистрирована!"
            );

            return;
        }

        foreach (var panel in panels.Values)
        {
            panel.style.display = DisplayStyle.None;
        }

        target.style.display = DisplayStyle.Flex;
    }

    public void HideAllPanels()
    {
        foreach (var panel in panels.Values)
        {
            panel.style.display = DisplayStyle.None;
        }
    }

    // ================================================
    // ELEMENT SEARCH
    // ================================================

    private T GetElement<T>(
        string panelName,
        string elementName
    ) where T : VisualElement
    {
        if (!panels.TryGetValue(panelName, out VisualElement panel))
        {
            Debug.LogWarning(
                $"Панель '{panelName}' не зарегистрирована!"
            );

            return null;
        }

        T element = panel.Q<T>(elementName);

        if (element == null)
        {
            Debug.LogWarning(
                $"Элемент '{elementName}' типа {typeof(T).Name} " +
                $"не найден в панели '{panelName}'!"
            );
        }

        return element;
    }
    public static float LoadFloat(
    string path,
    string jsonName,
    string key,
    float defaultValue)
    {
        return GetFloat(path, jsonName, key, defaultValue);
    }
    // ================================================
    // BUTTON
    // ================================================

    public void RegisterButton(
        string panelName,
        string buttonName,
        Action callback
    )
    {
        Button button = GetElement<Button>(
            panelName,
            buttonName
        );

        if (button == null || callback == null)
            return;

        button.clicked += callback;

        cleanupActions.Add(() =>
        {
            button.clicked -= callback;
        });
    }

    // ================================================
    // SLIDER
    // ================================================

    public void RegisterSlider(
        string panelName,
        string sliderName,
        string jsonPath,
        string jsonName,
        float minValue = 0f,
        float maxValue = 100f,
        float defaultValue = 50f
    )
    {
        Slider slider = GetElement<Slider>(
            panelName,
            sliderName
        );

        if (slider == null)
            return;

        if (minValue > maxValue)
        {
            Debug.LogError(
                $"Неправильный диапазон слайдера '{sliderName}'!"
            );

            return;
        }

        slider.lowValue = minValue;
        slider.highValue = maxValue;

        JObject json = GetJson(jsonPath, jsonName);

        float savedValue = GetFloat(
            jsonPath,
            jsonName,
            sliderName,
            defaultValue
        );

        savedValue = Mathf.Clamp(
            savedValue,
            minValue,
            maxValue
        );

        slider.SetValueWithoutNotify(savedValue);

        EventCallback<ChangeEvent<float>> callback = evt =>
        {
            SetValue(
                jsonPath,
                jsonName,
                sliderName,
                evt.newValue
            );
        };

        slider.RegisterValueChangedCallback(callback);

        cleanupActions.Add(() =>
        {
            slider.UnregisterValueChangedCallback(callback);
        });
    }

    // ================================================
    // TOGGLE
    // ================================================

    public void RegisterToggle(
        string panelName,
        string toggleName,
        string jsonPath,
        string jsonName,
        bool defaultValue = false
    )
    {
        Toggle toggle = GetElement<Toggle>(
            panelName,
            toggleName
        );

        if (toggle == null)
            return;

        JObject json = GetJson(jsonPath, jsonName);

        bool value = defaultValue;

        if (json[toggleName] != null)
        {
            try
            {
                value = json[toggleName].Value<bool>();
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"Ошибка чтения Toggle '{toggleName}': {e.Message}"
                );
            }
        }

        toggle.SetValueWithoutNotify(value);

        EventCallback<ChangeEvent<bool>> callback = evt =>
        {
            SetValue(
                jsonPath,
                jsonName,
                toggleName,
                evt.newValue
            );
        };

        toggle.RegisterValueChangedCallback(callback);

        cleanupActions.Add(() =>
        {
            toggle.UnregisterValueChangedCallback(callback);
        });
    }

    // ================================================
    // TEXT FIELD
    // ================================================

    public void RegisterTextField(
        string panelName,
        string fieldName,
        string jsonPath,
        string jsonName,
        string defaultValue = ""
    )
    {
        TextField field = GetElement<TextField>(
            panelName,
            fieldName
        );

        if (field == null)
            return;

        JObject json = GetJson(jsonPath, jsonName);

        string value = json[fieldName]?.ToString() ?? defaultValue;

        field.SetValueWithoutNotify(value);

        EventCallback<ChangeEvent<string>> callback = evt =>
        {
            SetValue(
                jsonPath,
                jsonName,
                fieldName,
                evt.newValue
            );
        };

        field.RegisterValueChangedCallback(callback);

        cleanupActions.Add(() =>
        {
            field.UnregisterValueChangedCallback(callback);
        });
    }

    // ================================================
    // DROPDOWN
    // ================================================

    public void RegisterDropdown(
        string panelName,
        string dropdownName,
        string jsonPath,
        string jsonName,
        List<string> choices,
        string defaultValue = ""
    )
    {
        DropdownField dropdown = GetElement<DropdownField>(
            panelName,
            dropdownName
        );

        if (dropdown == null)
            return;

        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning(
                $"Dropdown '{dropdownName}' не имеет вариантов!"
            );

            return;
        }

        dropdown.choices = new List<string>(choices);

        JObject json = GetJson(jsonPath, jsonName);

        string value = json[dropdownName]?.ToString() ?? defaultValue;

        if (!choices.Contains(value))
        {
            value = choices.Contains(defaultValue)
                ? defaultValue
                : choices[0];
        }

        dropdown.SetValueWithoutNotify(value);

        EventCallback<ChangeEvent<string>> callback = evt =>
        {
            SetValue(
                jsonPath,
                jsonName,
                dropdownName,
                evt.newValue
            );
        };

        dropdown.RegisterValueChangedCallback(callback);

        cleanupActions.Add(() =>
        {
            dropdown.UnregisterValueChangedCallback(callback);
        });
    }

    // ================================================
    // CLEANUP
    // ================================================

    public void Clear()
    {
        foreach (Action action in cleanupActions)
        {
            action?.Invoke();
        }

        cleanupActions.Clear();
        panels.Clear();
    }

    // ================================================
    // JSON CACHE
    // ================================================

    private static readonly Dictionary<string, JObject> cache = new();

    private static string GetKey(
        string path,
        string jsonName
    )
    {
        string resourcePath = string.IsNullOrEmpty(path)
            ? jsonName
            : path.TrimEnd('/') + "/" + jsonName;

        return resourcePath.ToLowerInvariant();
    }

    private static string GetResourcePath(
        string path,
        string jsonName
    )
    {
        return string.IsNullOrEmpty(path)
            ? jsonName
            : path.TrimEnd('/') + "/" + jsonName;
    }

    private static string GetSavePath(string key)
    {
        return Path.Combine(
            Application.persistentDataPath,
            key + ".json"
        );
    }

    // ================================================
    // LOAD JSON
    // ================================================

    public static JObject GetJson(
        string path,
        string jsonName
    )
    {
        string key = GetKey(path, jsonName);

        if (cache.TryGetValue(key, out JObject cached))
            return cached;

        JObject json = new JObject();

        // --------------------------------------------
        // LOAD DEFAULT JSON FROM RESOURCES
        // --------------------------------------------

        string resourcePath = GetResourcePath(
            path,
            jsonName
        );

        TextAsset file = Resources.Load<TextAsset>(resourcePath);

        if (file != null)
        {
            try
            {
                json = JObject.Parse(file.text);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Ошибка исходного JSON '{resourcePath}': {e.Message}"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                $"Исходный JSON не найден в Resources: {resourcePath}"
            );
        }

        // --------------------------------------------
        // LOAD SAVED JSON
        // --------------------------------------------

        string savePath = GetSavePath(key);

        if (File.Exists(savePath))
        {
            try
            {
                string savedText = File.ReadAllText(savePath);

                JObject saved = JObject.Parse(savedText);

                json.Merge(saved, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace
                });
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Ошибка загрузки сохранений '{savePath}': {e.Message}"
                );
            }
        }

        cache[key] = json;
        Debug.Log("Save path: " + GetSavePath(jsonName));
        Debug.Log("Loaded settings: " + json.ToString());
        return json;
    }

    // ================================================
    // SAVE VALUE
    // ================================================

    private void SetValue(string path, string jsonName, string key, object value)
    {
        try
        {
            JObject json = GetJson(path, jsonName);

            // Преобразуем значение в корректный JSON-токен
            JToken token;

            if (value is float f)
            {
                token = new JValue((double)f);
            }
            else if (value is double d)
            {
                token = new JValue(d);
            }
            else if (value is decimal dec)
            {
                token = new JValue(dec);
            }
            else
            {
                token = value == null
                    ? JValue.CreateNull()
                    : JToken.FromObject(value);
            }

            json[key] = token;

            string savePath = GetSavePath(jsonName);

            string directory = Path.GetDirectoryName(savePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonText = json.ToString(Formatting.Indented);

            File.WriteAllText(savePath, jsonText);

            Debug.Log($"Сохранено: {key} = {json[key]}");
            Debug.Log($"Путь: {savePath}");

            UpdateParams?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения '{key}': {e}");
        }
    }

    // ================================================
    // GET FLOAT
    // ================================================

    private static float GetFloat(
    string path,
    string jsonName,
    string key,
    float defaultValue)
    {
        try
        {
            JObject json = GetJson(path, jsonName);

            JToken token = json[key];

            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            float result;

            // Читаем числовое значение JSON
            if (token.Type == JTokenType.Float ||
                token.Type == JTokenType.Integer)
            {
                result = token.Value<float>();
            }
            else
            {
                // На случай, если старое значение было сохранено строкой
                string text = token.ToString();

                if (!float.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out result))
                {
                    // Поддержка старых значений с запятой
                    if (!float.TryParse(
                            text,
                            NumberStyles.Float,
                            CultureInfo.CurrentCulture,
                            out result))
                    {
                        Debug.LogWarning(
                            $"Не удалось прочитать float '{key}'. Используется значение по умолчанию.");

                        return defaultValue;
                    }
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка чтения '{key}': {e}");
            return defaultValue;
        }
    }

    // ================================================
    // RESET CACHE
    // ================================================

    public static void ResetCache()
    {
        cache.Clear();
    }
}
