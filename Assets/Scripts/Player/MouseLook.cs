using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Properties")]
    [SerializeField] private float mouseSensitivity = 100f;    
    [SerializeField] private Transform cameraRot;
    [SerializeField] private GameObject scopeVolume;
    

    [Header("Player Mesh Properties")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerMesh;
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

    private float xRotation = 0f;
    private float yRotation = 0f;    

    private PlayerInputActions playerControls;
    private Vector2 MouseInput;
    private Coroutine zoomCoroutine;        

    public bool ZoomIn => zoomIn;

    private void Awake()
    {
        playerControls = new PlayerInputActions();        
    }
        
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerCameras = GetComponentsInChildren<Camera>();

        if (playerCameras[0] != null)
        {
            defaultFoV = playerCameras[0].fieldOfView;
        }
        playerMeshDefaultLocalPos = playerMesh.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        MouseInput = playerControls.Player.Look.ReadValue<Vector2>();        

        xRotation -= MouseInput.y * mouseSensitivity * Time.deltaTime;
        yRotation = MouseInput.x * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);        

        if (player)
        {
            UpdatePlayerMeshPushAndPull();
            player.Rotate(Vector3.up * yRotation);
            playerMesh.localRotation = Quaternion.Euler(xRotation + xRotationDeltaPlayerMesh, playerMesh.localRotation.y, playerMesh.localRotation.z);            
        }        
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
        Vector3 cameraForwardLocal = playerMesh.parent.InverseTransformDirection(playerCameras[0].transform.forward);
        Vector3 targetLocalPos = playerMeshDefaultLocalPos + cameraForwardLocal * moveAmount;

        // Smoothly interpolate to the target position
        playerMesh.localPosition = Vector3.Lerp(
            playerMesh.localPosition,
            targetLocalPos,
            Time.deltaTime * pullSpeed
        );
    }

    private void PerformAim(float zoomFoV, float zoomSpeed)
    {
        zoomIn = !zoomIn;

        if(zoomIn)
        {
            scopeVolume.SetActive(true);            
            playerCameras[1].enabled = false;
            GameManager.Instance.Hud.ScopeEvent(true);
        }
        else
        {
            scopeVolume.SetActive(false);            
            playerCameras[1].enabled = true;
            GameManager.Instance.Hud.ScopeEvent(false);
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
        scopeVolume.SetActive(false);
        GameManager.Instance.Hud.ScopeEvent(false);
        playerCameras[1].enabled = true;

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(Zoom(defaultFoV, defaultZoomSpeed));
    }

    private void OnEnable()
    {
        playerControls.Enable();
        CactusCrossbow.AimEvent += PerformAim;              
        PlayerCharacterCombatController.onSwitchToWeapon += ZoomOut;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        CactusCrossbow.AimEvent -= PerformAim;
        AnimationTriggerEvents.onReload -= ZoomOut;
        PlayerCharacterCombatController.onSwitchToWeapon -= ZoomOut;
    }
}
