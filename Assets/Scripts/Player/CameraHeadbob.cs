using System;
using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    [SerializeField] private Transform target;

    //[Range(0.001f, 0.1f)]
    [SerializeField] private Vector2 Amount;

    //[Range(1f, 20)]
    [SerializeField] private Vector2 Frequency;                  

    private PlayerCharacterMovementController playerCharacterMovementController;    
    private Vector3 originalPosition;    

    private void Awake()
    {
        playerCharacterMovementController = GetComponentInParent<PlayerCharacterMovementController>();                
    }

    void Start()
    {
        originalPosition = target.localPosition;
    }

    
    void LateUpdate()
    {        
        CheckForHeadbobTrigger();

        if (playerCharacterMovementController.PlayerMovementStates == PlayerMovementStates.CROUCH || playerCharacterMovementController.PlayerMovementStates == PlayerMovementStates.CROUCHING)
            Frequency /= 2f;
    }    

    private void CheckForHeadbobTrigger()
    {
        if (!playerCharacterMovementController.IsGrounded) return;

        float inputMagnitude = playerCharacterMovementController.PlayerMovementVelocityMagnitude;        

        if (inputMagnitude > 0f)
        {
            StartHeadbob();            
        }
        else StopHeadbob();
    }

    private void StartHeadbob()
    {
        if (target)
        {            
            Vector3 armsPos = Vector3.zero;
            armsPos.x += Mathf.Sin(Time.time * Frequency.x) * Amount.x;
            armsPos.y += Mathf.Sin(Time.time * Frequency.y) * Amount.y;
            target.localPosition = armsPos;
        }
    }

    private void StopHeadbob()
    {
        if (target) target.localPosition = Vector3.Slerp(target.localPosition, originalPosition, 1f * Time.smoothDeltaTime);
    }
}
