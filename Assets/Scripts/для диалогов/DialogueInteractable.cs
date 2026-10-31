using UnityEngine;

/// <summary>
/// Объект, при взаимодействии с которым запускается диалог (плакат, табличка и т. д.).
///
/// Крепится на: объект с обычным (не триггерным) коллайдером.
/// </summary>
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public DialogueData Dialogue { get; private set; }
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Осмотреть";

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(Dialogue);
    }
}