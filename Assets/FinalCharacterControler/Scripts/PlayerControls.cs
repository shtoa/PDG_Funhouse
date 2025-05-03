using System;
using System.Collections;
using System.Collections.Generic;
using tutorialGenerator;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-1)]

public class PlayerController : MonoBehaviour
{
    #region Class Variables
    [Header("Componenets")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Rigidbody _rigidbody;
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


    [SerializeField] public Transform orientation;

    private bool _jumpedLastFrame = false;


    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _camRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    private PlayerState _playerState;

    private float _rotateToTargetTimer = 0f;
    private bool _isRotatingClockwise = false;

    private GameObject colliderSphere;

    public bool isClimbing = false; // FIX ME: REFACTOR
    public bool isSliding = false;
    private float _cameraPositionY;

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
        _cameraPositionY = _playerCamera.transform.position.y;

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

     

        if ((!isGrounded || _jumpedLastFrame) && _rigidbody.velocity.y > 0f) // _characterController
        {

            _playerState.SetPlayerMovementState(PlayerMovementState.Jump);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0;

        }
        else if ((!isGrounded || _jumpedLastFrame) && _rigidbody.velocity.y < 0f)
        {

            _playerState.SetPlayerMovementState(PlayerMovementState.Fall);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0;

        }
        else
        {
            _characterController.stepOffset = _stepOffset;
        }


        if (isClimbing)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Climb);
        }


        if ((_playerLocomotionInput.IsSliding && isGrounded) || (_lastMovementState == PlayerMovementState.Slide && _playerLocomotionInput.IsSliding))
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Slide);
        }

    }

    private void Update()
    {
        Movement();
        WallRun();

    }


    public LayerMask WallMask;
    public float wallRunForce, maxWallRunTime, maxWallSpeed;
    bool isWallRight, isWallLeft;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    public float playerHeight = 0.5f;
    private Vector3 slideDirection = new Vector3(0,0,0);

    // https://www.youtube.com/watch?v=Ryi9JxbMCFM&list=PLh9SS5jRVLAleXEcDTWxBF39UjyrFc6Nb&index=2&ab_channel=Dave%2FGameDevelopment
    private void WallRun()
    {
        CheckForWall();
        WallRunInput();
    }

    private void WallRunInput()
    {
        bool isGrounded = _playerState.InGroundedState();
        if (_playerLocomotionInput.MovementInput.x > 0f && isWallRight && !isGrounded)
        {
            StartWallRun();
        } else if (_playerLocomotionInput.MovementInput.x < 0f && isWallLeft && !isGrounded) 
        {
            StartWallRun();
        }
    }

    private void StartWallRun()
    {
        _rigidbody.useGravity = false;
        isWallRunning = true;

        if(_rigidbody.velocity.magnitude < maxWallSpeed)
        {
            //Debug.Log("ADDING SPEED");

            Vector3 laterFWD = new Vector3(orientation.forward.x, 0, orientation.forward.z);
            laterFWD.Normalize();
       


            _rigidbody.AddForce(laterFWD * wallRunForce);

            if (isWallRight)
            {
                _rigidbody.AddForce(new Vector3(orientation.right.x, 0, orientation.right.z) * wallRunForce/5f);

            } else
            {
                _rigidbody.AddForce(-new Vector3(orientation.right.x,0, orientation.right.z) * wallRunForce/5f);
            }
        } 
    }

    private void StopWallRun()
    {
        _rigidbody.useGravity = true;
        isWallRunning = false;
    }

    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 0.2f,WallMask);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 0.2f,WallMask);

        if (!isWallRight && !isWallLeft) {

            StopWallRun();
        }

    }

    private void Movement()
    {
        UpdateMovementState();
        // HandleVerticalMovement();
        SpeedControl();
        HandleLateralMovement();

        checkJump();

        HandleDrag();
    }

    public void SpeedControl()
    {
        _rigidbody.velocity = Vector3.ClampMagnitude(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z), 7f)+Vector3.up* Math.Min(_rigidbody.velocity.y,8f);
       
    }

    private void HandleDrag()
    {
        bool isGrounded = _playerState.InGroundedState(); 
        if (isGrounded)
        {
            _rigidbody.drag = 5;
        } else
        {
           _rigidbody.drag = 0;
        }
    }

    //private void Update()
    //{

    //}
    public bool hasLaunchedForward = false;

    public void checkJump()
    {
        bool isGrounded = _playerState.InGroundedState();
        if (_playerLocomotionInput.JumpPressed && isGrounded && !isWallRunning)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up*14f, ForceMode.Impulse);

            hasLaunchedForward = false;



        } else if (!isGrounded){
            if (_playerLocomotionInput.performedStab && !hasLaunchedForward)
            {
                Vector3 laterFWD = new Vector3(orientation.forward.x, 0, orientation.forward.z);
                laterFWD.Normalize();
                _rigidbody.AddForce(laterFWD * 1000f*4f, ForceMode.Force);
                hasLaunchedForward = true;
            }
            //_rigidbody.AddForce(-Vector3.up * 30f, ForceMode.Force);
            if (isWallRunning && _playerLocomotionInput.JumpPressed)
            {
                if (isWallLeft && _playerLocomotionInput.MovementInput.x > 0)
                {
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                    _rigidbody.AddForce(Vector3.up * 5f + _playerCamera.transform.right * 10f, ForceMode.Impulse); //  + _playerCamera.transform.right*10f
                } 
                else if (isWallRight && _playerLocomotionInput.MovementInput.x < 0)
                {
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                    _rigidbody.AddForce(Vector3.up * 5f - _playerCamera.transform.right * 10f, ForceMode.Impulse); //- _playerCamera.transform.right*10f
                }


            }
        }
    }

    public void HandleVerticalMovement()
    {
        // FIX ME: REFACTOR INTO SEPARATE PART 
        if (isClimbing && _playerLocomotionInput.MovementInput.y > 0)
        {
            _verticalVelocity = 5;
        } else if (isClimbing && _playerLocomotionInput.MovementInput.y < 0)
        {
            _verticalVelocity = -5;
        } else if (isClimbing)
        {
            _verticalVelocity = 0;
        }
        else
        {
            bool isGrounded = _playerState.InGroundedState();

            _verticalVelocity -= gravity * Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -_antiBump;

            }


            if (_playerLocomotionInput.JumpPressed && isGrounded) //  && isGrounded allow to jump multiple times
            {
                _verticalVelocity += Mathf.Sqrt(jumpHeight * 3f * gravity);
                _jumpedLastFrame = true;
            }

            // switch from grounded to not grounded (jump/fall)
            if (_playerState.IsStateGroundedState(_lastMovementState) && isGrounded)
            {
                _verticalVelocity += _antiBump;
            }

            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity = _terminalVelocity;
            }

         
            
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


        // movementDirection.y += _verticalVelocity;
        // Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        // Vector3 newVelocity = _rigidbody.velocity + movementDelta;


        // Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        // newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;


        //float movementPenalty = 1f;

        //if (_playerLocomotionInput.MovementInput.y < 0)
        //{
        //    clampLateralMagnitude *= movementPenalty;

        //} else if (_playerLocomotionInput.MovementInput.x != 0) {

        //    //clampLateralMagnitude *= movementPenalty * (Mathf.Abs(_playerLocomotionInput.MovementInput.x));

        //}

        //newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x,0,newVelocity.z), clampLateralMagnitude);
        //newVelocity.y += _verticalVelocity;

        //newVelocity = (!isGrounded && !isClimbing) ? HandleSteepWalls(newVelocity) : newVelocity;


        //_characterController.Move(newVelocity * Time.deltaTime);


        if (_playerState.CurrentPlayerMovementState == PlayerMovementState.Slide)
        {
            if (slideDirection.Equals(Vector3.zero)) {
                slideDirection = cameraForward;
            }
            _rigidbody.AddForce(slideDirection * 1000f *  Time.deltaTime, ForceMode.Impulse);
            _rigidbody.AddForce(-Vector3.up * 1000f * Time.deltaTime, ForceMode.Impulse);
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<SphereCollider>().enabled = true;

        }
        else
        {
            if (!GetComponent<CapsuleCollider>().enabled)
            {
                GetComponent<CapsuleCollider>().enabled = true;
                GetComponent<SphereCollider>().enabled = false;
                _rigidbody.AddForce(Vector3.up * 500f * Time.deltaTime, ForceMode.Impulse);
                _rigidbody.position = new Vector3(_rigidbody.position.x, _rigidbody.position.y + 0.5f, _rigidbody.position.z);
            }

            //}
            slideDirection = Vector3.zero;
            if (isGrounded) _rigidbody.AddForce(movementDirection * 1000f * 5f * Time.deltaTime, ForceMode.Force);
            else if (!isWallRunning)
            {
                _rigidbody.AddForce(movementDirection * 1000f * 1.2f * Time.deltaTime, ForceMode.Force);

            }
        }

        //Debug.Log(_rigidbody.velocity);

    }

    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCase(GetComponent<CapsuleCollider>(), _groundLayers);
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

        float rotationTolerance = 0f; // 90f

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




        //_cameraFollower.transform.rotation = Quaternion.Euler(_camRotation.y, _camRotation.x, 0f);


        _playerCamera.transform.rotation = Quaternion.Euler(_camRotation.y, _camRotation.x, 0f);

        Vector3 camForward = new Vector3(_cameraFollower.transform.forward.x, 0f, _cameraFollower.transform.forward.z).normalized;


        //Vector3 camForward = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossP = Vector3.Cross(transform.forward, camForward);
        float sign = Mathf.Sign(Vector3.Dot(crossP, transform.up));


        RotationMismatch = sign * Vector3.Angle(transform.forward, camForward);

        
        if (isWallRunning && Mathf.Abs(wallRunCameraTilt) <= 20f)
        {
            if (isWallLeft)
            {
                wallRunCameraTilt -= 1f;
            } else if (isWallRight)
            {
                wallRunCameraTilt += 1f;
            }
        } else if (!isWallRunning && wallRunCameraTilt != 0)
        {
            wallRunCameraTilt -= Mathf.Sign(wallRunCameraTilt)*1f;
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_camRotation.y, _camRotation.x, wallRunCameraTilt);
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
        Vector3 lateralVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.y);
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()

    {

        bool grounded = IsGroundedWhileGrounded(); //_playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        return grounded;
    }

    private bool IsGroundedWhileAirborne()

    {

        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCase(GetComponent<CapsuleCollider>(), _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        return _characterController.isGrounded; //&& validAngle;
    }

    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GetComponent<CapsuleCollider>().radius, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, GetComponent<CapsuleCollider>().radius, _groundLayers, QueryTriggerInteraction.Ignore); // _groundLayers, QueryTriggerInteraction.Ignore
        return grounded;
    }
    #endregion
}
