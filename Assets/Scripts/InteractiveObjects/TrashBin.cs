using System.Collections;
using UnityEngine;

/// <summary>
/// Мусорное ведро-животное: удаляет предметы со сцены.
/// Состояния: Idle (игрока рядом нет), Open (игрок в зоне, рот открыт), Eaten (предмет только что съеден).
///
/// Крепится на: корневой объект ведра с коллайдером Is Trigger (зона, в которой игрок "будит" животное).
/// Предметы съедаются не по всей зоне, а только у рта: см. поле Mouth.
/// </summary>
public class TrashBin : InteractiveObject
{
    [Header("Спрайты")]
    [Tooltip("Контейнер со спрайтами: он поворачивается к камере.")]
    [field: SerializeField] public Transform Sprites { get; private set; }
    [field: SerializeField] public GameObject Idle { get; private set; }
    [field: SerializeField] public GameObject Open { get; private set; }
    [field: SerializeField] public GameObject Eaten { get; private set; }

    [Tooltip("Поворачивать к камере только по горизонтали (спрайт не наклоняется).")]
    [field: SerializeField] public bool YawOnly { get; private set; } = true;

    [Header("Поедание")]
    [Tooltip("Пустой объект в том месте, куда должен попасть предмет.")]
    [field: SerializeField] public Transform Mouth { get; private set; }
    [field: SerializeField] public float EatRadius { get; private set; } = 0.5f;
    [Tooltip("Сколько секунд показывается состояние 'съедено'.")]
    [field: SerializeField] public float EatDuration { get; private set; } = 0.6f;

    private readonly Collider[] buffer = new Collider[16];

    // Счётчик, потому что у игрока может быть несколько коллайдеров.
    private int playerColliders;
    private Coroutine eatRoutine;
    private Camera cam;

    protected override void Awake()
    {
        base.Awake();
        Refresh();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Movement>() == null)
            return;

        playerColliders++;
        Refresh();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Movement>() == null)
            return;

        playerColliders = Mathf.Max(0, playerColliders - 1);
        Refresh();
    }

    private void FixedUpdate()
    {
        int count = Physics.OverlapSphereNonAlloc(
            Mouth.position,
            EatRadius,
            buffer,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            Ingredient item = buffer[i].GetComponentInParent<Ingredient>();

            if (item == null)
                continue;

            // Предмет в руках не едим: сначала его нужно отпустить или бросить.
            if (Grab.Instance != null && Grab.Instance.Item == item)
                continue;

            Eat(item);
        }
    }

    private void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        Sprites.rotation = YawOnly
            ? Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f)
            : cam.transform.rotation;
    }

    private void Eat(GrabbingItem item)
    {
        Destroy(item.gameObject);

        if (eatRoutine != null)
            StopCoroutine(eatRoutine);

        eatRoutine = StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        SetState(Eaten);
        yield return new WaitForSeconds(EatDuration);

        eatRoutine = null;
        Refresh();
    }

    /// <summary>
    /// Выбирает Idle или Open по наличию игрока в зоне (пока идёт "съедено", не вмешивается).
    /// </summary>
    private void Refresh()
    {
        if (eatRoutine != null)
            return;

        SetState(playerColliders > 0 ? Open : Idle);
    }

    private void SetState(GameObject state)
    {
        Idle.SetActive(state == Idle);
        Open.SetActive(state == Open);
        Eaten.SetActive(state == Eaten);
    }

    private void OnDrawGizmosSelected()
    {
        if (Mouth == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Mouth.position, EatRadius);
    }
}
