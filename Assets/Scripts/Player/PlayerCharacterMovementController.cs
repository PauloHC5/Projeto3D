using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerMovementStates
{    
    CROUCHING,

    DEFAULT
}

public class ReadOnlyAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}

public class PlayerCharacterMovementController : PlayerCharacterAnimationsController
{    
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] protected GameObject playerMesh;
    [SerializeField] protected Transform cameraPos;
    [ReadOnly]
    [SerializeField] private PlayerMovementStates playerMovementStates;

    [Header("Crouch")]
    [SerializeField] private bool isCrouching = false;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;

    [Space]
    [Header("Sway")]
    [SerializeField] private bool sway = true;
    [SerializeField] private float step = 0.01f; // Multiplied by the value from the mouse for 1 frame
    [SerializeField] private float maxStepDistance = 0.06f; // Max distance from the local origin
    [SerializeField] private float smooth = 10f;        
    private Vector3 swayPos; // Store our value for later

    [Header("Sway Rotation")]
    [SerializeField] private bool swayRotation = true;
    [SerializeField] private float rotationStep = 4f;
    [SerializeField] private float maxRotationStep = 5f;
    [SerializeField] private float smoothRot = 12f;    
    private Vector3 swayEulerRot;

    protected float maxSpeed;
    protected bool isGrounded;
    protected Vector3 velocity;    
    
    protected PlayerCombatStates playerCombatStates = PlayerCombatStates.DEFAULT;
    public PlayerCombatStates PlayerCombatStates
    {
        get { return playerCombatStates; }
        set { playerCombatStates = value; }
    }
    
    protected void IntializeMovement()
    {
        maxSpeed = walkSpeed;
    }

    protected void HandleMovement(Vector3 playerMovementInput, Vector2 playerLookInput)
    {
        isGrounded = CheckIfGrounded();

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;
        characterController.Move(move * maxSpeed * Time.deltaTime);

        Sway(playerLookInput);
        SwayRotation(playerLookInput);
        CompositePoitionRotation();

        float speed = characterController.velocity.magnitude;
        base.HandleLocomotion(speed, maxSpeed);
    }

    private void Sway(Vector2 lookInput) // x and y change as a result of moving the mouse
    {
        if(!sway) { swayPos = Vector3.zero; return; }

        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    private void SwayRotation(Vector2 lookInput)  // x and y change as a result of moving the mouse
    {
        if (!swayRotation) { swayPos = Vector3.zero; return; }

        Vector2 look = lookInput * rotationStep;
        look.x = Mathf.Clamp(look.x, -maxRotationStep, maxRotationStep);
        look.y = Mathf.Clamp(look.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new Vector3(look.y, look.x, -look.x);
    }

    private void CompositePoitionRotation()
    {
        // correction height for the player mesh
        Vector3 correctionHeight = new Vector3(0f, -1.65f, 0f);

        // position
        playerMesh.transform.localPosition = Vector3.Lerp(playerMesh.transform.localPosition, correctionHeight + swayPos, Time.deltaTime * smooth);

        // rotation
        playerMesh.transform.localRotation = Quaternion.Lerp(playerMesh.transform.localRotation, Quaternion.Euler(swayEulerRot), Time.deltaTime * smoothRot);
    }

    protected virtual void Jump()
    {        
        if(isGrounded) velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);        
    }

    protected virtual void Crouch()
    {
        if (!isCrouching)
        {
            characterController.height = crouchHeight;
            maxSpeed = crouchSpeed;
            isCrouching = true;
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, cameraPos.localPosition.y / 2f, cameraPos.localPosition.z);
            playerMesh.transform.localPosition = new Vector3(playerMesh.transform.localPosition.x, playerMesh.transform.localPosition.y / 2f, playerMesh.transform.localPosition.z);
        }
        else
        {
            characterController.height = standingHeight;
            maxSpeed = walkSpeed;
            isCrouching = false;
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
