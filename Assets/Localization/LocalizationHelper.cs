using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class LocalizationHelper
{
    // Кэш загруженных JSON-файлов
    private static Dictionary<string, JObject> cache =
        new Dictionary<string, JObject>();

    // Загружает JSON или берёт его из кэша
    private static JObject GetJson(string itemName)
    {
        itemName = itemName.ToLower();

        // Проверяем, есть ли предмет в кэше
        if (cache.TryGetValue(itemName, out JObject doc))
        {
            return doc;
        }

        // Если нет — читаем файл
        TextAsset file = Resources.Load<TextAsset>("Items/" + itemName);

        if (file == null)
            throw new FileNotFoundException(
                "Не найден JSON предмета: " + itemName
            );

        string json = file.text;

        doc = JObject.Parse(json);

        // Сохраняем в кэш
        cache[itemName] = doc;

        return doc;
    }

    // Основной метод получения названия предмета
    public static string GetItem(
        string itemName,
        string language,
        bool isSingular = true,
        string grammaticalCase = "")
    {
        language = language.ToLower();

        if (language == "ru")
            return GetItemRu(itemName, isSingular, grammaticalCase);

        if (language == "en")
            return GetItemEn(itemName, isSingular);

        return null;
    }

    // Получение русского названия
    private static string GetItemRu(
        string itemName,
        bool isSingular,
        string grammaticalCase)
    {
        if (string.IsNullOrEmpty(grammaticalCase))
            grammaticalCase = "nominative";

        grammaticalCase = grammaticalCase.ToLower();

        JObject doc = GetJson(itemName);

        return (string)doc["ru"]["forms"]
            [isSingular ? "singular" : "plural"]
            [grammaticalCase];
    }

    // Получение английского названия
    private static string GetItemEn(
        string itemName,
        bool isSingular)
    {
        JObject doc = GetJson(itemName);

        return (string)doc["en"]
            [isSingular ? "singular" : "plural"];
    }

    // Получение грамматического рода
    public static string GetGender(
        string itemName,
        string language)
    {
        JObject doc = GetJson(itemName);

        return (string)doc[language.ToLower()]["gender"];
    }

    // Очистка кэша
    public static void ClearCache()
    {
        cache.Clear();
    }
}
