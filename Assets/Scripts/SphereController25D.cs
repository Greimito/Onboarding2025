using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Platformer25D
{
    /// <summary>
    /// Controlador 2.5D para un personaje esférico (movimiento horizontal en el eje X y salto en el eje Y, manteniendo Z fijo).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Platformer 2.5D/Sphere Controller 2.5D")]
    public class SphereController25D : MonoBehaviour
    {
        [Header("--- Movimiento Horizontal ---")]
        [Tooltip("Velocidad máxima de movimiento horizontal (eje X).")]
        [SerializeField] private float moveSpeed = 8f;

        [Tooltip("Aceleración al empezar a moverse.")]
        [SerializeField] private float acceleration = 55f;

        [Tooltip("Fricción / desaceleración al soltar las teclas.")]
        [SerializeField] private float deceleration = 65f;

        [Range(0f, 1f)]
        [Tooltip("Control de movimiento en el aire (1 = control total, 0 = sin control).")]
        [SerializeField] private float airControl = 0.75f;

        [Header("--- Salto y Gravedad ---")]
        [Tooltip("Fuerza del impulso de salto.")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Multiplicador de gravedad al caer (hace el salto más ágil y menos flotante).")]
        [SerializeField] private float fallMultiplier = 2.5f;

        [Tooltip("Multiplicador de gravedad al soltar el botón de salto antes del punto más alto (salto corto).")]
        [SerializeField] private float lowJumpMultiplier = 2f;

        [Tooltip("Tiempo de gracia (en segundos) para saltar después de caer de una plataforma (Coyote Time).")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Tiempo (en segundos) que se almacena la pulsación de salto antes de tocar el suelo (Jump Buffer).")]
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Tooltip("Número máximo de saltos permitidos (1 = salto normal, 2 = doble salto).")]
        [SerializeField] private int maxJumps = 1;

        [Header("--- Detección de Suelo (Ground Check) ---")]
        [Tooltip("Capas consideradas como suelo.")]
        [SerializeField] private LayerMask groundLayer = ~0; // Todo por defecto

        [Tooltip("Offset relativo desde el centro de la esfera para comprobar el suelo.")]
        [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, -0.45f, 0f);

        [Tooltip("Radio de la esfera de detección de suelo.")]
        [SerializeField] private float groundCheckRadius = 0.18f;

        [Header("--- Restricción 2.5D ---")]
        [Tooltip("Si está activo, bloquea y corrige la posición en el eje Z para mantener el plano 2.5D.")]
        [SerializeField] private bool lockZPosition = true;

        [Tooltip("Posición Z fija del carril 2.5D.")]
        [SerializeField] private float fixedZPosition = 0f;

        [Header("--- Efecto Visual de Rodamiento ---")]
        [Tooltip("Si está activo, la esfera gira visualmente al moverse a izquierda/derecha.")]
        [SerializeField] private bool enableRollingEffect = true;

        [Tooltip("Transform opcional de la malla visual hija. Si se deja vacío, se rota este mismo objeto.")]
        [SerializeField] private Transform visualMesh;

        [Tooltip("Multiplicador de velocidad de giro.")]
        [SerializeField] private float rollMultiplier = 1f;

        // Referencias de componentes
        private Rigidbody _rb;
        private SphereCollider _sphereCollider;

        // Variables de estado
        private float _horizontalInput;
        private bool _isJumpPressed;
        private bool _isJumpHeld;
        private bool _isGrounded;
        private int _jumpsRemaining;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        public bool IsGrounded => _isGrounded;
        public float HorizontalInput => _horizontalInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();

            // Configuración óptima del Rigidbody para 2.5D
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Congelamos Z y las rotaciones físicas para tener un control plataformero preciso sin deslizamientos raros
            _rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            if (lockZPosition && Mathf.Approximately(fixedZPosition, 0f))
            {
                fixedZPosition = transform.position.z;
            }
        }

        private void Update()
        {
            // 1. Lectura de Input
            ProcessInput();

            // 2. Control de Temporizadores (Coyote Time & Jump Buffer)
            UpdateTimers();

            // 3. Efecto visual de rotación de la esfera
            if (enableRollingEffect)
            {
                ApplyVisualRoll();
            }
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            HandleHorizontalMovement();
            HandleJumpAndGravity();
            EnforceZPlane();
        }

        private void ProcessInput()
        {
            _horizontalInput = ReadHorizontalInput();

            if (ReadJumpInputDown())
            {
                _jumpBufferCounter = jumpBufferTime;
            }

            _isJumpHeld = ReadJumpInputHeld();
        }

        private void UpdateTimers()
        {
            // Coyote Time
            if (_isGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
                _jumpsRemaining = maxJumps;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump Buffer
            if (_jumpBufferCounter > 0f)
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void CheckGrounded()
        {
            Vector3 checkPosition = transform.position + groundCheckOffset;
            Collider[] colliders = Physics.OverlapSphere(checkPosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

            bool groundedNow = false;
            foreach (var col in colliders)
            {
                // Ignorar el propio collider del jugador o sus hijos
                if (col != _sphereCollider && !col.transform.IsChildOf(transform))
                {
                    groundedNow = true;
                    break;
                }
            }

            _isGrounded = groundedNow;
        }

        private void HandleHorizontalMovement()
        {
            Vector3 currentVel = GetVelocity();

            float targetSpeed = _horizontalInput * moveSpeed;
            float currentSpeed = currentVel.x;

            // Determinar si acelera o frena
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

            // Reducir control en el aire si no está en el suelo
            if (!_isGrounded)
            {
                accelRate *= airControl;
            }

            float newVelX = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);
            currentVel.x = newVelX;

            SetVelocity(currentVel);
        }

        private void HandleJumpAndGravity()
        {
            Vector3 currentVel = GetVelocity();

            // Intento de salto desde suelo o con coyote time
            if (_jumpBufferCounter > 0f && (_coyoteTimeCounter > 0f || _jumpsRemaining > 0 && !_isGrounded))
            {
                ExecuteJump(ref currentVel);
            }

            // Mejoras de sensación de salto (Game feel)
            if (currentVel.y > 0f && !_isJumpHeld)
            {
                // Salto corto al soltar botón
                currentVel += Vector3.up * (Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (currentVel.y < 0f)
            {
                // Caída más rápida y satisfactoria
                currentVel += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime);
            }

            SetVelocity(currentVel);
        }

        private void ExecuteJump(ref Vector3 currentVel)
        {
            currentVel.y = jumpForce;
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
            _jumpsRemaining--;
        }

        private void EnforceZPlane()
        {
            if (!lockZPosition) return;

            Vector3 pos = transform.position;
            if (!Mathf.Approximately(pos.z, fixedZPosition))
            {
                pos.z = fixedZPosition;
                transform.position = pos;
            }

            Vector3 vel = GetVelocity();
            if (!Mathf.Approximately(vel.z, 0f))
            {
                vel.z = 0f;
                SetVelocity(vel);
            }
        }

        private void ApplyVisualRoll()
        {
            Vector3 currentVel = GetVelocity();
            float radius = (_sphereCollider != null) ? _sphereCollider.radius * transform.lossyScale.x : 0.5f;
            if (radius <= 0.001f) radius = 0.5f;

            // Distancia recorrida en X
            float deltaDistance = currentVel.x * Time.deltaTime;
            // Rotación en grados (v = w * r  =>  w = v / r)
            float angleDelta = (deltaDistance / radius) * Mathf.Rad2Deg * rollMultiplier;

            Transform target = visualMesh != null ? visualMesh : transform;
            // Al moverse hacia la derecha (+X), rota en sentido horario alrededor de Z (-Z)
            target.Rotate(Vector3.forward, -angleDelta, Space.World);
        }

        #region Helpers de Velocidad (Compatibilidad Unity 6 y versiones anteriores)

        private Vector3 GetVelocity()
        {
#if UNITY_6000_0_OR_NEWER
            return _rb.linearVelocity;
#else
            return _rb.velocity;
#endif
        }

        private void SetVelocity(Vector3 vel)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = vel;
#else
            _rb.velocity = vel;
#endif
        }

        #endregion

        #region Sistema de Input Unificado

        private float ReadHorizontalInput()
        {
            float input = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input += 1f;
            }
            if (Gamepad.current != null)
            {
                float stickX = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(stickX) > 0.15f) input = stickX;
                if (Gamepad.current.dpad.left.isPressed) input -= 1f;
                if (Gamepad.current.dpad.right.isPressed) input += 1f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Mathf.Approximately(input, 0f))
            {
                try
                {
                    input = Input.GetAxisRaw("Horizontal");
                }
                catch { }
            }
#endif

            return Mathf.Clamp(input, -1f, 1f);
        }

        private bool ReadJumpInputDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    return true;
            }
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                    return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    return true;
            }
            catch { }
#endif

            return false;
        }

        private bool ReadJumpInputHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    return true;
            }
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.isPressed)
                    return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetButton("Jump") || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    return true;
            }
            catch { }
#endif

            return false;
        }

        #endregion

        #region Gizmos en Editor

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 checkPosition = transform.position + groundCheckOffset;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }

        #endregion
    }
}
