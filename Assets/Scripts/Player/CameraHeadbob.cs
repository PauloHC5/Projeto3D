using System;
using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    [SerializeField] private Transform mainCameraPos;
    [SerializeField] private Transform armsHeadbob;

    [Range(0.001f, 1f)]
    [SerializeField] private float Amount = 0.002f;

    [Range(1f, 30f)]
    [SerializeField] private float Frequency = 10f;                  

    [Range(10f, 100f)]
    [SerializeField] private float Smooth = 10f;    

    private PlayerCharacterMovementController playerCharacterMovementController;    
    private Vector3 originalPosition;    

    private void Awake()
    {
        playerCharacterMovementController = GetComponentInParent<PlayerCharacterMovementController>();                
    }

    void Start()
    {
        if (mainCameraPos)
            originalPosition = mainCameraPos.localPosition;
    }

    
    void Update()
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
        if (mainCameraPos)
        {
            Vector3 camPos = Vector3.zero;
            camPos.y += Mathf.Lerp(camPos.y, Mathf.Sin(Time.time * Frequency) * Amount * 1.4f, Smooth * Time.deltaTime);
            camPos.x += Mathf.Lerp(camPos.x, Mathf.Cos(Time.time * Frequency / 2f) * Amount * 1.6f, Smooth * Time.deltaTime);
            mainCameraPos.localPosition = camPos;
        }

        if (armsHeadbob)
        {            
            Vector3 armsPos = Vector3.zero;
            armsPos.y += Mathf.Lerp(armsPos.y, Mathf.Sin(Time.time * Frequency) * Amount * 1.4f, Smooth * Time.deltaTime);
            armsPos.x += Mathf.Lerp(armsPos.x, Mathf.Cos(Time.time * Frequency / 2f) * Amount * 1.6f, Smooth * Time.deltaTime);
            armsHeadbob.localPosition = armsPos;
        }
    }

    private void StopHeadbob()
    {
        if (mainCameraPos)
        {
            if (mainCameraPos.localPosition == originalPosition) return;
            mainCameraPos.localPosition = Vector3.Slerp(mainCameraPos.localPosition, originalPosition, 1f * Time.deltaTime);
        }
        if (armsHeadbob) armsHeadbob.localPosition = Vector3.Slerp(armsHeadbob.localPosition, originalPosition, 1f * Time.deltaTime);
    }
}
