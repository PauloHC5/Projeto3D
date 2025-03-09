using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;

    private Transform player;    
    private Transform playerMesh;    

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

        player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        playerMesh = player.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.name.Equals("PlayerMesh"));       
    }

    // Update is called once per frame
    void Update()
    {
        MouseInput = playerControls.Player.Look.ReadValue<Vector2>();        

        xRotation -= MouseInput.y * mouseSensitivity * Time.deltaTime;
        yRotation += MouseInput.x * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (player)
        {
            player.Rotate(Vector3.up * MouseInput.x * mouseSensitivity * Time.deltaTime);
            playerMesh.localRotation = Quaternion.Euler(xRotation + 5f, 0f, 0f);
            transform.position = player.position + (Vector3.up * 1.70f);
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
