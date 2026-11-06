using TMPro;
using UnityEngine;

public class ChangeLanguageButton : MonoBehaviour
{
    public TextMeshPro TextMeshPro;
    public void ChangeLanguage()
    {
        if (LocalizationHelper.Language == "ru")
        {
            LocalizationHelper.ChangeLanguage("en");
        }
        else LocalizationHelper.ChangeLanguage("ru");

        TextMeshPro.text = LocalizationHelper.Language;
    }
}
