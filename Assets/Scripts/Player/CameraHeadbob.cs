using System;
using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    [Range(0.001f, 1f)]
    [SerializeField] private float Amount = 0.002f;

    [Range(1f, 30f)]
    [SerializeField] private float Frequency = 10f;

    [Range(10f, 100f)]
    [SerializeField] private float Smooth = 10f;

    [SerializeField] private Transform armsHeadbob;

    private PlayerCharacterController playerCharacterController;    
    private Vector3 originalPosition;
    private float originalFrequency;

    private void Awake()
    {
        playerCharacterController = GetComponentInParent<PlayerCharacterController>();
        originalFrequency = Frequency;
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    
    void Update()
    {
        CheckForHeadbobTrigger();

        if (playerCharacterController.PlayerMovementStates == PlayerMovementStates.CROUCH || playerCharacterController.PlayerMovementStates == PlayerMovementStates.CROUCHING)
            Frequency /= 2f;
        else Frequency = originalFrequency;
    }    

    private void CheckForHeadbobTrigger()
    {
        float inputMagnitude = playerCharacterController.PlayerVelocityMagnitude;        

        if (inputMagnitude > 0f)
        {
            StartHeadbob();            
        }
        else StopHeadbob();
    }

    private void StartHeadbob()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * Frequency) * Amount * 1.4f, Smooth * Time.deltaTime);
        pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * Frequency / 2f) * Amount * 1.6f, Smooth * Time.deltaTime);

        transform.transform.localPosition = pos;      
        if(armsHeadbob) armsHeadbob.localPosition = pos;
    }

    private void StopHeadbob()
    {
        if (transform.localPosition == originalPosition) return;

        transform.localPosition = Vector3.Slerp(transform.localPosition, originalPosition, 1f * Time.deltaTime);
        if (armsHeadbob) armsHeadbob.localPosition = Vector3.Slerp(armsHeadbob.localPosition, originalPosition, 1f * Time.deltaTime);
    }
}
