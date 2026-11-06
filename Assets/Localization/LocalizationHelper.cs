
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class LocalizationHelper
{
    // ==========================================
    // CACHE
    // ==========================================

    private static readonly Dictionary<string, JObject> cache =
        new Dictionary<string, JObject>();

    public static string Language = "ru";

    public static void ChangeLanguage(string language)
    {
        Language = language.ToLower();
    }

    private static JObject GetJson(string path, string itemName)
    {
        itemName = itemName.ToLower();

        // Важно: учитываем путь, чтобы Items и Phrases
        // с одинаковыми id не конфликтовали в кэше.
        string cacheKey = path + itemName;

        if (cache.TryGetValue(cacheKey, out JObject doc))
            return doc;

        TextAsset file = Resources.Load<TextAsset>(
            path + itemName
        );

        if (file == null)
        {
            throw new FileNotFoundException(
                "Не найден JSON: " + path + itemName
            );
        }

        doc = JObject.Parse(file.text);
        cache[cacheKey] = doc;

        return doc;
    }

    // ==========================================
    // ITEMS
    // ==========================================

    public static string GetItem(
        string itemName,
        bool isSingular = true,
        string grammaticalCase = "nominative")
    {

        JObject doc = GetJson("Items/", itemName);

        if (Language == "ru")
        {
            return (string)doc["ru"]?["forms"]?
                [isSingular ? "singular" : "plural"]?
                [grammaticalCase.ToLower()];
        }

        if (Language == "en")
        {
            return (string)doc["en"]?
                [isSingular ? "singular" : "plural"];
        }

        throw new ArgumentException(
            "Неподдерживаемый язык: " + Language
        );
    }

    public static string GetGender(
        string itemName)
    {
        JObject doc = GetJson("Items/", itemName);

        return (string)doc[Language]?["gender"];
    }

    // ==========================================
    // PHRASES
    // ==========================================

    public static string GetPhrase(
        string phraseName,
        params string[] adds)
    {

        JObject doc = GetJson("Phrases/", phraseName);

        JArray variants = doc[Language]?["variants"] as JArray;

        if (variants == null || variants.Count == 0)
        {
            throw new Exception(
                "Не найдены варианты фразы: " + phraseName
            );
        }

        // Создаём словарь переданных предметов.
        // Например: CurrentRecipe:tomato
        Dictionary<string, string> items =
            new Dictionary<string, string>();

        foreach (string add in adds)
        {
            string[] parts = add.Split(':');

            if (parts.Length != 2)
                throw new ArgumentException(
                    "Неверный формат параметра: " + add
                );

            items[parts[0].ToLower()] = parts[1].ToLower();
        }

        // Собираем все подходящие варианты.
        List<JObject> suitableVariants = new List<JObject>();

        foreach (JObject variant in variants)
        {
            if (CheckConditions(
                variant["conditions"] as JObject,
                items))
            {
                suitableVariants.Add(variant);
            }
        }

        if (suitableVariants.Count == 0)
        {
            throw new Exception(
                "Нет подходящих вариантов фразы: " + phraseName
            );
        }

        // Случайно выбираем подходящий вариант.
        JObject selectedVariant = suitableVariants[
            UnityEngine.Random.Range(
                0,
                suitableVariants.Count
            )
        ];

        JArray texts = selectedVariant["texts"] as JArray;

        if (texts == null || texts.Count == 0)
        {
            throw new Exception(
                "У варианта отсутствуют тексты: " + phraseName
            );
        }

        // Случайно выбираем реплику
        string text = texts[
            UnityEngine.Random.Range(0, texts.Count)
        ].ToString();

        // Обрабатываем все команды в квадратных скобках.
        return Regex.Replace(
            text,
            @"\[([a-zA-Z0-9_]+):([a-zA-Z0-9_]+)\]",
            match =>
            {
                string alias = match.Groups[1].Value.ToLower();
                string command = match.Groups[2].Value.ToLower();

                if (!items.TryGetValue(alias, out string itemName))
                {
                    throw new Exception(
                        "Не передан предмет для команды: " + alias
                    );
                }

                return ResolveCommand(
                    itemName,
                    command
                );
            }
        );
    }

    // ==========================================
    // CHECK CONDITIONS
    // ==========================================

    private static bool CheckConditions(
        JObject conditions,
        Dictionary<string, string> items)
    {
        // Если условий нет, вариант подходит всегда.
        if (conditions == null)
            return true;

        foreach (JProperty condition in conditions.Properties())
        {
            // Например: CurrentRecipe.gender
            string[] parts = condition.Name.Split('.');

            if (parts.Length != 2)
                throw new Exception(
                    "Неверное условие: " + condition.Name
                );

            string alias = parts[0].ToLower();
            string property = parts[1].ToLower();

            if (!items.TryGetValue(alias, out string itemName))
                return false;

            string actualValue = GetItemProperty(
                itemName,
                property
            );

            string expectedValue = condition.Value.ToString()
                .ToLower();

            if (actualValue == null ||
                actualValue.ToLower() != expectedValue)
            {
                return false;
            }
        }

        return true;
    }

    // ==========================================
    // ITEM PROPERTIES
    // ==========================================

    private static string GetItemProperty(
        string itemName,
        string property)
    {
        JObject doc = GetJson("Items/", itemName);

        // Сейчас поддерживаем gender.
        // В дальнейшем сюда можно добавить другие свойства.
        if (property == "gender")
            return (string)doc[Language]?["gender"];

        throw new Exception(
            "Неизвестное свойство предмета: " + property
        );
    }

    // ==========================================
    // COMMAND RESOLVER
    // ==========================================

    private static string ResolveCommand(
        string itemName,
        string command)
    {
        switch (command)
        {
            // Именительный падеж
            case "singular":
                return GetItem(
                    itemName, true, "nominative"
                );

            // Множественное число
            case "plural":
                return GetItem(
                    itemName, false, "nominative"
                );

            // Грамматические падежи
            case "nominative":
            case "genitive":
            case "dative":
            case "accusative":
            case "instrumental":
            case "prepositional":

                return GetItem(
                    itemName,
                    true,
                    command
                );

            default:
                throw new Exception(
                    "Неизвестная команда: " + command
                );
        }
    }

    // ==========================================
    // STRING EXTRACTER
    // ==========================================

    public static string GetStringFromJSON(string path, string fileName, string stringID)
    {
        var json = GetJson(path, fileName);

        if (json == null)
        {
            Debug.LogError("Такого JSON нет");
            return null;
        }

        return json[stringID][Language].ToString();
    }


    // ==========================================
    // CACHE MANAGEMENT
    // ==========================================

    public static void ClearCache()
    {
        cache.Clear();
    }
}
