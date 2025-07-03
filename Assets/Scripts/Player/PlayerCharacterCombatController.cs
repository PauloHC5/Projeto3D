using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;


public enum PlayerWeaponTypes
{
    CARNIVOROUSPLANTS = 0,
    ACORNGUN = 1,
    BANANASHOTGUN = 2,
    CACTUSSCROSSBOW = 3,    
}

public enum PlayerCombatStates
{
    RAISING,
    RELOADING,
    ATTACKING,
    FIRING,
    DUALWIELDFIRING,
    CHARGING,
    INSPECTINGWEAPON,

    DEFAULT
}

public enum WeaponSocket
{
    RightHandSocket,
    LeftHandSocket    
}

[Serializable]
public struct WeaponPrefab
{
    [HideInInspector] public string name;
    public Weapon prefab;
}

[Serializable]
public struct GunAmmo
{
    public AmmoTypes AmmoType;
    public int AmmoAmount;    
}

[Serializable]
public enum AmmoTypes
{
    Acorn,
    Banana,
    Spikes,
    Corn,
    Coconut,
}

public class PlayerCharacterCombatController : MonoBehaviour
{
    [Space]
    [SerializeField] private PlayerWeaponTypes _weaponSelected;        
    [SerializeField] private Transform _rightHandSocket, _leftHandSocket;

    // Weapon prefabs and ammo amounts for the guns
    // This section is only visible in the Unity Editor for easy configuration
    // if you need access player weapons or ammo amounts in runtime, use _playerWeaponsSet and _playerGunAmmo
    [Header("Weapons Prefabs and Ammo")]
#if UNITY_EDITOR    
    [SerializeField] public WeaponPrefab[] WeaponsPrefabs;    
    [SerializeField] public GunAmmo[] GunsAmmo;
#endif    

    private IWeapon _equippedWeapon;
    private Dictionary<PlayerWeaponTypes, IWeapon> _playerWeapons = new();
    private Dictionary<AmmoTypes, Int32> _playerGunAmmo;
    private PlayerCombatStates _playerCombatStates = PlayerCombatStates.RAISING;
    private PlayerCharacterAnimationsController _playerCharacterAnimationsController;
    private MouseLook mouseLook;    

    public PlayerWeaponTypes WeaponSelected => _weaponSelected;
    public Dictionary<PlayerWeaponTypes, IWeapon> PlayerWeapons => _playerWeapons;    

    public IWeapon EquippedWeapon => _equippedWeapon;
    public Dictionary<AmmoTypes, Int32> PlayerGunsAmmo => _playerGunAmmo;    
    public void SetPlayerGunsAmmo(AmmoTypes type, int amount)
    {
        _playerGunAmmo[type] = Mathf.Max(0, amount);
    }

    public PlayerCombatStates PlayerCombatStates
    {
        get { return _playerCombatStates; }
        set { _playerCombatStates = value; }
    }
    public PlayerCharacterAnimationsController PlayerCharacterAnimationsController => _playerCharacterAnimationsController;

    public static event Action onSwitchToWeapon;               

    private void Awake()
    {                
        _playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
        mouseLook = GetComponentInChildren<MouseLook>();

        // Initialize weapons inventory and guns ammo        
        foreach (var weaponPrefab in WeaponsPrefabs
            .Select(w => w.prefab)
            .Where(w => w != null)
            .Where(w => !_playerWeapons.ContainsKey(w.WeaponType)))
        {
            if (weaponPrefab is CarnivovrousPlant carnivorousPlant)
            {
                // Use DualWieldMeleeManager for CarnivorousPlant
                var dualWieldMelee = new DualWieldMeleeManager(
                    carnivorousPlant,
                    _rightHandSocket,
                    _leftHandSocket,
                    _playerCharacterAnimationsController
                );
                _playerWeapons.Add(PlayerWeaponTypes.CARNIVOROUSPLANTS, dualWieldMelee);
            }
            else
            {
                // Default instantiation for other weapons
                var instantiatedWeapon = Instantiate(
                    weaponPrefab,
                    weaponPrefab.GetSocketToAttach == WeaponSocket.RightHandSocket ? _rightHandSocket : _leftHandSocket
                );
                _playerWeapons.Add(instantiatedWeapon.WeaponType, instantiatedWeapon);
            }
        }

        Debug.Log($"Player weapons initialized with {_playerWeapons.Count} weapons.");
        foreach (var weapon in _playerWeapons)
        {
            Debug.Log($"Weapon: {weapon.Key}");
        }

        _playerGunAmmo = GunsAmmo.ToDictionary(g => g.AmmoType, g => g.AmmoAmount); // Initialize playerGunAmmo dictionary with the gunsAmmo array
    }

