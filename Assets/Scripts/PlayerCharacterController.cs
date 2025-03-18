using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerWeapon
{
    Crowbar = 0,
    Flaregun = 1,
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

public class PlayerCharacterController : PlayerCharacterCombatController
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

        playerControls.Player.Reload.performed += ctx => Reload();
        playerControls.Player.Weapon1.performed += ctx => SwitchToWeapon(PlayerWeapon.Crowbar);
        playerControls.Player.Weapon2.performed += ctx => SwitchToWeapon(PlayerWeapon.Flaregun);
        playerControls.Player.Weapon3.performed += ctx => SwitchToWeapon(PlayerWeapon.Shotgun);
        playerControls.Player.Weapon4.performed += ctx => SwitchToWeapon(PlayerWeapon.Thompson);
        playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(PlayerWeapon.Crossbow);

        // faça com que Mouse Scrool seja 1 quandi o playerControls.Player.MouseScroll for para cima e -1 quando for para baixo
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
        CompositePoitionRotation();
        HandleJump();
        ApplyGravity();
        HandleAmmo(playerWeaponAmmo[weaponSelected]);

        // if k button is pressed, add 3 to playerWeaponAmmo[weaponSelected]
        if (Keyboard.current.kKey.wasPressedThisFrame) playerWeaponAmmo[weaponSelected] += 3;

        // if l button is pressed, debug log the playerWeaponAmmo[weaponSelected]
        if (Keyboard.current.lKey.wasPressedThisFrame) Debug.Log($"{weaponSelected} ammo: " + playerWeaponAmmo[weaponSelected]);

        // if j button is pressed, debug log the equippedWeapon.MagAmmo
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            Gun equippedGun = equippedWeapon as Gun;
            Debug.Log($"{weaponSelected} MagAmmo: " + equippedGun.MagAmmo);
        }
    }

    private void HandleMouseScroll()
    {
        int weaponCount = Enum.GetValues(typeof(PlayerWeapon)).Length;
        int newWeaponIndex = ((int)weaponSelected + MouseScroll + weaponCount) % weaponCount;
        SwitchToWeapon((PlayerWeapon)newWeaponIndex);
    }

    private void HandleInput()
    {
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && weaponSelected == PlayerWeapon.Shotgun) PerformSecondaryAction();

        playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerLookInput = playerControls.Player.Look.ReadValue<Vector2>();
    }

    protected override void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (playerStates == PlayerStates.FIRING || playerStates == PlayerStates.ATTACKING || (weapon == weaponSelected && weapon != PlayerWeapon.Crowbar)) return;        

        base.SwitchToWeapon(weapon);
    }           

    protected void PerformPrimaryAction()
    {
        if (playerStates == PlayerStates.RELOADING || playerStates == PlayerStates.ATTACKING || playerStates == PlayerStates.RAISING) return;                

        UseWeapon();
    }    

    protected void PerformSecondaryAction()
    {
        if (playerStates == PlayerStates.RAISING || playerStates ==  PlayerStates.RELOADING) return;

        UseWeaponGadget();
    }

    protected override void Reload()
    {
        if(weaponSelected == PlayerWeapon.Crowbar) return;
        base.Reload();
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
