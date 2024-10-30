using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-1)]

public class PlayerController : MonoBehaviour
{
    #region Class Variables
    [Header("Componenets")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private GameObject _cameraFollower;
    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Base Movement")]
    public float runAcceleration = 0.25f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 0.5f;
    public float sprintSpeed = 1.0f;
    public float drag = 0.1f;
    public float movingThreshold = 0.01f;
    public float inAirAcceleration = 0.15f;

    [Header("Animation Settings")]
    public float playerModelRotationSpeed = 10f;
    public float rotateToTargetTime = 0.25f;


    [Header("Jump Settings")]

    public float _verticalVelocity = 0f;
    public float gravity = 9.8f;
    public float jumpHeight = 1f;
    private float _terminalVelocity = 20f;

    private float _antiBump;
    private float _stepOffset;

    private PlayerMovementState _lastMovementState = PlayerMovementState.Fall;


    [Header("Camera Settings")]

    public Vector2 LookSense = new Vector3(0.1f, 0.1f);
    public float lookLimitY = 89f;

    [Header("Environmental Details")]
    [SerializeField] public LayerMask _groundLayers;

    private bool _jumpedLastFrame = false;


    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _camRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    private PlayerState _playerState;

    private float _rotateToTargetTimer = 0f;
    private bool _isRotatingClockwise = false;

    private GameObject colliderSphere;

    #endregion

    #region Startup

    private void addColliderSphere()
    {
        //colliderSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //colliderSphere.transform.position = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);
        //colliderSphere.transform.localScale = new Vector3(_characterController.radius, _characterController.radius, _characterController.radius);
        // GameObject.Instantiate(colliderSphere);
    }
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        addColliderSphere();
        _antiBump = sprintSpeed;
        _stepOffset = _characterController.stepOffset;

    }
    #endregion

    #region Update Logic
    private void UpdateMovementState()
    {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprint :
                                            isMovingLaterally || isMovementInput ? PlayerMovementState.Run : PlayerMovementState.Idle;



        _playerState.SetPlayerMovementState(lateralState);


        if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
        {

            _playerState.SetPlayerMovementState(PlayerMovementState.Jump);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0;

        }
        else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
        {

            _playerState.SetPlayerMovementState(PlayerMovementState.Fall);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0;

        } else
        {
            _characterController.stepOffset = _stepOffset;
        }



    }

    private void Update()
    {
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();

    }

    public void HandleVerticalMovement()
    {
        bool isGrounded = _playerState.InGroundedState();

        _verticalVelocity -= gravity * Time.deltaTime;

        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -_antiBump;

        }


        if (_playerLocomotionInput.JumpPressed && isGrounded)
        {
            _verticalVelocity += Mathf.Sqrt(jumpHeight * 3f * gravity);
            _jumpedLastFrame = true;
        }

        // switch from grounded to not grounded (jump/fall)
        if (_playerState.IsStateGroundedState(_lastMovementState) && isGrounded)
        {
            _verticalVelocity += _antiBump;
        }

        if(_verticalVelocity > _terminalVelocity)
        {
            _verticalVelocity = _terminalVelocity;
        }
    }

    public void HandleLateralMovement()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprint;
        bool isGrounded = _playerState.InGroundedState();


        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                      isSprinting ? sprintSpeed : runSpeed;


        Vector3 cameraForward = new Vector3(_playerCamera.transform.forward.x, 0, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraSide = new Vector3(_playerCamera.transform.right.x, 0, _playerCamera.transform.right.z).normalized;


        Vector3 movementDirection = cameraForward * _playerLocomotionInput.MovementInput.y + cameraSide * _playerLocomotionInput.MovementInput.x;

        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;


        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;


        //float movementPenalty = 1f;

        //if (_playerLocomotionInput.MovementInput.y < 0)
        //{
        //    clampLateralMagnitude *= movementPenalty;

        //} else if (_playerLocomotionInput.MovementInput.x != 0) {

        //    //clampLateralMagnitude *= movementPenalty * (Mathf.Abs(_playerLocomotionInput.MovementInput.x));

        //}

        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x,0,newVelocity.z), clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;

        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;


        _characterController.Move(newVelocity * Time.deltaTime);
    }

    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCase(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        if (!validAngle && _verticalVelocity < 0f)
        {
            velocity = Vector3.ProjectOnPlane(velocity, normal);
        }

        return velocity;
    }
    #endregion

    #region Late Update Logic
    public void LateUpdate()
    {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        _camRotation.x += LookSense.x * _playerLocomotionInput.LookInput.x;
        _camRotation.y = Mathf.Clamp(_camRotation.y - LookSense.y * _playerLocomotionInput.LookInput.y, -lookLimitY, lookLimitY);

        _playerTargetRotation.x += transform.eulerAngles.x + LookSense.x * _playerLocomotionInput.LookInput.x;

        float rotationTolerance = 90f;

        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idle;
        IsRotatingToTarget = _rotateToTargetTimer > 0f;


        if (!isIdling)
        {
            RotatePlayerToTarget();
        }

        else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
        {


            UpdateIdleRotation(rotationTolerance);

        }




        _cameraFollower.transform.rotation = Quaternion.Euler(_camRotation.y, _camRotation.x, 0f);

        Vector3 camForward = new Vector3(_cameraFollower.transform.forward.x, 0f, _cameraFollower.transform.forward.z).normalized;
        Vector3 crossP = Vector3.Cross(transform.forward, camForward);
        float sign = Mathf.Sign(Vector3.Dot(crossP, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForward);
    }

    private void UpdateIdleRotation(float rotationTolerance)
    {


        if (Mathf.Abs(RotationMismatch) > rotationTolerance)
        {
            _rotateToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }
        _rotateToTargetTimer -= Time.deltaTime;

        if (_isRotatingClockwise && RotationMismatch > 0f ||
            !_isRotatingClockwise && RotationMismatch < 0f)
        {

            RotatePlayerToTarget();

        }

    }

    private void RotatePlayerToTarget()
    {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime); // rotate player
    }

    #endregion

    #region State Checks
    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.y);
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()

    {

        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        return grounded;
    }

    private bool IsGroundedWhileAirborne()

    {

        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCase(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        return _characterController.isGrounded && validAngle;
    }

    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore); // _groundLayers, QueryTriggerInteraction.Ignore
        return grounded;
    }
    #endregion
}