    private void Start()
    {           
        // Switch to the first weapon in the inventory
        if (_playerWeapons.Count > 0)
        {
            /*if (_playerWeapons.ContainsKey(WeaponSelected))
                RaiseWeapon(_weaponSelected);
            else*/
                RaiseWeapon(_playerWeapons.First().Key); // If the selected weapon is not in the inventory, switch to the first weapon in the inventory
        }
        else
        {
            Debug.LogWarning("No weapons in the inventory.");
        }
    }

    private void Update()
    {
        // if k button is pressed, add 3 to playerWeaponAmmo[weaponSelected]
        if (_equippedWeapon is IEquippedGun equippedGun)
        {            
            if (Keyboard.current.kKey.wasPressedThisFrame) _playerGunAmmo[equippedGun.AmmoType] += 3;

            _playerCharacterAnimationsController.CheckAutoReload(equippedGun.MagAmmo, equippedGun.MagCapacity, _playerGunAmmo[equippedGun.AmmoType]);
        }       
    }                

    public void SwitchToWeapon(PlayerWeaponTypes weaponToSwitch)
    {
        if (!ConditionToSwitchWeapon(weaponToSwitch)) return;        

        if(_playerWeapons.ContainsKey(WeaponSelected) && _playerWeapons[weaponToSwitch] != null) RaiseWeapon(weaponToSwitch);                
    }

    public void SwitchToWeapon(int index)
    {
        if (index < 0 || index >= _playerWeapons.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {index}. Cannot switch to weapon.");
            return; // If the index is out of bounds, do nothing
        }

        var weaponToSwitch = _playerWeapons.Keys.ElementAt(index);
        if (ConditionToSwitchWeapon(weaponToSwitch))
        {
            RaiseWeapon(weaponToSwitch);
        }
    }

    private bool ConditionToSwitchWeapon(PlayerWeaponTypes weaponToSwitch) =>
        !PlayerCharacterController.PrimaryActionButtonPressed &&
        PlayerCombatStates != PlayerCombatStates.ATTACKING &&
        PlayerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        PlayerCombatStates != PlayerCombatStates.FIRING &&
        PlayerCombatStates != PlayerCombatStates.ATTACKING &&
        weaponToSwitch != _weaponSelected;

    private void RaiseWeapon(PlayerWeaponTypes weaponToSwitch)
    {        
        foreach (var weapon in _playerWeapons)
        {
            weapon.Value.DisableWeapon();
        }

        _equippedWeapon = _playerWeapons[weaponToSwitch];                
        if (_equippedWeapon == null)
        {
            Debug.LogWarning($"Weapon {weaponToSwitch} not found in the inventory.");
            return; // If the weapon is not found in the inventory, do nothing
        }

        if (_equippedWeapon.WeaponType == PlayerWeaponTypes.BANANASHOTGUN)
        {
            RetrieveNewShotguns();
        }

        _weaponSelected = _equippedWeapon.WeaponType; // Set the weapon selected to the equipped weapon type

        _equippedWeapon.EnableWeapon(); // Enable the equipped weapon                

        _playerCharacterAnimationsController.PlaySwitchToWeapon(_weaponSelected); // Play the switch to weapon animation

        onSwitchToWeapon?.Invoke();
    }    

    public void PerformPrimaryAction()
    {
        // Check if the equipped weapon implements the IEquippedGun interface
        if (_equippedWeapon is IEquippedGun equippedGun && ConditionsToFire(equippedGun))
        {
            if (equippedGun.MagAmmo > 0)
            {
                equippedGun.Fire();
                _playerCharacterAnimationsController.PlayUseWeapon();
            }
            else
            {
                PerformReload(); // If the weapon has no ammo, perform a reload
            }            
        }
        else if (_equippedWeapon is IEquippedMelee equippedMelee && equippedMelee.CanAttack)
        {
            equippedMelee.Attack();
        }
    }

    private bool ConditionsToFire(IEquippedGun equippedGun) =>
        _playerCombatStates != PlayerCombatStates.RELOADING &&        
        _playerCombatStates != PlayerCombatStates.RAISING &&
        _playerCombatStates != PlayerCombatStates.FIRING &&
        _playerCombatStates != PlayerCombatStates.CHARGING &&
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        equippedGun.CanFire;            

    public void PerformReload()
    {
        if(_equippedWeapon is IEquippedGun equippedGun && ConditionsToReload(equippedGun))
        {
            equippedGun.PerformReload();
            _playerCharacterAnimationsController.PlayReload();
            if(mouseLook) mouseLook.ZoomOut(); // Zoom out the camera if the player is reloading a gun
        }
    }

