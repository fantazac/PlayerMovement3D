using System.Collections;
using UnityEngine;

public class PlayerMovement_1 : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runBuildUpSpeed;

    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;

    [SerializeField] private AnimationCurve jumpFallOff;
    [SerializeField] private float jumpMultiplier;
    
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private KeyCode runKey;
    
    private bool isJumping;
    private float movementSpeed;

    private float baseSlopeLimit;

    private void Awake()
    {
        baseSlopeLimit = characterController.slopeLimit;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        var forwardMovement = transform.forward * verticalInput;
        var rightMovement = transform.right * horizontalInput;
        
        characterController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed);

        if ((verticalInput != 0 || horizontalInput != 0) && IsWalkingOnSlope())
        {
            characterController.Move(Vector3.down * (characterController.height * 0.5f * slopeForce * Time.deltaTime));
        }

        SetMovementSpeed();
        JumpInput();
    }

    private void SetMovementSpeed()
    {
        movementSpeed = Mathf.Lerp(movementSpeed, Input.GetKey(runKey) ? runSpeed : walkSpeed, Time.deltaTime * runBuildUpSpeed);
    }

    private bool IsWalkingOnSlope()
    {
        return !isJumping && Physics.Raycast(transform.position, Vector3.down, out var hit, characterController.height * 0.5f * slopeForceRayLength) && hit.normal != Vector3.up;
    }

    private void JumpInput()
    {
        if (!isJumping && Input.GetKeyDown(jumpKey))
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
        }
    }
    
    private IEnumerator JumpEvent()
    {
        characterController.slopeLimit = 90f;

        float timeInAir = 0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            characterController.Move(Vector3.up * (jumpForce * jumpMultiplier * Time.deltaTime));
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!characterController.isGrounded && characterController.collisionFlags != CollisionFlags.Above);

        characterController.slopeLimit = baseSlopeLimit;
        isJumping = false;
    }
}
