using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerControlTypes
{
    GAMEPLAY,
    UI,
    CUTSCENE,
    DISABLED
}

public class PlayerCharacterController : MonoBehaviour
{
    [HideInInspector] public bool primaryActionButtonPressed = false;
    [HideInInspector] public bool secondaryActionButtonPressed = false;

    private PlayerInputActions _playerControls;
    public PlayerInputActions PlayerControls => _playerControls;
    
    private InputAction _reloadAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;

    private int _mouseScroll;
    private Vector2 _playerMovementInput;
    private Vector2 _playerLookInput;

    private PlayerCharacterMovementController _playerCharacterMovementController;
    private PlayerCharacterCombatController _playerCharacterCombatController;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        
        Cursor.lockState = CursorLockMode.Locked;
        
        _playerCharacterMovementController = GetComponent<PlayerCharacterMovementController>();
        _playerCharacterCombatController = GetComponent<PlayerCharacterCombatController>();
        
        InitializePlayerControls();
    }

    private void InitializePlayerControls()
    {
        _playerControls.Player.PrimaryAction.started += ctx => primaryActionButtonPressed = true;
        _playerControls.Player.PrimaryAction.canceled += ctx => primaryActionButtonPressed = false;

        _playerControls.Player.SecondaryAction.started += ctx => secondaryActionButtonPressed = true;
        _playerControls.Player.SecondaryAction.canceled += ctx => secondaryActionButtonPressed = false;

        _playerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();
        _reloadAction = _playerControls.Player.Reload;
        _jumpAction = _playerControls.Player.Jump;
        _crouchAction = _playerControls.Player.Crouch;

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
        _playerControls.Player.Weapon1.performed += ctx => _playerCharacterCombatController.SwitchToWeapon(0);
        _playerControls.Player.Weapon2.performed += ctx => _playerCharacterCombatController.SwitchToWeapon(1);
        _playerControls.Player.Weapon3.performed += ctx => _playerCharacterCombatController.SwitchToWeapon(2);
        _playerControls.Player.Weapon4.performed += ctx => _playerCharacterCombatController.SwitchToWeapon(3);


        // Assign the HandleMouseScroll method to the respective input actions
        _playerControls.Player.MouseScrollUp.performed += ctx => { _mouseScroll = 1; HandleMouseScroll(); };
        _playerControls.Player.MouseScrollDown.performed += ctx => { _mouseScroll = -1; HandleMouseScroll(); };
    }

    void Update()
    {
        HandleInput();
        _playerCharacterMovementController.HandleMovement(_playerMovementInput, _playerLookInput);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void SwitchPlayerControlType(PlayerControlTypes playerControlTypes)
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
                break;
            case PlayerControlTypes.DISABLED:
                _playerControls.Player.Disable();
                _playerControls.UI.Disable();
                _playerControls.Cutscene.Disable();
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }

    private void HandleMouseScroll()
    {
        if (_playerCharacterCombatController == null || _playerCharacterCombatController.EquippedWeapon == null || _playerCharacterCombatController.WeaponOrder == null || _playerCharacterCombatController.PlayerWeapons == null)
            return;
        
        // Get the weapon order and the dictionary of weapons
        var weaponOrder = _playerCharacterCombatController.WeaponOrder;
        var playerWeapons = _playerCharacterCombatController.PlayerWeapons;

        int inventoryCount = weaponOrder.Count;
        if (inventoryCount <= 1)
            return; // No need to scroll if only one weapon

        // Get the current index of the equipped weapon
        int currentIndex = _playerCharacterCombatController.WeaponOrder.ToList().IndexOf(_playerCharacterCombatController.EquippedWeapon.WeaponType);

        // Calculate new index based on scroll direction
        int newIndex = currentIndex - _mouseScroll;

        // Wrap around
        if (newIndex < 0)
            newIndex = inventoryCount - 1;
        else if (newIndex >= inventoryCount)
            newIndex = 0;

        // Find the next available (non-null) weapon
        for (int i = 0; i < inventoryCount; i++)
        {
            int tryIndex = (newIndex + i) % inventoryCount;
            var tryWeaponType = weaponOrder[tryIndex];
            if (playerWeapons.TryGetValue(tryWeaponType, out var weapon) && weapon != null)
            {
                _playerCharacterCombatController.SwitchToWeapon(tryWeaponType);
                break;
            }
        }
    }

    private void HandleInput()
    {
        if (primaryActionButtonPressed) PerformPrimaryAction();

        _playerCharacterCombatController?.ChargeWeapon(secondaryActionButtonPressed);

        _playerMovementInput = _playerControls.Player.Move.ReadValue<Vector2>();
        _playerLookInput = _playerControls.Player.Look.ReadValue<Vector2>();
    }

    private void PerformPrimaryAction()
    {
        if (_playerCharacterCombatController)
        {
            if (_playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RELOADING || _playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING || _playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.RAISING) return;

            _playerCharacterCombatController.PerformPrimaryAction();
        }
    }

    private void PerformSecondaryAction()
    {
        if (_playerCharacterCombatController)
        {
            _playerCharacterCombatController.PerformSecondaryAction();
        }
    }

    private void PerformReload()
    {
        _playerCharacterCombatController.PerformReload();
    }

    private void PerformJump()
    {
        _playerCharacterMovementController.Jump();
    }

    private void Crouch()
    {
        _playerCharacterMovementController.Crouch();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        if (_reloadAction != null)
            _reloadAction.performed += OnReloadPerformed;
        if(_jumpAction != null)
            _jumpAction.performed += ctx => PerformJump();
        if(_crouchAction != null)
            _crouchAction.performed += ctx => Crouch();
    }

    private void OnDisable()
    {
        if (_reloadAction != null)
            _reloadAction.performed -= OnReloadPerformed;
        if(_jumpAction != null)
            _jumpAction.performed -= ctx => PerformJump();
        if(_crouchAction != null)
            _crouchAction.performed -= ctx => Crouch();
        
        _playerControls.Disable();
    }
    
    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        // Unity's null check works for "destroyed" objects due to operator overload
        if (this == null) return;

        _playerCharacterCombatController.PerformReload();
    }

}
