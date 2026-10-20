using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : InteractiveObject
{
    [field: SerializeField] public bool IsActive { get; private set; } = true;
    [field: SerializeField] public UnityEvent Event {  get; private set; }

    [Header("Параметры анимации")]
    [field: SerializeField] private float indentValue = 0.05f;
    [field: SerializeField] private float indentSpeed = 0.01f;
    [field: SerializeField] private float indentDelay = 0.01f;

    private bool isPressed = false;

    public override void ObjectFound()
    {
        StartCoroutine(Indentation());
    }

    private IEnumerator Indentation()
    {
        if (isPressed) yield break  ;

        isPressed = true;

        Event?.Invoke();

        var counter = indentValue / indentSpeed;

        for (int i = 0; i < counter; i++)
        {
            Vector3 pos = transform.localPosition;
            pos.y -= indentSpeed;
            transform.localPosition = pos;

            yield return new WaitForSeconds(indentDelay);
        }

        for (int i = 0; i < counter; i++)
        {
            Vector3 pos = transform.localPosition;
            pos.y += indentSpeed;
            transform.localPosition = pos;

            yield return new WaitForSeconds(indentDelay);
        }

        isPressed = false;
    }
}
