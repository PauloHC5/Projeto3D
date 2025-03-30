using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;

    [SerializeField] private Transform player;    
    private Transform playerMesh;    
    private Transform cameraPos;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private PlayerInputActions playerControls;

    private Vector2 MouseInput;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }
        
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        playerMesh = player.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.name.Equals("Mesh Root"));       
        cameraPos = player.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.name.Equals("CameraPos"));
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
            transform.position = cameraPos.position;
        }        
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
