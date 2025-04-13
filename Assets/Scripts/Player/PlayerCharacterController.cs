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

public enum WeaponSocket
{
    RightHand,
    LeftHand,
    Back
}

public class PlayerCharacterController : PlayerCharacterAnimationsController
{                             
    protected bool lmbPressed = false;
    protected bool rmbPressed = false;                       

    public bool LmbPressed { get { return lmbPressed; } }
    public bool RmbPressed { get { return rmbPressed; } }    

    private int MouseScroll;
    private PlayerInputActions playerControls;
    private Vector2 playerMovementInput;
    private Vector2 playerLookInput;

    private new void Awake()
    {
        InitializePlayerControls();
        IntializeMovement();
        base.Awake();
    }

    private void InitializePlayerControls()
    {
        playerControls = new PlayerInputActions();

        playerControls.Player.PrimaryAction.started += ctx => lmbPressed = true;
        playerControls.Player.PrimaryAction.canceled += ctx => lmbPressed = false;

        playerControls.Player.SecondaryAction.started += ctx => rmbPressed = true;
        playerControls.Player.SecondaryAction.canceled += ctx => rmbPressed = false;

        playerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        playerControls.Player.Jump.performed += ctx => Jump();
        playerControls.Player.Reload.performed += ctx => Reload();
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

    private void Start()
    {
        SwitchToWeapon(weaponSelected);        
    }

    void Update()
    {
        HandleInput();
        HandleMovement(playerMovementInput, playerLookInput);
        HandleLocomotion();
        ApplyGravity();
        HandleAmmo(playerWeaponAmmo[weaponSelected]);

        // if k button is pressed, add 3 to playerWeaponAmmo[weaponSelected]
        if (Keyboard.current.kKey.wasPressedThisFrame) playerWeaponAmmo[weaponSelected] += 3;        
    }

    private void HandleMouseScroll()
    {
        // Get the number of weapons in the PlayerWeapon enum
        int weaponCount = Enum.GetValues(typeof(PlayerWeapon)).Length;

        /* Calculate the new weapon index        
         * This will allow the player to cycle through the weapons
         * For example, if the player has the Crossbow equipped and scrolls down, the new weapon index will be 0 (Crowbar)
         * If the player has the Crowbar equipped and scrolls up, the new weapon index will be 4 (Crossbow) */        
        int newWeaponIndex = ((int)weaponSelected + MouseScroll + weaponCount) % weaponCount;
        
        SwitchToWeapon((PlayerWeapon)newWeaponIndex);
    }

    private void HandleInput()
    {
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && weaponSelected == PlayerWeapon.Shotgun) PerformSecondaryAction();

        //playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerMovementInput = playerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = playerControls.Player.Look.ReadValue<Vector2>();
    }

    protected override void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (lmbPressed || playerCombatStates == PlayerCombatStates.ATTACKING || (weapon == weaponSelected && weapon != PlayerWeapon.Crowbar)) return;        

        base.SwitchToWeapon(weapon);
    }           

    protected void PerformPrimaryAction()
    {
        if (playerCombatStates == PlayerCombatStates.RELOADING || playerCombatStates == PlayerCombatStates.ATTACKING || playerCombatStates == PlayerCombatStates.RAISING) return;                

        UseWeapon();
    }    

    protected void PerformSecondaryAction()
    {
        if (playerCombatStates == PlayerCombatStates.RAISING || playerCombatStates ==  PlayerCombatStates.RELOADING) return;

        UseWeaponGadget();
    }

    protected override void Reload()
    {
        if(weaponSelected == PlayerWeapon.Crowbar || playerCombatStates == PlayerCombatStates.RELOADING) return;
        base.Reload();
    }

    protected override void Jump()
    {        
        base.Jump();
    }

    protected override void Crouch()
    {
        base.Crouch();
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
