using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;

    private float _animatorSpeed;
    [SerializeField] private float _animatorSmoothness = 6f;

    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _jumpAction;
    private InputAction _aimAction;

    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _smoothTime = 0.05f; // Valor bajo para giro inmediato

    private float _turnSmoothVelocity;
    private float _gravity;
    [SerializeField] private Vector3 _playerGravity;

    [SerializeField] private Transform _sensorTransform;
    [SerializeField] private float _sensorRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    private Transform _cameraTransform;

    private bool _isDead = false;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _aimAction = InputSystem.actions["Aim"];

        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    void Start()
    {
        _gravity = Physics.gravity.y;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (_isDead) return;

        _moveInput = _moveAction.ReadValue<Vector2>();

        Gravity();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        _animator.SetBool("IsJumping", !IsGrounded());

        if (_aimAction.IsPressed())
        {
            AimMovement();
        }
        else
        {
            TPMovement();
        }
    }

   void TPMovement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        float targetSpeed = direction.magnitude;

        // Transición progresiva del valor entre 0 y 1
        _animatorSpeed = Mathf.MoveTowards(_animatorSpeed, targetSpeed, _animatorSmoothness * Time.deltaTime);
    
        _animator.SetFloat("Vertical", _animatorSpeed);
        _animator.SetFloat("Horizontal", 0f);

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
        
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDir.normalized * _movementSpeed * Time.deltaTime);
        }
    }

    void AimMovement()
    {
        // En apuntado suavizamos ambos ejes
        float targetVertical = _moveInput.y;
        float targetHorizontal = _moveInput.x;

        _animator.SetFloat("Vertical", Mathf.MoveTowards(_animator.GetFloat("Vertical"), targetVertical, _animatorSmoothness * Time.deltaTime));
        _animator.SetFloat("Horizontal", Mathf.MoveTowards(_animator.GetFloat("Horizontal"), targetHorizontal, _animatorSmoothness * Time.deltaTime));

        float targetAngle = _cameraTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float moveAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, moveAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDir.normalized * _movementSpeed * Time.deltaTime);
        }
    }
    void Gravity()
    {
        if (!IsGrounded())
        {
            _playerGravity.y += _gravity * Time.deltaTime;
        }
        else if (IsGrounded() && _playerGravity.y < 0)
        {
            _playerGravity.y = -2f;
        }

        _characterController.Move(_playerGravity * Time.deltaTime);
    }

    void Jump()
    {
        _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensorTransform.position, _sensorRadius, _groundLayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Muerte") && !_isDead)
        {
            _isDead = true;
            
            // Activa la animación de muerte
            _animator.SetBool("IsDeath", true);

            // Resetea los parámetros del Blend Tree para detener animaciones de caminar inmediatamente
            _animator.SetFloat("Vertical", 0f);
            _animator.SetFloat("Horizontal", 0f);
        }
    }
}