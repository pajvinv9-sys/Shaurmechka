
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Movement movement;
    [SerializeField] private Jump jump;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform effectsPivot;

    [Header("Head Bob")]
    [SerializeField] private float walkAmplitude = 0.018f;
    [SerializeField] private float runAmplitude = 0.045f;
    [SerializeField] private float walkFrequency = 6.5f;
    [SerializeField] private float runFrequency = 9.5f;
    [SerializeField] private float bobSmoothSpeed = 10f;

    [Header("Idle Breathing")]
    [SerializeField] private float idleAmplitude = 0.2f;
    [SerializeField] private float idleFrequency = 0.15f;

    [Header("Transitions")]
    [SerializeField] private float transitionSpeed = 4f;

    [Header("FOV")]
    [SerializeField] private float walkFOV = 65f;
    [SerializeField] private float runFOV = 70f;
    [SerializeField] private float fovTransitionSpeed = 20f;

    [Header("Jump Camera")]
    [SerializeField] private float jumpOffset = 0.06f;
    [SerializeField] private float jumpSmoothTime = 0.15f;

    [Header("Fall Camera")]
    [SerializeField] private float fallOffset = 0.06f;
    [SerializeField] private float maxFallOffset = 0.14f;
    [SerializeField] private float maxFallSpeed = 15f;

    [Header("Landing")]
    [SerializeField] private float minImpactSpeed = 2f;
    [SerializeField] private float maxImpactSpeed = 15f;
    [SerializeField] private float minLandingOffset = 0.08f;
    [SerializeField] private float maxLandingOffset = 0.30f;
    [SerializeField] private float landingRecoveryTime = 0.30f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float bobTimer;
    private float currentBobY;
    private float currentIdleRotation;

    private float jumpFallOffset;
    private float jumpFallVelocity;

    private float landingOffset;
    private float landingVelocity;

    private float highestFallSpeed;
    private bool wasFalling;

    private void Awake()
    {
        if (effectsPivot == null)
            effectsPivot = transform;

        initialPosition = effectsPivot.localPosition;
        initialRotation = effectsPivot.localRotation;

        if (playerCamera == null)
            Debug.LogError("CameraEffects: Player Camera не назначена.");

        if (movement == null)
            Debug.LogError("CameraEffects: Movement не назначен.");

        if (jump == null)
            Debug.LogError("CameraEffects: Jump не назначен.");
    }

    private void LateUpdate()
    {
        if (movement == null || jump == null || playerCamera == null)
            return;

        UpdateHeadBob();
        UpdateIdleBreathing();
        UpdateJumpAndFall();
        UpdateFOV();
        ApplyCameraEffects();
    }

    private void UpdateHeadBob()
    {
        float targetBob = 0f;
        float frequency = walkFrequency;

        if (movement.IsWalk)
        {
            frequency = walkFrequency;
            targetBob = Mathf.Sin(bobTimer) * walkAmplitude;
        }
        else if (movement.IsRun)
        {
            frequency = runFrequency;
            targetBob = Mathf.Sin(bobTimer) * runAmplitude;
        }

        if (movement.IsWalk || movement.IsRun)
        {
            bobTimer += Time.deltaTime * frequency;
            targetBob = Mathf.Sin(bobTimer) *
                        (movement.IsRun ? runAmplitude : walkAmplitude);
        }
        else
        {
            bobTimer = 0f;
        }

        currentBobY = Mathf.Lerp(
            currentBobY,
            targetBob,
            Time.deltaTime * bobSmoothSpeed
        );
    }

    private void UpdateIdleBreathing()
    {
        float targetRotation = 0f;

        if (movement.IsStay && !jump.InJump)
        {
            targetRotation =
                Mathf.Sin(Time.time * idleFrequency * Mathf.PI * 2f)
                * idleAmplitude;
        }

        currentIdleRotation = Mathf.Lerp(
            currentIdleRotation,
            targetRotation,
            Time.deltaTime * transitionSpeed
        );
    }

    private void UpdateJumpAndFall()
    {
        float velocityY = movement.Rigidbody.linearVelocity.y;

        float targetOffset = 0f;

        if (jump.InJump)
        {
            if (velocityY > 0.1f)
            {
                // Камера слегка поднимается при прыжке.
                targetOffset = jumpOffset;
                wasFalling = false;
                highestFallSpeed = 0f;
            }
            else if (velocityY < -0.1f)
            {
                // Запоминаем максимальную скорость падения.
                wasFalling = true;

                highestFallSpeed = Mathf.Max(
                    highestFallSpeed,
                    Mathf.Abs(velocityY)
                );

                float fallAmount = Mathf.Clamp01(
                    Mathf.Abs(velocityY) / maxFallSpeed
                );

                targetOffset = -fallOffset * fallAmount;
            }
        }

        jumpFallOffset = Mathf.SmoothDamp(
            jumpFallOffset,
            targetOffset,
            ref jumpFallVelocity,
            jumpSmoothTime
        );

        // Приземление.
        if (!jump.InJump && wasFalling)
        {
            ApplyLandingEffect();

            wasFalling = false;
            highestFallSpeed = 0f;
        }

        // Восстановление камеры после приземления.
        landingOffset = Mathf.SmoothDamp(
            landingOffset,
            0f,
            ref landingVelocity,
            landingRecoveryTime
        );
    }

    private void ApplyLandingEffect()
    {
        float impact = Mathf.InverseLerp(
            minImpactSpeed,
            maxImpactSpeed,
            highestFallSpeed
        );

        landingOffset = Mathf.Lerp(
            minLandingOffset,
            maxLandingOffset,
            impact
        );
    }

    private void UpdateFOV()
    {
        float targetFOV = walkFOV;

        if (movement.IsRun)
            targetFOV = runFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovTransitionSpeed
        );
    }

    private void ApplyCameraEffects()
    {
        Vector3 targetPosition = initialPosition;

        targetPosition.y += currentBobY;
        targetPosition.y += jumpFallOffset;
        targetPosition.y -= landingOffset;

        effectsPivot.localPosition = Vector3.Lerp(
            effectsPivot.localPosition,
            targetPosition,
            Time.deltaTime * transitionSpeed
        );

        Quaternion targetRotation = initialRotation *
            Quaternion.Euler(currentIdleRotation, 0f, 0f);

        effectsPivot.localRotation = Quaternion.Slerp(
            effectsPivot.localRotation,
            targetRotation,
            Time.deltaTime * transitionSpeed
        );
    }
}
