using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ультра-легкий триггер: при попадании InteractiveObject показывает картинку посередине экрана.
/// 
/// Вешается на: GameObject с Collider (Is Trigger = true).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ImageTriggerZone : MonoBehaviour
{
    [Header("UI Картинка")]
    /// <summary>
    /// Компонент Image на Canvas (NotificationIcon).
    /// </summary>
    [field: SerializeField]
    public Image NotificationIcon { get; private set; }

    /// <summary>
    /// Время показа картинки на экране (сек).
    /// </summary>
    [field: SerializeField]
    public float DisplayDuration { get; private set; } = 1.5f;
    
    /// <summary>
    /// Звук с картинкой
    /// </summary>    
    [field: SerializeField]
    public AudioClip Sound { get; private set; }

    [Header("Размер и положение")]
    /// <summary>
    /// Размер картинки (ширина, высота).
    /// </summary>
    [field: SerializeField]
    public Vector2 IconSize { get; private set; } = new Vector2(500, 500f);
    

    private Coroutine hideCoroutine;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (NotificationIcon != null)
        {
            NotificationIcon.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractiveObject interactiveObj = other.GetComponent<InteractiveObject>();
        if (interactiveObj == null)
            return;

        ShowImage();
        
        if (Sound != null)
        {
            AudioSource.PlayClipAtPoint(Sound, Camera.main.transform.position);
        }

    }

    private void ShowImage()
    {
        if (NotificationIcon == null)
            return;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }


        RectTransform rect = NotificationIcon.rectTransform;
        rect.sizeDelta = IconSize;

        NotificationIcon.gameObject.SetActive(true);

        hideCoroutine = StartCoroutine(AutoHideRoutine());
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(DisplayDuration);

        if (NotificationIcon != null)
        {
            NotificationIcon.gameObject.SetActive(false);
        }
        hideCoroutine = null;
    }
}