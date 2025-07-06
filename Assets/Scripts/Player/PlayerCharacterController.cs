using System.Linq;
using UnityEngine;

public enum PlayerControlTypes
{
    GAMEPLAY,
    UI,
    CUTSCENE
}

public class PlayerCharacterController : MonoBehaviour
{                             
    public static bool PrimaryActionButtonPressed = false;
    public static bool SecondaryActionButtonPressed = false;                           

    private static PlayerInputActions _playerControls;
    public static PlayerInputActions PlayerControls => _playerControls;

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
        _playerControls = new PlayerInputActions();

        _playerControls.Player.PrimaryAction.started += ctx => PrimaryActionButtonPressed = true;
        _playerControls.Player.PrimaryAction.canceled += ctx => PrimaryActionButtonPressed = false;

        _playerControls.Player.SecondaryAction.started += ctx => SecondaryActionButtonPressed = true;
        _playerControls.Player.SecondaryAction.canceled += ctx => SecondaryActionButtonPressed = false;

        _playerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        _playerControls.Player.Jump.performed += ctx => PerformJump();
        _playerControls.Player.Reload.performed += ctx => PerformReload();
        _playerControls.Player.Crouch.performed += ctx => Crouch();

        _playerControls.Player.Pause.performed += ctx =>
        {            
            GameManager.PauseGame();            
        };

        _playerControls.UI.Unpause.performed += ctx =>
        {
            if (GameManager.IsPaused)
            {
                GameManager.ResumeGame();
            }
        };
    
        // Assign the SwitchToWeapon method to the respective input action        
        _playerControls.Player.Weapon1.performed += ctx => playerCharacterCombatController.SwitchToWeapon(0);
        _playerControls.Player.Weapon2.performed += ctx => playerCharacterCombatController.SwitchToWeapon(1);
        _playerControls.Player.Weapon3.performed += ctx => playerCharacterCombatController.SwitchToWeapon(2);
        _playerControls.Player.Weapon4.performed += ctx => playerCharacterCombatController.SwitchToWeapon(3);
                        

        // Assign the HandleMouseScroll method to the respective input actions
        _playerControls.Player.MouseScrollUp.performed += ctx => { MouseScroll = 1; HandleMouseScroll(); };
        _playerControls.Player.MouseScrollDown.performed += ctx => { MouseScroll = -1; HandleMouseScroll(); };        
    }        

    void Update()
    {
        HandleInput();
        playerCharacterMovementController.HandleMovement(playerMovementInput, playerLookInput);                
    }

    public static void SwitchPlayerControlType(PlayerControlTypes playerControlTypes)
    {
        switch (playerControlTypes)
        {
            case PlayerControlTypes.GAMEPLAY:
                _playerControls.Player.Enable();
                _playerControls.UI.Disable();
                _playerControls.Cutscene.Disable();
                Cursor.lockState = CursorLockMode.Locked;
                HUDManager.Enable();
                break;
            case PlayerControlTypes.UI:
                _playerControls.Player.Disable();
                _playerControls.UI.Enable();
                _playerControls.Cutscene.Disable();
                Cursor.lockState = CursorLockMode.None;
                HUDManager.Disable();
                break;
            case PlayerControlTypes.CUTSCENE:
                _playerControls.Player.Disable();
                _playerControls.UI.Disable();
                _playerControls.Cutscene.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                HUDManager.Disable();
                break;
        }
    }    

    private void HandleMouseScroll()
    {
        // Use the new IReadOnlyList<IWeapon> PlayerWeapons property
        var ownedWeapons = playerCharacterCombatController.PlayerWeapons;
        int inventoryCount = ownedWeapons.Count;

        if (inventoryCount <= 1)
            return; // No need to scroll if only one weapon

        // Get the current weapon index by matching WeaponType
        int currentIndex = ownedWeapons
            .Select((w, idx) => new { w, idx })
            .FirstOrDefault(x => x.w.WeaponType == playerCharacterCombatController.WeaponSelected)?.idx ?? 0;

        // Calculate new index based on scroll direction
        int newIndex = currentIndex + MouseScroll;

        // Wrap around
        if (newIndex < 0)
            newIndex = inventoryCount - 1;
        else if (newIndex >= inventoryCount)
            newIndex = 0;

        // Switch weapon using WeaponType
        playerCharacterCombatController.SwitchToWeapon(newIndex);
    }

    private void HandleInput()
    {
        if (PrimaryActionButtonPressed) PerformPrimaryAction();
        //if (rmbPressed) PerformSecondaryAction();

        playerCharacterCombatController?.ChargeWeapon(SecondaryActionButtonPressed);
        
        playerMovementInput = _playerControls.Player.Move.ReadValue<Vector2>();
        playerLookInput = _playerControls.Player.Look.ReadValue<Vector2>();
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
        _playerControls.Enable();        
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

}
