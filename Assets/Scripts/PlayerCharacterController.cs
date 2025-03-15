using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerWeapon
{
    CROWBAR = 0,
    PISTOL = 1,
    SHOTGUNS = 2,
    THOMPSOM = 3,
    CROSSBOW = 4
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
                    

    public PlayerStates PlayerStates { 
        get { return playerStates; }
        set { playerStates = value; }
    }       

    public bool LmbPressed { get { return lmbPressed; } }
    public bool RmbPressed { get { return rmbPressed; } }    

    private int MouseScroll;
    private PlayerInputActions playerControls;
    private Vector2 playerMovementInput;

    private void Awake()
    {
        InitializePlayerControls();             
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
        playerControls.Player.Weapon1.performed += ctx => SwitchToWeapon(PlayerWeapon.CROWBAR);
        playerControls.Player.Weapon2.performed += ctx => SwitchToWeapon(PlayerWeapon.PISTOL);
        playerControls.Player.Weapon3.performed += ctx => SwitchToWeapon(PlayerWeapon.SHOTGUNS);
        playerControls.Player.Weapon4.performed += ctx => SwitchToWeapon(PlayerWeapon.THOMPSOM);
        playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(PlayerWeapon.CROSSBOW);

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
        HandleMovement(playerMovementInput);
        HandleJump();
        ApplyGravity();        
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
        if (rmbPressed && weaponSelected == PlayerWeapon.SHOTGUNS) PerformSecondaryAction();

        playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    protected override void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (playerStates == PlayerStates.FIRING || playerStates == PlayerStates.ATTACKING || (weapon == weaponSelected && weapon != PlayerWeapon.CROWBAR)) return;        

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
