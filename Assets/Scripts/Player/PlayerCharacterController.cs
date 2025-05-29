using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponTypes
{
    Melee = 0,
    Pistol = 1,
    Shotgun = 2,
    Crossbow = 3,
    Smg = 4,
}

public class PlayerCharacterController : MonoBehaviour
{                             
    protected bool lmbPressed = false;
    protected bool rmbPressed = false;                       

    public bool LmbPressed { get { return lmbPressed; } }
    public bool RmbPressed { get { return rmbPressed; } }

    public static PlayerInputActions PlayerControls;

    private int MouseScroll;    
    private Vector2 playerMovementInput;
    private Vector2 playerLookInput;

    private PlayerCharacterMovementController playerCharacterMovementController;
    private PlayerCharacterCombatController playerCharacterCombatController;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;

    private bool ConditionToSwitchWeapon() => !lmbPressed && playerCharacterCombatController?.PlayerCombatStates != PlayerCombatStates.ATTACKING;   

private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        InitializePlayerControls();                

        playerCharacterMovementController = GetComponent<PlayerCharacterMovementController>();
        playerCharacterCombatController = GetComponent<PlayerCharacterCombatController>();
        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
    }

    private void InitializePlayerControls()
    {
        PlayerControls = new PlayerInputActions();

        PlayerControls.Player.PrimaryAction.started += ctx => lmbPressed = true;
        PlayerControls.Player.PrimaryAction.canceled += ctx => lmbPressed = false;

        PlayerControls.Player.SecondaryAction.started += ctx => rmbPressed = true;
        PlayerControls.Player.SecondaryAction.canceled += ctx => rmbPressed = false;

        PlayerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        PlayerControls.Player.Jump.performed += ctx => PerformJump();
        PlayerControls.Player.Reload.performed += ctx => PerformReload();
        PlayerControls.Player.Crouch.performed += ctx => Crouch();

        PlayerControls.Player.Pause.performed += ctx =>
        {            
            PauseManager.Instance.PauseGame();            
        };

        PlayerControls.UI.Unpause.performed += ctx =>
        {
            if (PauseManager.Instance.IsPaused)
            {
                PauseManager.Instance.ResumeGame();
            }
        };

        // Assign the SwitchToWeapon method to the respective input action
        if (ConditionToSwitchWeapon())
        {
            PlayerControls.Player.Weapon1.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Melee);
            PlayerControls.Player.Weapon2.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Pistol);
            PlayerControls.Player.Weapon3.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Shotgun);
            PlayerControls.Player.Weapon4.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Crossbow);
        }        
        //playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(4);

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
        if (lmbPressed || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING)
            return;        

        int inventoryCount = playerCharacterCombatController.WeaponsInventoryCount;

        if (inventoryCount <= 1)
            return; // No need to scroll if only one weapon

        // Get the current weapon index
        int currentIndex = 0;

        // Calculate new index based on scroll direction
        int newIndex = currentIndex + MouseScroll;

        // Wrap around
        if (newIndex < 0)
            newIndex = 3;
        else if (newIndex >= inventoryCount)
            newIndex = 0;

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
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && playerCharacterCombatController.WeaponSelected == WeaponTypes.Shotgun) PerformSecondaryAction();
        
        playerMovementInput = PlayerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = PlayerControls.Player.Look.ReadValue<Vector2>();
    }           

    private void PerformPrimaryAction()
    {
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING) return;                

        playerCharacterCombatController.PerformPrimaryAction();
    }    

    private void PerformSecondaryAction()
    {        
        playerCharacterCombatController.PerformSecondaryAction();
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
