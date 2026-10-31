using UnityEngine;

/// <summary>
/// Временный скрипт для проверки PlayerInteractor.
/// Крепится на: любой объект с обычным (не триггерным) коллайдером.
/// После проверки удаляется.
/// </summary>
public class TestInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Проверка";

    public void Interact()
    {
        Debug.Log("Е нажата на " + gameObject.name, this);
    }
}