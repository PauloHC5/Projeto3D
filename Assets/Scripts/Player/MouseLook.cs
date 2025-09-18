using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Properties")]
    [Range(1f, 50f)]
    [SerializeField] private float mouseSensitivity;    
    [SerializeField] private Transform cameraRot;
    [SerializeField] private LayerMask scopeLayerMask; // Layer mask for the scope view
    

    [Header("Player Mesh Properties")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerMeshPushAndPull;
    [SerializeField] private float xRotationDeltaPlayerMesh = 0f;    

    [Range(-2f, 2f)]
    [SerializeField] private float maxPullBack = -0.5f; // How far back to pull
    [SerializeField] private float pullSpeed = 5f;      // How fast to interpolate
    [Range(-2f, 2f)]
    [SerializeField] private float maxPushForward = 0.5f; // How far to push forward    

    private Vector3 playerMeshDefaultLocalPos;    

    private Camera[] playerCameras;
        
    private float defaultFoV;
    private bool zoomIn = false;
    private const float defaultZoomSpeed = 1000f;
    private LayerMask defaultCullingMask;

    private float xRotation = 0f;
    private float yRotation = 0f;    

    
    private Vector2 MouseInput;
    private Coroutine zoomCoroutine;        

    public bool ZoomIn => zoomIn;
        
    void Start()
    {        
        playerCameras = GetComponentsInChildren<Camera>();

        if (playerCameras[0] != null)
        {
            defaultFoV = playerCameras[0].fieldOfView;
        }
        playerMeshDefaultLocalPos = playerMeshPushAndPull.localPosition;
        
        defaultCullingMask = playerCameras[0].cullingMask; // Store the default culling mask
    }

    // Update is called once per frame
    void Update()
    {
        MouseInput = PlayerCharacterController.PlayerControls.Player.Look.ReadValue<Vector2>();        

        xRotation -= MouseInput.y * mouseSensitivity * Time.deltaTime;
        yRotation = MouseInput.x * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);        

        if (player)
        {
            UpdatePlayerMeshPushAndPull();
            player.Rotate(Vector3.up * yRotation);
            playerMeshPushAndPull.localRotation = Quaternion.Euler(xRotation + xRotationDeltaPlayerMesh, playerMeshPushAndPull.localRotation.y, playerMeshPushAndPull.localRotation.z);            
        }      
        
        mouseSensitivity = PauseManager.MouseSensitivitySlider.value; // Update the slider value in the pause menu
    }

    private void UpdatePlayerMeshPushAndPull()
    {
        float moveAmount = 0f;
        if (xRotation > 0f)
        {
            moveAmount = Mathf.InverseLerp(0f, 90f, xRotation) * maxPullBack;
        }
        else if (xRotation < 0f)
        {
            moveAmount = Mathf.InverseLerp(0f, -90f, xRotation) * maxPushForward;
        }

        // Move along the camera's forward direction, relative to the mesh's parent
        Vector3 cameraForwardLocal = playerMeshPushAndPull.parent.InverseTransformDirection(playerCameras[0].transform.forward);
        Vector3 targetLocalPos = playerMeshDefaultLocalPos + cameraForwardLocal * moveAmount;

        // Smoothly interpolate to the target position
        playerMeshPushAndPull.localPosition = Vector3.Lerp(
            playerMeshPushAndPull.localPosition,
            targetLocalPos,
            Time.deltaTime * pullSpeed
        );
    }

    private void PerformAim(float zoomFoV, float zoomSpeed)
    {
        zoomIn = !zoomIn;

        if(zoomIn)
        {
            playerCameras[1].enabled = false;     
            playerCameras[0].cullingMask = scopeLayerMask; // Change to scope layer mask
        }
        else
        {
            playerCameras[1].enabled = true;
            playerCameras[0].cullingMask = defaultCullingMask; // Revert to default culling mask
        }

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(Zoom(zoomFoV, zoomSpeed));
    }

    private IEnumerator Zoom(float zoomFoV, float zoomSpeed)
    {        
        float elapsedTime = 0;
        float startFoV = playerCameras[0].fieldOfView;
        float targetFoV = zoomIn ? zoomFoV : defaultFoV;
        float localscopeSpeed = zoomIn ? zoomSpeed : zoomSpeed * 3;        

        while (Mathf.Abs(playerCameras[0].fieldOfView - targetFoV) > 0.01f)
        {
            foreach (Camera playerCamera in playerCameras)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFoV, targetFoV, localscopeSpeed * (elapsedTime / zoomSpeed));
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Camera playerCamera in playerCameras)
        {
            playerCamera.fieldOfView = targetFoV;
        }
    }

    public void ZoomOut()
    {
        if (zoomIn == false) return;

        zoomIn = false;
        playerCameras[1].enabled = true;
        playerCameras[0].cullingMask = defaultCullingMask; // Revert to default culling mask

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(Zoom(defaultFoV, defaultZoomSpeed));
    }

    private void OnEnable()
    {        
        CactusCrossbow.AimEvent += PerformAim;              
        PlayerCharacterCombatController.OnSwitchToWeapon += ZoomOut;
        PlayerCharacterCombatController.OnPerformReload += ZoomOut;
    }

    private void OnDisable()
    {        
        CactusCrossbow.AimEvent -= PerformAim;
        PlayerCharacterCombatController.OnSwitchToWeapon -= ZoomOut;
        PlayerCharacterCombatController.OnPerformReload -= ZoomOut;
        
    }
}
