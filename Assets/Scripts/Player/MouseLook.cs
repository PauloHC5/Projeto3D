using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Properties")]
    [Range(1f, 50f)]
    [SerializeField] private float defaultMouseSensitivity;
    [Range(1f, 5f)]
    [SerializeField] private float mouseScopeSensitivity;
    [Range(1f, 90f)]
    [SerializeField] private float maxRotationDown = 90f; // Maximum up/down rotation angle
    [Range(-90f, 0f)]
    [SerializeField] private float maxRotationUp = -90f; // Minimum up/down rotation angle
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

    private Vector3 _playerMeshDefaultLocalPos;    

    private Camera[] _playerCameras;
        
    private float _defaultFoV;
    private bool _zoomIn = false;
    private float _defaultZoomSpeed = 1f;
    private LayerMask _defaultCullingMask;
    private float _currentMouseSensitivity;
    private PlayerCharacter _playerCharacter;
    private PlayerCharacterController _playerCharacterController;

    private float _xRotation = 0f;
    private float _yRotation = 0f;    

    
    private Vector2 _mouseInput;
    private Coroutine _zoomCoroutine;        

    public bool ZoomIn => _zoomIn;

    private void Awake()
    {
        _playerCharacter = GetComponentInParent<PlayerCharacter>();
        _playerCharacterController = GetComponentInParent<PlayerCharacterController>();
    }

    void Start()
    {        
        _playerCameras = GetComponentsInChildren<Camera>();

        if (_playerCameras[0] != null)
        {
            _defaultFoV = _playerCameras[0].fieldOfView;
        }
        _playerMeshDefaultLocalPos = playerMeshPushAndPull.localPosition;
        
        _defaultCullingMask = _playerCameras[0].cullingMask; // Store the default culling mask

        _currentMouseSensitivity = defaultMouseSensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        _mouseInput = _playerCharacterController.PlayerControls.Player.Look.ReadValue<Vector2>();        

        _xRotation -= _mouseInput.y * _currentMouseSensitivity * Time.deltaTime;
        _yRotation = _mouseInput.x * _currentMouseSensitivity * Time.deltaTime;

        _xRotation = Mathf.Clamp(_xRotation, maxRotationUp, maxRotationDown);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);        

        if (player)
        {
            UpdatePlayerMeshPushAndPull();
            player.Rotate(Vector3.up * _yRotation);
            playerMeshPushAndPull.localRotation = Quaternion.Euler(_xRotation + xRotationDeltaPlayerMesh, playerMeshPushAndPull.localRotation.y, playerMeshPushAndPull.localRotation.z);            
        }      
        
        defaultMouseSensitivity = PauseManager.MouseSensitivitySlider.value; // Update the slider value in the pause menu
        if(!_zoomIn) _currentMouseSensitivity = PauseManager.MouseSensitivitySlider.value; // Update the slider value in the pause menu
    }

    private void UpdatePlayerMeshPushAndPull()
    {
        float moveAmount = 0f;
        if (_xRotation > 0f)
        {
            moveAmount = Mathf.InverseLerp(0f, maxRotationDown, _xRotation) * maxPullBack;
        }
        else if (_xRotation < 0f)
        {
            moveAmount = Mathf.InverseLerp(0f, maxRotationUp, _xRotation) * maxPushForward;
        }

        // Move along the camera's forward direction, relative to the mesh's parent
        Vector3 cameraForwardLocal = playerMeshPushAndPull.parent.InverseTransformDirection(_playerCameras[0].transform.forward);
        Vector3 targetLocalPos = _playerMeshDefaultLocalPos + cameraForwardLocal * moveAmount;

        // Smoothly interpolate to the target position
        playerMeshPushAndPull.localPosition = Vector3.Lerp(
            playerMeshPushAndPull.localPosition,
            targetLocalPos,
            Time.deltaTime * pullSpeed
        );
    }

    private void PerformAim(float zoomFoV, float zoomSpeed)
    {
        _zoomIn = !_zoomIn;
        _defaultZoomSpeed = zoomSpeed;

        if(_zoomIn)
        {
            _playerCameras[1].enabled = false;     
            _playerCameras[0].cullingMask = scopeLayerMask; // Change to scope layer mask
            _currentMouseSensitivity = mouseScopeSensitivity; // Change to scope sensitivity
        }
        else
        {
            _playerCameras[1].enabled = true;
            _playerCameras[0].cullingMask = _defaultCullingMask; // Revert to default culling mask
            _currentMouseSensitivity = defaultMouseSensitivity; // Revert to default sensitivity
        }

        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }
        _zoomCoroutine = StartCoroutine(Zoom(zoomFoV, zoomSpeed));
    }

    private IEnumerator Zoom(float zoomFoV, float zoomSpeed)
    {        
        float elapsedTime = 0;
        float startFoV = _playerCameras[0].fieldOfView;
        float targetFoV = _zoomIn ? zoomFoV : _defaultFoV;

        while (Mathf.Abs(_playerCameras[0].fieldOfView - targetFoV) > 0.01f)
        {
            foreach (Camera playerCamera in _playerCameras)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFoV, targetFoV, elapsedTime / zoomSpeed);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Camera playerCamera in _playerCameras)
        {
            playerCamera.fieldOfView = targetFoV;
        }
    }

    public void ZoomOut()
    {
        if (_zoomIn == false) return;

        _zoomIn = false;
        _playerCameras[1].enabled = true;
        _playerCameras[0].cullingMask = _defaultCullingMask; // Revert to default culling mask

        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }
        _zoomCoroutine = StartCoroutine(Zoom(_defaultFoV, _defaultZoomSpeed));
    }

    private void OnEnable()
    {        
        CactusCrossbow.AimEvent += PerformAim;              
        PlayerCharacterCombatController.OnSwitchToWeapon += ZoomOut;
        PlayerCharacterCombatController.OnPerformReload += ZoomOut;
        if (_playerCharacter)
        {
            _playerCharacter.OnDeath += ZoomOut;
            _playerCharacter.OnDeath += () => { enabled = false; };
        }
            
        
    }

    private void OnDisable()
    {        
        CactusCrossbow.AimEvent -= PerformAim;
        PlayerCharacterCombatController.OnSwitchToWeapon -= ZoomOut;
        PlayerCharacterCombatController.OnPerformReload -= ZoomOut;
        if (_playerCharacter)
        {
            _playerCharacter.OnDeath -= ZoomOut;
            _playerCharacter.OnDeath -= () => { enabled = false; };
        }
        
    }
}
