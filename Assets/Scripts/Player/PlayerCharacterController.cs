using System;
using System.Collections;
using System.Security.Cryptography;
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

    private int MouseScroll;
    private PlayerInputActions playerControls;
    private Vector2 playerMovementInput;
    private Vector2 playerLookInput;

    private PlayerCharacterMovementController playerCharacterMovementController;
    private PlayerCharacterCombatController playerCharacterCombatController;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;

    private void Awake()
    {
        InitializePlayerControls();                

        playerCharacterMovementController = GetComponent<PlayerCharacterMovementController>();
        playerCharacterCombatController = GetComponent<PlayerCharacterCombatController>();
        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
    }

    private void InitializePlayerControls()
    {
        playerControls = new PlayerInputActions();

        playerControls.Player.PrimaryAction.started += ctx => lmbPressed = true;
        playerControls.Player.PrimaryAction.canceled += ctx => lmbPressed = false;

        playerControls.Player.SecondaryAction.started += ctx => rmbPressed = true;
        playerControls.Player.SecondaryAction.canceled += ctx => rmbPressed = false;

        playerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        playerControls.Player.Jump.performed += ctx => PerformJump();
        playerControls.Player.Reload.performed += ctx => PerformReload();
        playerControls.Player.Crouch.performed += ctx => Crouch();

        // Assign the SwitchToWeapon method to the respective input action
        playerControls.Player.Weapon1.performed += ctx => SwitchToWeapon(0);
        playerControls.Player.Weapon2.performed += ctx => SwitchToWeapon(1);
        playerControls.Player.Weapon3.performed += ctx => SwitchToWeapon(2);
        playerControls.Player.Weapon4.performed += ctx => SwitchToWeapon(3);
        //playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(4);

        // Assign the HandleMouseScroll method to the respective input actions
        playerControls.Player.MouseScrollUp.performed += ctx => { MouseScroll = 1; HandleMouseScroll(); };
        playerControls.Player.MouseScrollDown.performed += ctx => { MouseScroll = -1; HandleMouseScroll(); };        
    }        

    void Update()
    {
        HandleInput();
        playerCharacterMovementController.HandleMovement(playerMovementInput, playerLookInput);
        playerCharacterAnimationsController.HandleAmmo(playerCharacterCombatController.WeaponAmmo[playerCharacterCombatController.WeaponSelected]);        
    }

    private void HandleMouseScroll()
    {
        if (lmbPressed || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING)
            return;

        // Get the current weapon index
        int currentIndex;
        switch(playerCharacterCombatController.WeaponSelected)
        {
            case WeaponTypes.Melee:
                currentIndex = 0;
                break;
            case WeaponTypes.Pistol:
                currentIndex = 1;
                break;
            case WeaponTypes.Shotgun:
                currentIndex = 2;
                break;            
            case WeaponTypes.Crossbow:
                currentIndex = 3;
                break;
            default:
                currentIndex = -1; // Invalid index
                break;
        }

        int inventoryCount = playerCharacterCombatController.WeaponsInventoryCount;

        if (inventoryCount <= 1)
            return; // No need to scroll if only one weapon

        // Calculate new index based on scroll direction
        int newIndex = currentIndex + MouseScroll;

        // Wrap around
        if (newIndex < 0)
            newIndex = 3;
        else if (newIndex >= inventoryCount)
            newIndex = 0;

        Debug.Log($"Switching to weapon index: {newIndex}");

        // Switch weapon
        SwitchToWeapon(newIndex);
    }

    private void HandleInput()
    {
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && playerCharacterCombatController.WeaponSelected == WeaponTypes.Shotgun) PerformSecondaryAction();

        //playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerMovementInput = playerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = playerControls.Player.Look.ReadValue<Vector2>();
    }

    private void SwitchToWeapon(int weaponsIndex)
    {        
        if (lmbPressed || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING) return;        

        playerCharacterCombatController.SwitchToWeapon(weaponsIndex);
    }           

    private void PerformPrimaryAction()
    {
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING) return;                

        playerCharacterCombatController.UseWeapon();
    }    

    private void PerformSecondaryAction()
    {
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING || playerCharacterCombatController.PlayerCombatStates ==  PlayerCombatStates.RELOADING) return;

        playerCharacterCombatController.UseWeaponGadget();
    }

    private void PerformReload()
    {
        if(playerCharacterCombatController.WeaponSelected == WeaponTypes.Melee || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING) return;
        
        playerCharacterCombatController.Reload();
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
        playerControls.Enable();        
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

}
