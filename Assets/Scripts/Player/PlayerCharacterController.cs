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
        _playerControls.Player.Weapon1.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Melee);
        _playerControls.Player.Weapon2.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Pistol);
        _playerControls.Player.Weapon3.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Shotgun);
        _playerControls.Player.Weapon4.performed += ctx => playerCharacterCombatController.SwitchToWeapon(WeaponTypes.Crossbow);
                        

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
