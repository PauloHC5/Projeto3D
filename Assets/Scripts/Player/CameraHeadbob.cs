using System;
using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    
    [SerializeField] private Vector2 Amount;
    [SerializeField] private Vector2 Frequency;
    
    [Header("Player Arms")]
    [SerializeField] private Transform playerArms;
    [SerializeField] private Vector2 ArmsAmount;
    [SerializeField] private Vector2 ArmsFrequency;

    private PlayerCharacterMovementController _playerCharacterMovementController;    
    private Vector3 _armsOriginalPosition;    
    private Vector3 _originalPosition;

    private void Awake()
    {
        _playerCharacterMovementController = GetComponentInParent<PlayerCharacterMovementController>();                
    }

    void Start()
    {
        if(playerArms) _armsOriginalPosition = playerArms.localPosition;
        else
            Debug.LogWarning("Player Arms not assigned in CameraHeadbob script.");
    }

    
    void LateUpdate()
    {        
        CheckForHeadbobTrigger();

        if (_playerCharacterMovementController.PlayerMovementStates == PlayerMovementStates.CROUCH || _playerCharacterMovementController.PlayerMovementStates == PlayerMovementStates.CROUCHING)
            Frequency /= 2f;
    }    

    private void CheckForHeadbobTrigger()
    {
        if (!_playerCharacterMovementController.IsGrounded) return;

        float inputMagnitude = _playerCharacterMovementController.PlayerMovementVelocityMagnitude;        

        if (inputMagnitude > 0f)
        {
            StartHeadbob();            
        }
        else StopHeadbob();
    }

    private void StartHeadbob()
    {
        Vector3 pos = Vector3.zero;
        pos.x += Mathf.Sin(Time.time * Frequency.x) * Amount.x;
        pos.y += Mathf.Sin(Time.time * Frequency.y) * Amount.y;
        transform.localPosition = pos;
        
        if (playerArms)
        {            
            Vector3 armsPos = Vector3.zero;
            armsPos.x += Mathf.Sin(Time.time * ArmsFrequency.x) * ArmsAmount.x;
            armsPos.y += Mathf.Sin(Time.time * ArmsFrequency.y) * ArmsAmount.y;
            playerArms.localPosition = armsPos;
        }
    }

    private void StopHeadbob()
    {
        transform.localPosition = Vector3.Slerp(transform.localPosition, _originalPosition, 1f * Time.smoothDeltaTime);
        
        if (playerArms) playerArms.localPosition = Vector3.Slerp(playerArms.localPosition, _armsOriginalPosition, 1f * Time.smoothDeltaTime);
    }
}
