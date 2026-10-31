using UnityEngine;

/// <summary>
/// Одна реплика диалога.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    /// <summary>
    /// Имя говорящего. Может быть пустым (например, у плаката).
    /// </summary>
    public string Speaker;

    [TextArea(2, 5)]
    public string Text;
}

/// <summary>
/// Ассет диалога: набор реплик, которые игрок листает клавишей Е.
/// Создаётся через Assets → Create → Dialogue → Dialogue Data.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [field: SerializeField] public DialogueLine[] Lines { get; private set; }
}