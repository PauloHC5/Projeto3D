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
    [SerializeField] private float acceleration = 5f; // New field for acceleration
    [SerializeField] private float deceleration = 5f; // New field for deceleration
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform meshRoot;
    [SerializeField] protected GameObject playerMesh;
    [SerializeField] protected Transform cameraPos;
    [ReadOnly]
    [SerializeField] private PlayerMovementStates playerMovementStates = PlayerMovementStates.DEFAULT;

    [Header("Crouch")]
    [SerializeField] private bool isCrouching = false;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchRadius = 0.3f;
    [SerializeField] private float crouchSmooth = 10f;

    [Space]
    [Header("Sway")]
    [SerializeField] private bool sway = true;
    [SerializeField] private float swayStep = 0.01f; // Multiplied by the value from the mouse for 1 frame
    [SerializeField] private float swayMaxStepDistance = 0.06f; // Max distance from the local origin
    [SerializeField] private float swaySmooth = 10f;        
    private Vector3 swayPos; // Store our value for later

    [Header("Sway Rotation")]
    [SerializeField] private bool swayRotation = true;
    [SerializeField] private float rotationStep = 4f;
    [SerializeField] private float maxRotationStep = 5f;
    [SerializeField] private float swaySmoothRot = 12f;    
    private Vector3 swayEulerRot;

    private float maxSpeed;
    private bool isGrounded;
    private Vector3 velocity;
    private bool hasAppliedCrouchImpulse = false;

    private float standingHeight; // Default height of the character controller
    private float standingRadius; // Default radius of the character controller    
    private Vector3 standingCameraPos; // Default position of the camera
    private Vector3 standingMeshRootPos; // Default position of the mesh root
    private Vector3 standingGroundCheckPos; // Default position of the ground check    

    private IEnumerator crouchingRoutine; 

    private Vector3 movementVelocity; // New field for movement velocity
    private Vector3 gravityVelocity; // New field for gravity velocity

    protected PlayerCombatStates playerCombatStates = PlayerCombatStates.DEFAULT;
    public PlayerCombatStates PlayerCombatStates
    {
        get { return playerCombatStates; }
        set { playerCombatStates = value; }
    }
    
    protected void IntializeMovement()
    {
        maxSpeed = walkSpeed;
        standingHeight = characterController.height;
        standingRadius = characterController.radius;
        standingCameraPos = cameraPos.localPosition;        
        standingMeshRootPos = meshRoot.transform.localPosition;
        standingGroundCheckPos = groundCheck.localPosition;
        playerMovementStates = PlayerMovementStates.DEFAULT;
    }

    protected void HandleMovement(Vector3 playerMovementInput, Vector2 playerLookInput)
    {
        isGrounded = CheckIfGrounded();

        if (isGrounded && gravityVelocity.y < 0) gravityVelocity.y = -2f;

        // Calculate movement direction
        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;

        if (playerMovementInput.magnitude > 0) // If we are moving
        {
            if(!isGrounded) // If we are not grounded, divide acceleration by half
                movementVelocity += move.normalized * acceleration /2f * Time.deltaTime;

            else // increase acceleration normally
                movementVelocity += move.normalized * acceleration * Time.deltaTime;

            movementVelocity = Vector3.ClampMagnitude(movementVelocity, maxSpeed);
        }
        else
        {
            // Decrease movement velocity based on deceleration
            movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
        
        characterController.Move((movementVelocity + new Vector3(0, gravityVelocity.y, 0)) * Time.deltaTime);

        Sway(playerLookInput);
        SwayRotation(playerLookInput);
        CompositePoitionRotation();

        float speed = characterController.velocity.magnitude;
        base.HandleLocomotion(speed, maxSpeed);
    }

    private void Sway(Vector2 lookInput) // x and y change as a result of moving the mouse
    {
        if(!sway) { swayPos = Vector3.zero; return; }

        Vector3 invertLook = lookInput * -swayStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -swayMaxStepDistance, swayMaxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -swayMaxStepDistance, swayMaxStepDistance);

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
        playerMesh.transform.localPosition = Vector3.Lerp(playerMesh.transform.localPosition, correctionHeight + swayPos, Time.deltaTime * swaySmooth);

        // rotation
        playerMesh.transform.localRotation = Quaternion.Lerp(playerMesh.transform.localRotation, Quaternion.Euler(swayEulerRot), Time.deltaTime * swaySmoothRot);
    }

    protected virtual void Jump()
    {        
        if(isGrounded) gravityVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);        
    }

    protected virtual void Crouch()
    {
        isCrouching = !isCrouching;

        // If we are already crouching, stop the routine and start a new one
        if (crouchingRoutine != null)
        {
            StopCoroutine(crouchingRoutine);
        }

        crouchingRoutine = CrouchingRoutine();
        StartCoroutine(crouchingRoutine);
    }

    private IEnumerator CrouchingRoutine()
    {
        Vector3 crouchCameraPos = new Vector3(cameraPos.localPosition.x, 1.25f, cameraPos.localPosition.z);
        Vector3 crouchMeshRootPos = new Vector3(playerMesh.transform.localPosition.x, 1.170f, playerMesh.transform.localPosition.z);   
        Vector3 crouchGrundCheckPos = new Vector3(groundCheck.localPosition.x, 0.6f, groundCheck.localPosition.z);        

        if (isCrouching)
        {
            playerMovementStates = PlayerMovementStates.CROUCHING;

            // Add impulse to the player when crouching and jumping
            if (!isGrounded && !hasAppliedCrouchImpulse)
            {
                gravityVelocity.y = Mathf.Sqrt((jumpForce / 2f) * -2f * gravity);
                hasAppliedCrouchImpulse = true;                
            }

            while (Mathf.Abs(characterController.height - crouchHeight) > 0.01f)
            {
                characterController.height = Mathf.Lerp(characterController.height, crouchHeight, Time.deltaTime * crouchSmooth);
                characterController.radius = Mathf.Lerp(characterController.radius, crouchRadius, Time.deltaTime * crouchSmooth);
                cameraPos.localPosition = Vector3.Lerp(cameraPos.localPosition, crouchCameraPos, Time.deltaTime * crouchSmooth);
                meshRoot.transform.localPosition = Vector3.Lerp(meshRoot.transform.localPosition, crouchMeshRootPos, Time.deltaTime * crouchSmooth);
                groundCheck.localPosition = Vector3.Lerp(groundCheck.localPosition, crouchGrundCheckPos, Time.deltaTime * crouchSmooth);                
                maxSpeed = crouchSpeed;
                isCrouching = true;
                yield return null;
            }
        }
        else
        {
            playerMovementStates = PlayerMovementStates.DEFAULT;

            if (!isGrounded) yield break;
            while (Mathf.Abs(characterController.height - standingHeight) > 0.01f)
            {
                characterController.height = Mathf.Lerp(characterController.height, standingHeight, Time.deltaTime * crouchSmooth);
                characterController.radius = Mathf.Lerp(characterController.radius, standingRadius, Time.deltaTime * crouchSmooth);
                cameraPos.localPosition = Vector3.Lerp(cameraPos.localPosition, standingCameraPos, Time.deltaTime * crouchSmooth);
                meshRoot.transform.localPosition = Vector3.Lerp(meshRoot.transform.localPosition, standingMeshRootPos, Time.deltaTime * crouchSmooth);
                groundCheck.localPosition = Vector3.Lerp(groundCheck.localPosition, standingGroundCheckPos, Time.deltaTime * crouchSmooth);                
                maxSpeed = walkSpeed;
                isCrouching = false;
                yield return null;
            }
        }
        
        if (isGrounded)
        {
            hasAppliedCrouchImpulse = false;
        }
    }

    protected void ApplyGravity()
    {
        // Apply gravity only to the gravity velocity
        gravityVelocity.y += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    protected bool CheckIfGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

}
