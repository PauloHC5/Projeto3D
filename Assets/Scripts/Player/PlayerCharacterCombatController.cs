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
    private List<IWeapon> _playerWeapons = new List<IWeapon>();
    private Dictionary<AmmoTypes, Int32> _playerGunAmmo;
    private PlayerCombatStates _playerCombatStates = PlayerCombatStates.RAISING;
    private PlayerCharacterAnimationsController _playerCharacterAnimationsController;
    private MouseLook _mouseLook;    
    private int _whichIndexShotgunsWereAssigned = 0; // Used to track which index the shotguns were assigned to in the player weapons list

    public PlayerWeaponTypes WeaponSelected => _weaponSelected;
    public IReadOnlyList<IWeapon> PlayerWeapons => _playerWeapons.AsReadOnly();    

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
        _mouseLook = GetComponentInChildren<MouseLook>();

        InitializeWeaponsSet();                

        _playerGunAmmo = GunsAmmo.ToDictionary(g => g.AmmoType, g => g.AmmoAmount);
    }

    private void Start()
    {           
        // Switch to the first weapon in the inventory
        if (_playerWeapons.Count > 0)
        {
            // If the selected weapon is not in the inventory, switch to the first weapon in the inventory
            RaiseWeapon(_playerWeapons[0].WeaponType);
        }
        else
        {
            Debug.LogWarning("No weapons in the inventory.");
        }

        //Debug.Log($"Player weapons initialized with {_playerWeapons.Count} weapons.");
        /*foreach (var weapon in _playerWeapons)
        {
            Debug.Log($"Weapon: {weapon.WeaponType}");
        }*/
    }

    private void InitializeWeaponsSet()
    {
        // Initialize weapons
        foreach (var weaponPrefab in WeaponsPrefabs
            .Select(w => w.prefab)
            .Where(w => w != null)
            .Where(w => !_playerWeapons.Any(weap => weap.WeaponType == w.WeaponType)))
        {
            if (weaponPrefab is CarnivovrousPlant carnivorousPlant)
            {
                var dualWieldMelee = new DualWieldMeleeManager(
                    carnivorousPlant,
                    _rightHandSocket,
                    _leftHandSocket,
                    _playerCharacterAnimationsController
                );
                _playerWeapons.Add(dualWieldMelee);
            }
            else
            if(weaponPrefab is BananaShotgun bananaShotgun)
            {
                var dualWieldGun = new DualWieldGunManager(
                    bananaShotgun,
                    _rightHandSocket,
                    _leftHandSocket,
                    _playerCharacterAnimationsController
                );
                _playerWeapons.Add(dualWieldGun);
                _whichIndexShotgunsWereAssigned = _playerWeapons.Count - 1; // Store the index where the shotguns were assigned
            }            
            else
            {
                var instantiatedWeapon = Instantiate(
                    weaponPrefab,
                    weaponPrefab.GetSocketToAttach == WeaponSocket.RightHandSocket ? _rightHandSocket : _leftHandSocket
                );
                _playerWeapons.Add(instantiatedWeapon);
            }
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

        var weapon = _playerWeapons.FirstOrDefault(w => w.WeaponType == weaponToSwitch);
        if (weapon != null)
            RaiseWeapon(weaponToSwitch);                
    }

    public void SwitchToWeapon(int index)
    {
        if (index < 0 || index >= _playerWeapons.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {index}. Cannot switch to weapon.");
            return; // If the index is out of bounds, do nothing
        }        

        if(index == _whichIndexShotgunsWereAssigned && _playerWeapons[index] is null)
        {
            // If the index is the one where the shotguns were assigned, retrieve the new shotguns
            RetrieveNewShotguns();            
        }

        var weaponToSwitch = _playerWeapons[index].WeaponType;
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
            weapon?.DisableWeapon();
        }

        var weaponToEquip = _playerWeapons.FirstOrDefault(w => w != null && w.WeaponType == weaponToSwitch);
        _equippedWeapon = weaponToEquip;
        if (_equippedWeapon == null)
        {
            Debug.LogWarning($"Weapon {weaponToSwitch} not found in the inventory.");
            return; // If the weapon is not found in the inventory, do nothing
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
            if(_mouseLook) _mouseLook.ZoomOut(); // Zoom out the camera if the player is reloading a gun
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
        // Replace the foreach loop with a for loop to avoid modifying the iteration variable
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            if (_playerWeapons[i].WeaponType == PlayerWeaponTypes.BANANASHOTGUN)
            {
                _playerWeapons[i] = null; // Set the weapon to null
            }
        }

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
        
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            if (_playerWeapons[i] == null)
            {
                _playerWeapons[i] = shotguns; // Assign the new shotguns
                break;
            }
        }

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