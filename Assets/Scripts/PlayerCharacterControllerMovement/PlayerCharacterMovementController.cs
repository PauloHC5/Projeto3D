using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMovementController : PlayerCharacterAnimationsController
{    
    [Header("Movement")]
    [SerializeField] protected float speed;
    [SerializeField] protected float jumpForce;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundDistance = 0.4f;
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] protected CharacterController characterController;

    protected bool isGrounded;
    protected Vector3 velocity;    
    
    protected PlayerStates playerStates = PlayerStates.DEFAULT;    


    protected void HandleMovement(Vector3 playerMovementInput)
    {
        isGrounded = CheckIfGrounded();

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;
        characterController.Move(move * speed * Time.deltaTime);

        base.HandleMovement(characterController.velocity.magnitude);
    }

    protected void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    protected void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    protected bool CheckIfGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

}
