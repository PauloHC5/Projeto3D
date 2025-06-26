using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerCharacterController : MonoBehaviour
{                             
    public static bool PrimaryActionButtonPressed = false;
    public static bool SecondaryActionButtonPressed = false;                           

    public static PlayerInputActions PlayerControls;

    private int MouseScroll;    
    private Vector2 playerMovementInput;
    private Vector2 playerLookInput;

    private PlayerCharacterMovementController playerCharacterMovementController;
    private PlayerCharacterCombatController playerCharacterCombatController;           

private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;                        

        playerCharacterMovementController = GetComponent<PlayerCharacterMovementController>();
        playerCharacterCombatController = GetComponent<PlayerCharacterCombatController>();

        InitializePlayerControls();
    }

    private void InitializePlayerControls()
    {
        PlayerControls = new PlayerInputActions();

        PlayerControls.Player.PrimaryAction.started += ctx => PrimaryActionButtonPressed = true;
        PlayerControls.Player.PrimaryAction.canceled += ctx => PrimaryActionButtonPressed = false;

        PlayerControls.Player.SecondaryAction.started += ctx => SecondaryActionButtonPressed = true;
        PlayerControls.Player.SecondaryAction.canceled += ctx => SecondaryActionButtonPressed = false;

        PlayerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        PlayerControls.Player.Jump.performed += ctx => PerformJump();
        PlayerControls.Player.Reload.performed += ctx => PerformReload();
        PlayerControls.Player.Crouch.performed += ctx => Crouch();

        PlayerControls.Player.Pause.performed += ctx =>
        {            
            GameManager.PauseGame();            
        };

        PlayerControls.UI.Unpause.performed += ctx =>
        {
            if (GameManager.IsPaused)
            {
                GameManager.ResumeGame();
            }
        };

        // Assign the SwitchToWeapon method to the respective input action        
        PlayerControls.Player.Weapon1.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Melee);
        PlayerControls.Player.Weapon2.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Pistol);
        PlayerControls.Player.Weapon3.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Shotgun);
        PlayerControls.Player.Weapon4.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Crossbow);
                        

        // Assign the HandleMouseScroll method to the respective input actions
        PlayerControls.Player.MouseScrollUp.performed += ctx => { MouseScroll = 1; HandleMouseScroll(); };
        PlayerControls.Player.MouseScrollDown.performed += ctx => { MouseScroll = -1; HandleMouseScroll(); };        
    }        

    void Update()
    {
        HandleInput();
        playerCharacterMovementController.HandleMovement(playerMovementInput, playerLookInput);                
    }

    private void HandleMouseScroll()
    {          
        int inventoryCount = playerCharacterCombatController.WeaponsInventoryCount;

        if (inventoryCount <= 1)
            return; // No need to scroll if only one weapon

        // Get the current weapon index
        int currentIndex = (int)playerCharacterCombatController.WeaponSelected;

        // Calculate new index based on scroll direction
        int newIndex = currentIndex + MouseScroll;

        // Wrap around
        if (newIndex < 0)
            newIndex = 3;
        else if (newIndex >= inventoryCount)
            newIndex = 0;

        currentIndex = newIndex;

        // Switch weapon
        switch (currentIndex)
        {
            case 0:
                playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Melee);
                break;
            case 1:
                playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Pistol);
                break;
            case 2:
                playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Shotgun);
                break;
            case 3:
                playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Crossbow);
                break;
        }
    }

    private void HandleInput()
    {
        if (PrimaryActionButtonPressed) PerformPrimaryAction();
        //if (rmbPressed) PerformSecondaryAction();

        playerCharacterCombatController?.ChargeWeapon(SecondaryActionButtonPressed);
        
        playerMovementInput = PlayerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = PlayerControls.Player.Look.ReadValue<Vector2>();
    }           

    private void PerformPrimaryAction()
    {
        if(playerCharacterCombatController)
        {
            if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING) return;

            playerCharacterCombatController.PerformPrimaryAction();
        }        
    }    

    private void PerformSecondaryAction()
    {        
        if(playerCharacterCombatController)
        {
            playerCharacterCombatController.PerformSecondaryAction();
        }        
    }

    private void PerformReload()
    {
        playerCharacterCombatController.PerformReload();
    }

    private void PerformJump()
    {
        playerCharacterMovementController.Jump();
    }

    private void Crouch()
    {
        playerCharacterMovementController.Crouch();
    }

    private void OnEnable()
    {
        PlayerControls.Enable();        
    }

    private void OnDisable()
    {
        PlayerControls.Disable();
    }

}
