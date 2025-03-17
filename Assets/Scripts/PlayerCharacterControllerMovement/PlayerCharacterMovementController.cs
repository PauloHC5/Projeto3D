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
    [SerializeField] protected GameObject playerMesh;

    [Header("Sway and Bobbing")]
    [SerializeField] protected float smooth = 10f;
    [SerializeField] protected float smoothRot = 12f;
    [Space]
    [Header("Sway")]
    [SerializeField] protected bool sway = true;
    [SerializeField] protected float step = 0.01f; // Multiplied by the value from the mouse for 1 frame
    [SerializeField] protected float maxStepDistance = 0.06f; // Max distance from the local origin
    private Vector3 swayPos; // Store our value for later

    [Header("Sway Rotation")]
    [SerializeField] protected bool swayRotation = true;
    [SerializeField] protected float rotationStep = 4f;
    [SerializeField] protected float maxRotationStep = 5f;
    private Vector3 swayEulerRot;

    protected bool isGrounded;
    protected Vector3 velocity;    
    
    protected PlayerStates playerStates = PlayerStates.DEFAULT;    


    protected void HandleMovement(Vector3 playerMovementInput, Vector2 playerLookInput)
    {
        isGrounded = CheckIfGrounded();

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;
        characterController.Move(move * speed * Time.deltaTime);

        Sway(playerLookInput);
        SwayRotation(playerLookInput);

        base.HandleMovement(characterController.velocity.magnitude);
    }

    protected void Sway(Vector2 lookInput) // x and y change as a result of moving the mouse
    {
        if(!sway) { swayPos = Vector3.zero; return; }

        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    protected void SwayRotation(Vector2 lookInput)
    {
        if (!swayRotation) { swayPos = Vector3.zero; return; }

        Vector2 look = lookInput * rotationStep;
        look.x = Mathf.Clamp(look.x, -maxRotationStep, maxRotationStep);
        look.y = Mathf.Clamp(look.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new Vector3(look.y, look.x, -look.x);
    }

    protected void CompositePoitionRotation()
    {
        // correction height for the player mesh
        Vector3 correctionHeight = new Vector3(0f, -1.65f, 0f);

        // position
        playerMesh.transform.localPosition = Vector3.Lerp(playerMesh.transform.localPosition, correctionHeight + swayPos, Time.deltaTime * smooth);

        // rotation
        playerMesh.transform.localRotation = Quaternion.Lerp(playerMesh.transform.localRotation, Quaternion.Euler(swayEulerRot), Time.deltaTime * smoothRot);
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
