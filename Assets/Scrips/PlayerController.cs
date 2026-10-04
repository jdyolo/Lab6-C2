using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;

public class PlayerController : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private bool isPlayer2 = false;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float maxDashCharge = 2f;
    [SerializeField] private float dashDuration = 0.5f;

    [SerializeField]
    private AnimationCurve dashSpeedCurve =
        AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [SerializeField]
    private AnimationCurve dashChargeCurve =
        AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Retroceso")]
    [SerializeField] private float knockbackSpeed = 12f;
    [SerializeField] private float knockbackDuration = 0.35f;

    [SerializeField]
    private AnimationCurve knockbackCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player chargeFeedback;
    [SerializeField] private MMF_Player dashFeedback;
    [SerializeField] private MMF_Player impactFeedback;

    [Header("Intensidad Feedback")]
    [SerializeField] private float minShakeAmplitude = 0.1f;
    [SerializeField] private float maxShakeAmplitude = 0.6f;

    private PlayerControls controls;
    private Vector2 moveInput;

    private float dashCharge;
    private bool isChargingDash;

    private bool isDashing;
    private float dashTimer;
    private float dashForce;
    private Vector2 dashDirection;

    private bool isKnockedBack;
    private float knockbackTimer;
    private Vector2 knockbackDirection;
    private float knockbackForce = 1f;

    private Vector3 originalScale;

    private void Awake()
    {
        controls = new PlayerControls();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (isPlayer2)
        {
            controls.Player2.Enable();

            controls.Player2.Move.performed += OnMove;
            controls.Player2.Move.canceled += OnMove;

            controls.Player2.Dash.started += OnDashStarted;
            controls.Player2.Dash.performed += OnDashPerformed;
            controls.Player2.Dash.canceled += OnDashCanceled;
        }
        else
        {
            controls.Player.Enable();

            controls.Player.Move.performed += OnMove;
            controls.Player.Move.canceled += OnMove;

            controls.Player.Dash.started += OnDashStarted;
            controls.Player.Dash.performed += OnDashPerformed;
            controls.Player.Dash.canceled += OnDashCanceled;
        }
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        if (isPlayer2)
        {
            controls.Player2.Move.performed -= OnMove;
            controls.Player2.Move.canceled -= OnMove;

            controls.Player2.Dash.started -= OnDashStarted;
            controls.Player2.Dash.performed -= OnDashPerformed;
            controls.Player2.Dash.canceled -= OnDashCanceled;

            controls.Player2.Disable();
        }
        else
        {
            controls.Player.Move.performed -= OnMove;
            controls.Player.Move.canceled -= OnMove;

            controls.Player.Dash.started -= OnDashStarted;
            controls.Player.Dash.performed -= OnDashPerformed;
            controls.Player.Dash.canceled -= OnDashCanceled;

            controls.Player.Disable();
        }
    }

    private void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                knockbackTimer / knockbackDuration
            );

            float curveForce =
                knockbackCurve.Evaluate(t);

            transform.Translate(
                knockbackDirection *
                knockbackSpeed *
                knockbackForce *
                curveForce *
                Time.deltaTime
            );

            if (knockbackTimer >= knockbackDuration)
            {
                isKnockedBack = false;
                knockbackTimer = 0f;
            }

            return;
        }

        if (isDashing)
        {
            dashTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                dashTimer / dashDuration
            );

            float curveSpeed =
                dashSpeedCurve.Evaluate(t);

            transform.Translate(
                dashDirection *
                dashSpeed *
                dashForce *
                curveSpeed *
                Time.deltaTime
            );

            if (dashTimer >= dashDuration)
            {
                isDashing = false;
                dashTimer = 0f;

                if (dashFeedback != null)
                {
                    dashFeedback.StopFeedbacks();
                }

                transform.localScale = originalScale;
            }

            return;
        }

        transform.Translate(
            moveInput * moveSpeed * Time.deltaTime
        );

        if (isChargingDash)
        {
            dashCharge += Time.deltaTime;

            dashCharge = Mathf.Clamp(
                dashCharge,
                0f,
                maxDashCharge
            );
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput =
                context.ReadValue<Vector2>();
        }
    }

    private void OnDashStarted(
        InputAction.CallbackContext context)
    {
        if (isDashing || isKnockedBack)
            return;

        isChargingDash = true;
        dashCharge = 0f;

        if (chargeFeedback != null)
        {
            chargeFeedback.PlayFeedbacks();
        }

        Debug.Log(
            isPlayer2
                ? "PLAYER 2 comenzó a cargar Dash."
                : "PLAYER 1 comenzó a cargar Dash."
        );
    }

    private void OnDashPerformed(
        InputAction.CallbackContext context)
    {
        Debug.Log(
            isPlayer2
                ? "PLAYER 2 Dash performed."
                : "PLAYER 1 Dash performed."
        );
    }

    private void OnDashCanceled(
        InputAction.CallbackContext context)
    {
        if (!isChargingDash || isDashing)
            return;

        isChargingDash = false;

        if (chargeFeedback != null)
        {
            chargeFeedback.StopFeedbacks();
        }

        transform.localScale = originalScale;

        float chargeNormalized =
            Mathf.Clamp01(
                dashCharge / maxDashCharge
            );

        dashForce =
            dashChargeCurve.Evaluate(
                chargeNormalized
            );

        dashForce =
            Mathf.Max(dashForce, 0.25f);

        dashDirection =
            moveInput.normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection =
                isPlayer2
                    ? Vector2.left
                    : Vector2.right;
        }

        AjustarIntensidadFeedback(
            chargeNormalized
        );

        isDashing = true;
        dashTimer = 0f;

        if (dashFeedback != null)
        {
            dashFeedback.PlayFeedbacks();
        }

        Debug.Log(
            (isPlayer2 ? "PLAYER 2" : "PLAYER 1") +
            " DASH | Carga: " +
            dashCharge.ToString("F2") +
            " | Fuerza: " +
            dashForce.ToString("F2") +
            " | Intensidad: " +
            chargeNormalized.ToString("F2")
        );

        dashCharge = 0f;
    }

    private void AjustarIntensidadFeedback(
        float chargeNormalized)
    {
        if (dashFeedback == null)
            return;

        MMF_CameraShake cameraShake =
            dashFeedback
                .GetFeedbackOfType<MMF_CameraShake>();

        if (cameraShake == null)
            return;

        float amplitude =
            Mathf.Lerp(
                minShakeAmplitude,
                maxShakeAmplitude,
                chargeNormalized
            );

        cameraShake
            .CameraShakeProperties
            .Amplitude = amplitude;
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (!isDashing)
            return;

        PlayerController otherPlayer =
            collision.gameObject
                .GetComponent<PlayerController>();

        if (otherPlayer == null)
            return;

        Vector2 direction =
            (
                otherPlayer.transform.position -
                transform.position
            ).normalized;

        otherPlayer.ReceiveKnockback(
            direction,
            dashForce
        );

        if (impactFeedback != null)
        {
            impactFeedback.PlayFeedbacks();
        }

        Debug.Log(
            (isPlayer2 ? "PLAYER 2" : "PLAYER 1") +
            " impactó al rival con retroceso."
        );
    }

    public void ReceiveKnockback(
        Vector2 direction,
        float force)
    {
        knockbackDirection =
            direction.normalized;

        knockbackForce =
            Mathf.Max(force, 0.25f);

        knockbackTimer = 0f;
        isKnockedBack = true;

        isDashing = false;
        isChargingDash = false;

        Debug.Log(
            (isPlayer2 ? "PLAYER 2" : "PLAYER 1") +
            " recibió retroceso con AnimationCurve."
        );
    }

    public bool IsPlayer2()
    {
        return isPlayer2;
    }
}