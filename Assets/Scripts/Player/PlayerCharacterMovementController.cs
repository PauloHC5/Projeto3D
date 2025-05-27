using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerMovementStates
{    
    CROUCHING,
    CROUCH,
    GETTINGUP,
    STANDING,

    DEFAULT
}

public class PlayerCharacterMovementController : MonoBehaviour
{    
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float acceleration = 5f; // New field for acceleration
    [SerializeField] private float deceleration = 5f; // New field for deceleration
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] protected CharacterController characterController;
    [SerializeField] private Transform playerMeshRoot;
    [SerializeField] private Transform playerMeshSway;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float impulseForce = 5f; // Impulse force to apply to the player


    [Header("Crouch")]
    [SerializeField] private bool isCrouching = false;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchRadius = 0.3f;    
    [SerializeField] private float crouchSmooth = 10f;
    [SerializeField] private float crouchCameraHeight;
    [SerializeField] private float crouchMeshRootHeight;
    [SerializeField] private float crouchGroundCheckHeight;
    [SerializeField] private Vector3 obstacleDetectorPosition;
    [SerializeField] private LayerMask obstacleMask; // Layer mask for obstacles
    [SerializeField] private float obstacleDetectorRadius = 0.5f; // Radius of the sphere used to detect obstacles
    [SerializeField] private bool DebugSpheresCheck = false;

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
    private bool hasAppliedCrouchImpulse = false;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;

    private float standingHeight; // Default height of the character controller
    private float standingRadius; // Default radius of the character controller        
    private Vector3 standingCameraPos; // Default position of the camera
    private Vector3 standingMeshRootPos; // Default position of the mesh root
    private Vector3 standingGroundCheckPos; // Default position of the ground check
                                            
    private Vector3 defaultMeshSwayPosition; // Default position of the mesh sway

    private IEnumerator crouchingRoutine;
    
    private Vector3 movementVelocity; // New field for movement velocity
    private Vector3 gravityVelocity; // New field for gravity velocity    
    private float decelerationTime = 0f; // New field to track deceleration time

    private PlayerMovementStates playerMovementStates = PlayerMovementStates.DEFAULT;    

    public PlayerMovementStates PlayerMovementStates => playerMovementStates;

    public float PlayerMovementVelocityMagnitude => movementVelocity.magnitude;
    public float PlayerMaxSpeed => maxSpeed;
    public bool IsGrounded => isGrounded;    

    private void Awake()
    {
        IntializeMovement();

        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
    }

    private void IntializeMovement()
    {
        maxSpeed = walkSpeed;
        standingHeight = characterController.height;
        standingRadius = characterController.radius;        
        standingCameraPos = cameraPos.localPosition;        
        standingMeshRootPos = playerMeshRoot.transform.localPosition;
        standingGroundCheckPos = groundCheck.localPosition;
        playerMovementStates = PlayerMovementStates.DEFAULT;     
        characterController = GetComponent<CharacterController>();
        defaultMeshSwayPosition = playerMeshSway.localPosition;
    }

    private void Update()
    {
        ApplyGravity();        
    }

    public void HandleMovement(Vector3 playerMovementInput, Vector2 playerLookInput)
    {
        isGrounded = CheckIfGrounded();

        if (isGrounded && gravityVelocity.y < 0) gravityVelocity.y = -2f;

        // Check for collision above and zero gravityVelocity if detected
        if (IsCollisionAbove()) gravityVelocity.y = -1f;        

        // Calculate movement direction
        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;

        if (playerMovementInput.magnitude > 0) // If we are moving
        {
            decelerationTime = 0f; // Reset deceleration time when moving

            if (characterController.velocity.magnitude < maxSpeed)
            {
                if (!isGrounded) // If we are not grounded, divide acceleration by half
                    movementVelocity += move.normalized * acceleration / 4f * Time.deltaTime;
                else // increase acceleration normally
                    movementVelocity += move.normalized * acceleration * Time.deltaTime;

                movementVelocity = Vector3.ClampMagnitude(movementVelocity, maxSpeed);
            }
            else
            {
                movementVelocity = move.normalized * maxSpeed;
            }
        }
        else
        {
            // Increase deceleration time
            decelerationTime += Time.deltaTime;

            // Gradually increase deceleration over time
            float currentDeceleration = Mathf.Lerp(0f, deceleration, decelerationTime);
            movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, currentDeceleration * Time.deltaTime);
        }

        MoveCharacter(movementVelocity, gravityVelocity);        

        Sway(playerLookInput);
        SwayRotation(playerLookInput);
        CompositePoitionRotation();        

        float speed = characterController.velocity.magnitude;
        playerCharacterAnimationsController.HandleLocomotion(speed, maxSpeed);        

        if (playerMovementStates == PlayerMovementStates.GETTINGUP && IsObstacleAbove())
        {
            isCrouching = true;

            // If we are already crouching, stop the routine and start a new one
            if (crouchingRoutine != null)
            {
                StopCoroutine(crouchingRoutine);
            }

            crouchingRoutine = CrouchingRoutine();
            StartCoroutine(crouchingRoutine);            
        }                
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
        // position
        playerMeshSway.localPosition = Vector3.Lerp(playerMeshSway.localPosition, defaultMeshSwayPosition + swayPos, Time.deltaTime * swaySmooth);

        // rotation
        playerMeshSway.localRotation = Quaternion.Lerp(playerMeshSway.localRotation, Quaternion.Euler(swayEulerRot), Time.deltaTime * swaySmoothRot);
    }

    public void Jump()
    {
        if (isCrouching && IsObstacleAbove()) return;

        if (isGrounded) gravityVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);        
    }

    public void Crouch()
    {
        if (isCrouching && IsObstacleAbove()) return;

        isCrouching = !isCrouching;

        // f we are already crouching, stop the routine and start a new one
        if (crouchingRoutine != null)
        {
            StopCoroutine(crouchingRoutine);
        }

        crouchingRoutine = CrouchingRoutine();
        StartCoroutine(crouchingRoutine);
    }

    private IEnumerator CrouchingRoutine()
    {
        if (isCrouching)
        {
            playerMovementStates = PlayerMovementStates.CROUCHING;

            // Add impulse to the player when crouching and jumping
            if (!isGrounded && !hasAppliedCrouchImpulse)
            {
                gravityVelocity.y = Mathf.Sqrt(1f * -2f * gravity);
                hasAppliedCrouchImpulse = true;
            }

            while (Mathf.Abs(characterController.height - crouchHeight) > 0.01f)
            {
                float previousHeight = characterController.height;
                characterController.height = Mathf.Lerp(characterController.height, crouchHeight, Time.deltaTime * crouchSmooth);
                float heightDifference = previousHeight - characterController.height;
                characterController.center = new Vector3(characterController.center.x, characterController.center.y - heightDifference / 2, characterController.center.z);

                // Adjust the player's position
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y - heightDifference / 2, transform.position.z), Time.deltaTime * crouchSmooth);

                characterController.radius = Mathf.Lerp(characterController.radius, crouchRadius, Time.deltaTime * crouchSmooth);
                groundCheck.localPosition = new Vector3(groundCheck.localPosition.x, crouchGroundCheckHeight, groundCheck.localPosition.z);
                cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, Mathf.Lerp(cameraPos.localPosition.y, crouchCameraHeight, Time.deltaTime * crouchSmooth), cameraPos.localPosition.z);
                playerMeshRoot.localPosition = new Vector3(playerMeshRoot.localPosition.x, Mathf.Lerp(playerMeshRoot.localPosition.y, crouchMeshRootHeight, Time.deltaTime * crouchSmooth), playerMeshRoot.localPosition.z);
                maxSpeed = crouchSpeed;
                isCrouching = true;
                yield return null;
            }

            playerMovementStates = PlayerMovementStates.CROUCH;
        }
        else
        {           
            if(!isGrounded) yield break;

            playerMovementStates = PlayerMovementStates.GETTINGUP;

            while (Mathf.Abs(characterController.height - standingHeight) > 0.01f)
            {                
                float previousHeight = characterController.height;
                characterController.height = Mathf.Lerp(characterController.height, standingHeight, Time.deltaTime * crouchSmooth);
                float heightDifference = previousHeight - characterController.height;
                characterController.center = new Vector3(characterController.center.x, characterController.center.y - heightDifference / 2, characterController.center.z);

                // Adjust the player's position
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y - heightDifference / 2, transform.position.z), Time.deltaTime * crouchSmooth);

                characterController.radius = Mathf.Lerp(characterController.radius, standingRadius, Time.deltaTime * crouchSmooth);
                groundCheck.localPosition = new Vector3(groundCheck.localPosition.x, standingGroundCheckPos.y, groundCheck.localPosition.z);
                cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, Mathf.Lerp(cameraPos.localPosition.y, standingCameraPos.y, Time.deltaTime * crouchSmooth), cameraPos.localPosition.z);
                playerMeshRoot.localPosition = new Vector3(playerMeshRoot.localPosition.x, Mathf.Lerp(playerMeshRoot.localPosition.y, standingMeshRootPos.y, Time.deltaTime * crouchSmooth), playerMeshRoot.localPosition.z);
                maxSpeed = walkSpeed;
                isCrouching = false;
                yield return null;
            }

            playerMovementStates = PlayerMovementStates.STANDING;
        }

        if (isGrounded)
        {
            hasAppliedCrouchImpulse = false;
        }
    }

    private bool IsObstacleAbove()
    {        
        Vector3 spherePosition = transform.position + obstacleDetectorPosition;
        if (Physics.CheckSphere(spherePosition, obstacleDetectorRadius, obstacleMask))
        {
            isCrouching = true;            
            return true; // Obstacle detected
        }
        return false; // No obstacle detected
    }

    private void OnDrawGizmos()
    {
        if(DebugSpheresCheck)
        {
            // Draw the sphere used in IsObstacleAbove for visualization
            Gizmos.color = Color.red;            
            Vector3 spherePosition = transform.position + obstacleDetectorPosition;
            Gizmos.DrawWireSphere(spherePosition, obstacleDetectorRadius);

            // Debug groud check sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // Debug isCollisionAbove sphere
            Gizmos.color = Color.blue;
            Vector3 spherePositionAbove = transform.position + Vector3.up * (characterController.height + 0.3f);
            Gizmos.DrawWireSphere(spherePositionAbove, characterController.radius);
        }
    }

    protected void ApplyGravity()
    {
        // Apply gravity only to the gravity velocity
        gravityVelocity.y += gravity * Time.deltaTime;
        if(gravityVelocity.x != 0)
        {
            gravityVelocity.x = Mathf.Lerp(gravityVelocity.x, 0, Time.deltaTime * deceleration);
        }

        if(gravityVelocity.z != 0)
        {
            gravityVelocity.z = Mathf.Lerp(gravityVelocity.z, 0, Time.deltaTime * deceleration);
        }

        MoveCharacter(movementVelocity, gravityVelocity);
    }

    private void ApplyImpulse()
    {
        // Apply impulse to the player in the backward direction
        Vector3 impulseDirection = -Camera.main.transform.forward; // Backward direction
        gravityVelocity += impulseDirection * impulseForce; // Apply impulse force        
    }

    private void MoveCharacter(Vector3 movement, Vector3 gravity)
    {
        characterController.Move((movement + gravity) * Time.deltaTime);
    }

    protected bool CheckIfGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    // New method to detect collision above the player
    private bool IsCollisionAbove()
    {
        Vector3 spherePosition = transform.position + Vector3.up * (characterController.height + 0.3f);
        return Physics.CheckSphere(spherePosition, characterController.radius, obstacleMask);
    }    
}
