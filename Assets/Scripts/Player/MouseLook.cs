using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerMesh;
    [SerializeField] private Transform cameraRot;    

    private Camera[] playerCameras;
        
    private float defaultFoV;
    private bool zoomIn = false;
    private const float defaultZoomSpeed = 1000f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private PlayerInputActions playerControls;
    private Vector2 MouseInput;
    private Coroutine zoomCoroutine;    

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
    }

    // Update is called once per frame
    void Update()
    {
        MouseInput = playerControls.Player.Look.ReadValue<Vector2>();        

        xRotation -= MouseInput.y * mouseSensitivity * Time.deltaTime;
        yRotation += MouseInput.x * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (player)
        {
            player.Rotate(Vector3.up * MouseInput.x * mouseSensitivity * Time.deltaTime);
            playerMesh.localRotation = Quaternion.Euler(xRotation + 5f, playerMesh.localRotation.y, playerMesh.localRotation.z);
            //transform.position = cameraRot.position;
        }        
    }

    private void PerformAim(float zoomFoV, float zoomSpeed)
    {
        zoomIn = !zoomIn;

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

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(Zoom(defaultFoV, defaultZoomSpeed));
    }

    private void OnEnable()
    {
        playerControls.Enable();
        Crossbow.AimEvent += PerformAim;      
        PlayerCharacterCombatController.onReload += ZoomOut;
        PlayerCharacterCombatController.onSwitchToWeapon += ZoomOut;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        Crossbow.AimEvent -= PerformAim;
        PlayerCharacterCombatController.onReload -= ZoomOut;
        PlayerCharacterCombatController.onSwitchToWeapon -= ZoomOut;
    }
}
