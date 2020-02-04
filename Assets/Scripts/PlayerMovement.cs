using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float fallSpeedLimit;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeCheckRayLength;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private float baseSlopeLimit;
    private float baseStepOffset;

    private void Awake()
    {
        baseSlopeLimit = controller.slopeLimit;
        baseStepOffset = controller.stepOffset;
    }
    
    private void Update()
    {
        isGrounded = velocity.y <= 0f && Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        controller.slopeLimit = isGrounded ? baseSlopeLimit : 90f;
        controller.stepOffset = velocity.y <= 0f ? baseStepOffset : 0f;

        if (isGrounded && velocity.y < 0f)
        {
            isJumping = false;
            velocity.y = -2f;
        }
        
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");
        
        var move = transform.forward * zMove + transform.right * xMove;

        controller.Move(move * (movementSpeed * Time.deltaTime));
        
        if ((Math.Abs(xMove) > float.Epsilon || Math.Abs(zMove) > float.Epsilon) && IsWalkingOnSlope())
        {
            controller.Move(Vector3.down * (transform.localScale.y * 0.5f * slopeForce * Time.deltaTime));
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, fallSpeedLimit);
        
        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsWalkingOnSlope()
    {
        return !isJumping && Physics.Raycast(groundCheck.position, Vector3.down, out var hit, slopeCheckRayLength) && hit.normal != Vector3.up;
    }
}