    private bool ConditionsToReload(IEquippedGun equippedGun) =>
        equippedGun.CanReload() &&
        _playerGunAmmo[equippedGun.AmmoType] > 0 &&
        _playerCombatStates != PlayerCombatStates.RELOADING &&
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        _playerCombatStates != PlayerCombatStates.FIRING;

    public void Reload()
    {        
        if(_equippedWeapon is IEquippedGun equippedGun)
        {
            int equippedGunAmmo = _playerGunAmmo[equippedGun.AmmoType];
            equippedGun.Reload(ref equippedGunAmmo);
            _playerGunAmmo[equippedGun.AmmoType] = equippedGunAmmo;
        }
    }
       
    public void ChargeWeapon(bool buttomPressed)
    {
        var equippedGun = _equippedWeapon as IEquippedGun;

        if (ConditionsToCharge(equippedGun) && _equippedWeapon is IChargeable chargeableWeapon)
        {
            chargeableWeapon.PerformCharge(buttomPressed);
            _playerCharacterAnimationsController.ChargeWeapon(buttomPressed);
        }        
    }

    private bool ConditionsToCharge(IEquippedGun equippedGun) =>
        equippedGun != null &&
        equippedGun is IChargeable &&
        equippedGun.CanFire &&
        equippedGun.MagAmmo == equippedGun.MagCapacity &&
        _playerCombatStates != PlayerCombatStates.RELOADING &&
        _playerCombatStates != PlayerCombatStates.ATTACKING &&
        _playerCombatStates != PlayerCombatStates.RAISING &&
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON;


    public void PerformSecondaryAction()
    {   
        if(_equippedWeapon is DualWieldGunManager dualWieldGun && ConditionsToSuperFire(dualWieldGun))
        {
            dualWieldGun.FireBoth();
            _playerCharacterAnimationsController.PlayFireBoth();
            GetComponent<PlayerCharacterMovementController>().ApplyImpulse(20f); // Apply backward impulse from camera when firing both guns
            return;
        }        

        if (_equippedWeapon is ISecondaryAction equippedGun) equippedGun.Perform();
    }

    private bool ConditionsToSuperFire(IEquippedGun equippedGun) =>
        _playerCombatStates != PlayerCombatStates.RELOADING &&
        _playerCombatStates != PlayerCombatStates.RAISING &&
        _playerCombatStates != PlayerCombatStates.FIRING &&
        _playerCombatStates != PlayerCombatStates.CHARGING &&
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        equippedGun.CanFire &&
        equippedGun.MagAmmo == equippedGun.MagCapacity;

    private void DropShotgun()
    {                       
        _playerWeapons.Remove(PlayerWeaponTypes.BANANASHOTGUN); // Remove the shotgun from the player's weapons

        _equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {
        BananaShotgun shotgunPrefab = WeaponsPrefabs.Any(w => w.prefab.WeaponType == PlayerWeaponTypes.BANANASHOTGUN)
            ? WeaponsPrefabs.FirstOrDefault(w => w.prefab.WeaponType == PlayerWeaponTypes.BANANASHOTGUN).prefab as BananaShotgun
            : null;
        if (shotgunPrefab == null) return;        

        DualWieldGunManager shotguns = new DualWieldGunManager(shotgunPrefab, _rightHandSocket, _leftHandSocket, _playerCharacterAnimationsController);
        shotguns.EnableWeapon();

        _playerWeapons[PlayerWeaponTypes.BANANASHOTGUN] = shotguns; // Add the new shotguns to the player's weapons

        _equippedWeapon = shotguns;
        var playerShotgunsAmmo = _playerGunAmmo[shotguns.AmmoType];
        shotguns.Reload(ref playerShotgunsAmmo);        
        SetPlayerGunsAmmo(shotguns.AmmoType, playerShotgunsAmmo); // Set the ammo for the new shotguns
    }

    [ExecuteInEditMode]
    private void OnEnable()
    {
        AnimationTriggerEvents.onDropShotgun += DropShotgun;
        AnimationTriggerEvents.onReTrieveNewShotguns += RetrieveNewShotguns;
        AnimationTriggerEvents.onReload += Reload;        
    }

    private void OnDisable()
    {
        AnimationTriggerEvents.onDropShotgun -= DropShotgun;
        AnimationTriggerEvents.onReTrieveNewShotguns -= RetrieveNewShotguns;
        AnimationTriggerEvents.onReload -= Reload;        
    }
}