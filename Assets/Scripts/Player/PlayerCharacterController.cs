using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerWeapon
{
    Crowbar = 0,
    ACornGun = 1,
    Shotgun = 2,
    Thompson = 3,
    Crossbow = 4
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
        playerCharacterAnimationsController = GetComponent<PlayerCharacterAnimationsController>();
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
        playerControls.Player.Weapon1.performed += ctx => SwitchToWeapon(PlayerWeapon.Crowbar);
        playerControls.Player.Weapon2.performed += ctx => SwitchToWeapon(PlayerWeapon.ACornGun);
        playerControls.Player.Weapon3.performed += ctx => SwitchToWeapon(PlayerWeapon.Shotgun);
        playerControls.Player.Weapon4.performed += ctx => SwitchToWeapon(PlayerWeapon.Thompson);
        playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(PlayerWeapon.Crossbow);

        // Assign the HandleMouseScroll method to the respective input actions
        playerControls.Player.MouseScrollUp.performed += ctx => { MouseScroll = -1; HandleMouseScroll(); };
        playerControls.Player.MouseScrollDown.performed += ctx => { MouseScroll = 1; HandleMouseScroll(); };        
    }        

    void Update()
    {
        HandleInput();
        playerCharacterMovementController.HandleMovement(playerMovementInput, playerLookInput);
        playerCharacterAnimationsController.HandleAmmo(playerCharacterCombatController.WeaponAmmo[playerCharacterCombatController.WeaponSelected]);        
    }

    private void HandleMouseScroll()
    {
        // Get the number of weapons in the PlayerWeapon enum
        int weaponCount = Enum.GetValues(typeof(PlayerWeapon)).Length;

        /* Calculate the new weapon index        
         * This will allow the player to cycle through the weapons
         * For example, if the player has the Crossbow equipped and scrolls down, the new weapon index will be 0 (Crowbar)
         * If the player has the Crowbar equipped and scrolls up, the new weapon index will be 4 (Crossbow) */        
        int newWeaponIndex = ((int)playerCharacterCombatController.WeaponSelected + MouseScroll + weaponCount) % weaponCount;
        
        SwitchToWeapon((PlayerWeapon)newWeaponIndex);
    }

    private void HandleInput()
    {
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && playerCharacterCombatController.WeaponSelected == PlayerWeapon.Shotgun) PerformSecondaryAction();

        //playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerMovementInput = playerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = playerControls.Player.Look.ReadValue<Vector2>();
    }

    private void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (lmbPressed || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || (weapon == playerCharacterCombatController.WeaponSelected && weapon != PlayerWeapon.Crowbar)) return;        

        playerCharacterCombatController.SwitchToWeapon(weapon);
    }           

    private void PerformPrimaryAction()
    {
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING) return;                

        playerCharacterCombatController.UseWeapon();
    }    

    private void PerformSecondaryAction()
    {
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING || playerCharacterCombatController.PlayerCombatStates ==  PlayerCombatStates.RELOADING) return;

        playerCharacterCombatController.UseWeaponGadget();
    }

    private void PerformReload()
    {
        if(playerCharacterCombatController.WeaponSelected == PlayerWeapon.Crowbar || playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING) return;
        
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
