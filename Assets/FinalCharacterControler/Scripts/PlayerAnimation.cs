using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;

    // CHECK ALTERNATIVE METHOD

    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");


    private Vector3 _currentBlendInput = Vector3.zero;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
       
        UpdateAnimationState();

    }

    private void UpdateAnimationState()
    {

        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprint;
        Vector2 inputTarget = isSprinting ?  _playerLocomotionInput.MovementInput * 1.5f : _playerLocomotionInput.MovementInput;


        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);


        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);


        _animator.SetBool(isGroundedHash, _playerState.InGroundedState());
        _animator.SetBool(isFallingHash, _playerState.CurrentPlayerMovementState == PlayerMovementState.Fall );
        _animator.SetBool(isJumpingHash, _playerState.CurrentPlayerMovementState == PlayerMovementState.Jump);
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);


        _animator.SetBool(isIdlingHash, _playerState.CurrentPlayerMovementState == PlayerMovementState.Idle);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
    }

}
