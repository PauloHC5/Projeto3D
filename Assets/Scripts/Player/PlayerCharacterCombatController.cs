using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

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

    public WeaponPrefab(string toString, Weapon newWeapon)
    {
        name = toString;
        prefab = newWeapon as Weapon; // Cast the IWeapon to Weapon
        if (prefab == null)
        {
            Debug.LogError($"WeaponPrefab: {toString} is not a valid Weapon type.");
        }
    }
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
    [SerializeField] private bool skipWeaponInspection = false;
    
    [Space]
    
    [Header("Weapon Socket")]
    [SerializeField] private Transform _rightHandSocket, _leftHandSocket;

    [Space]
    
    // Weapon prefabs and ammo amounts for the guns
    // This section is only visible in the Unity Editor for easy configuration
    // if you need access player weapons or ammo amounts in runtime, use _playerWeaponsSet and _playerGunAmmo
    [Header("Weapons Prefabs and Ammo")]
    public List<WeaponPrefab> WeaponsPrefabs;
    public GunAmmo[] GunsAmmoInitializer;

    private IWeapon _equippedWeapon;
    public IWeapon EquippedWeapon => _equippedWeapon;

    private Dictionary<PlayerWeaponTypes, IWeapon> _playerWeapons = new Dictionary<PlayerWeaponTypes, IWeapon>();
    public IReadOnlyDictionary<PlayerWeaponTypes, IWeapon> PlayerWeapons => _playerWeapons;

    private Dictionary<AmmoTypes, Int32> _playerGunAmmo;
    public Dictionary<AmmoTypes, Int32> PlayerGunsAmmo => _playerGunAmmo;

    private PlayerCombatStates _playerCombatStates = PlayerCombatStates.DEFAULT;
    private PlayerCharacterAnimationsController _playerCharacterAnimationsController;
    private MouseLook _mouseLook;

    private List<PlayerWeaponTypes> _weaponOrder = new List<PlayerWeaponTypes>();
    public IReadOnlyList<PlayerWeaponTypes> WeaponOrder => _weaponOrder;

    public void SetPlayerGunsAmmo(AmmoTypes type, int amount)
    {
        if (!enabled) return;
        if (_playerGunAmmo.ContainsKey(type))
            _playerGunAmmo[type] = Mathf.Max(0, amount);
    }

    public PlayerCombatStates PlayerCombatStates
    {
        get { return _playerCombatStates; }
        set { _playerCombatStates = value; }
    }

    public PlayerCharacterAnimationsController PlayerCharacterAnimationsController =>
        _playerCharacterAnimationsController;

    public static event Action OnSwitchToWeapon;

    private void Awake()
    {
        _playerCharacterAnimationsController =
            new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>(), skipWeaponInspection);
        _mouseLook = GetComponentInChildren<MouseLook>();

        InitializePlayerWeapons();

        _playerGunAmmo = GunsAmmoInitializer.ToDictionary(g => g.AmmoType, g => g.AmmoAmount);
    }

    private void Start()
    {
        // Switch to the first weapon in the inventory
        if (_playerWeapons.Count > 0)
        {
            RaiseWeapon(_playerWeapons.First().Key);
        }
        else
        {
            Debug.LogWarning("No weapons in the inventory.");
        }
    }

    private void InitializePlayerWeapons()
    {
        foreach (var weaponPrefab in WeaponsPrefabs)
        {
            // Check if the weapon type is already in the player's weapons
            if(IsWeaponAlreadyInPlayerWeapons(weaponPrefab.prefab.WeaponType))
                continue; // Skip adding this weapon if it's already present
            
            var instantiatedWeapon = Instantiate(weaponPrefab.prefab);
            AddWeapon(new[] { instantiatedWeapon });
        }
    }
    
    private bool IsWeaponAlreadyInPlayerWeapons(PlayerWeaponTypes weaponType)
    {
        var weaponAlreadyExists = _playerWeapons.ContainsKey(weaponType) && _playerWeapons[weaponType] != null;
        if (weaponAlreadyExists)
        {
            Debug.LogWarning($"Weapon {weaponType} is already in the player's weapons.");
        }
        return weaponAlreadyExists;
    }

    private void AddWeapon(Weapon[] newWeapon)
    {
        // Check if the weapon is null
        if (newWeapon == null || newWeapon.Length == 0 || newWeapon[0] == null)
        {
            Debug.LogWarning("Cannot add a null weapon to the player's weapons.");
            return;
        }

        // Check if the weapon is already in the player's weapons
        if (IsWeaponAlreadyInPlayerWeapons(newWeapon[0].WeaponType))
            return;

        if (WeaponsPrefabs.All(w => w.prefab.WeaponType != newWeapon[0].WeaponType))
        {
            var prefabs = Resources.LoadAll("Weapons"); // Load all weapon prefabs from the Resources folder

            // Find the weapon prefab in the loaded resources
            var weaponPrefab = prefabs
                .Select(prefab => prefab.GetComponent<Weapon>()) // Get the Weapon component from each prefab
                .FirstOrDefault(w =>
                    w != null &&
                    w.WeaponType ==
                    newWeapon[0].WeaponType); // Check if the prefab's WeaponType matches the new weapon's WeaponType

            if (weaponPrefab is null)
            {
                Debug.LogWarning("Weapon prefab not found for " + newWeapon[0].WeaponType);
            }
            else
                WeaponsPrefabs.Add(new WeaponPrefab(newWeapon[0].WeaponType.ToString(), weaponPrefab));
        }

        IWeapon weaponToAdd; // Variable to hold the weapon to add to the player's weapons
        
        switch (newWeapon[0]) // Check the type of the new weapon to determine which manager to instantiate
        {
            case CarnivovrousPlant:
                if(newWeapon.Length == 1) // If only one CarnivorousPlant is provided, instantiate a second one 
                    newWeapon = new[] { newWeapon[0], Instantiate(newWeapon[0]) };
                
                var dualWieldMelee = new DualWieldMeleeManager(
                    new[] { newWeapon[0] as CarnivovrousPlant, newWeapon[1] as CarnivovrousPlant },
                    _playerCharacterAnimationsController
                );
                dualWieldMelee.AttatchToSocket(_rightHandSocket,
                    _leftHandSocket); // Attach the dual wield melee to the sockets
                weaponToAdd = dualWieldMelee; // Initialize the weapon to add with the dual wield melee manager
                break;
            case BananaShotgun:
                if (newWeapon.Length == 1) // If only one BananaShotgun is provided, instantiate a second one
                    newWeapon = new[] { newWeapon[0], Instantiate(newWeapon[0]) };
                
                var dualWieldGun = new DualWieldGunManager(
                    new[] { newWeapon[0] as Gun, newWeapon[1] as Gun },
                    _playerCharacterAnimationsController
                );
                dualWieldGun.AttatchToSocket(_rightHandSocket,
                    _leftHandSocket); // Attach the dual wield gun to the sockets
                weaponToAdd = dualWieldGun; // Initialize the weapon to add with the dual wield gun manager
                break;
            default:
                weaponToAdd = newWeapon[0]; // Use the first weapon in the array as the weapon to add
                weaponToAdd.AttatchToSocket(_rightHandSocket, _leftHandSocket);
                break;
        }


        // Check if there is a null weapon in the player's weapons
        // If there is, replace it with the new weapon
        if (_playerWeapons.Any(w => w.Value is null))
        {
            // If there is a null weapon in the player's weapons, replace it with the new weapon
            var nullWeaponIndex = _playerWeapons.FirstOrDefault(w => w.Value == null).Key;
            _playerWeapons[nullWeaponIndex] = weaponToAdd;
            return; // Exit after replacing the null weapon
        }
        else // If there is no null weapon, add the new weapon to the player's weapons
        {
            _playerWeapons.Add(weaponToAdd.WeaponType, weaponToAdd);
            _weaponOrder.Add(weaponToAdd.WeaponType);
        }
    }

    private void Update()
    {
        // if k button is pressed, add 3 to playerWeaponAmmo[weaponSelected]
        if (_equippedWeapon is IEquippedGun equippedGun)
        {
            if (Keyboard.current.kKey.wasPressedThisFrame) _playerGunAmmo[equippedGun.AmmoType] += 3;

            _playerCharacterAnimationsController.CheckAutoReload(equippedGun.MagAmmo, equippedGun.MagCapacity,
                _playerGunAmmo[equippedGun.AmmoType]);
        }
    }

    public void SwitchToWeapon(PlayerWeaponTypes weaponToSwitch)
    {
        if (!enabled) return;
        if (!ConditionToSwitchWeapon(weaponToSwitch)) return;

        if (_playerWeapons.TryGetValue(weaponToSwitch, out var weapon))
            RaiseWeapon(weaponToSwitch);
    }

    public void SwitchToWeapon(int index)
    {
        if (!enabled) return;
        if (index < 0 || index >= _weaponOrder.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {index}. Cannot switch to weapon.");
            return;
        }

        var weaponType = _weaponOrder[index];
        if (_playerWeapons.TryGetValue(weaponType, out var weapon) && weapon != null &&
            ConditionToSwitchWeapon(weapon.WeaponType))
        {
            RaiseWeapon(weapon.WeaponType);
        }
    }

    private bool ConditionToSwitchWeapon(PlayerWeaponTypes weaponToSwitch) =>
        !PlayerCharacterController.PrimaryActionButtonPressed &&
        PlayerCombatStates != PlayerCombatStates.ATTACKING &&
        PlayerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        PlayerCombatStates != PlayerCombatStates.FIRING &&
        PlayerCombatStates != PlayerCombatStates.ATTACKING &&
        weaponToSwitch != _equippedWeapon?.WeaponType;


    private void RaiseWeapon(PlayerWeaponTypes weaponToRaise)
    {
        if (_playerWeapons.TryGetValue(weaponToRaise, out var weaponToEquip))
            EquipWeapon(weaponToEquip);

        _playerCharacterAnimationsController?.PlayRaiseWeapon(_equippedWeapon.WeaponType);
        OnSwitchToWeapon?.Invoke();
    }

    private void EquipWeapon(IWeapon weaponToEquip)
    {
        foreach (var weapon in _playerWeapons.Values)
            weapon?.DisableWeapon();

        _equippedWeapon = weaponToEquip;
        if (_equippedWeapon == null)
        {
            Debug.LogWarning($"Weapon to equip not found in the inventory.");
            return;
        }

        _equippedWeapon.EnableWeapon();
    }

    public void PerformPrimaryAction()
    {
        if (!enabled) return;
        switch (_equippedWeapon)
        {
            case IEquippedGun equippedGun when ConditionsToFire(equippedGun):
                if (equippedGun.MagAmmo > 0)
                {
                    equippedGun.Fire();
                    _playerCharacterAnimationsController?.PlayUseWeapon();
                }
                else
                {
                    PerformReload();
                }

                break;
            case IEquippedMelee equippedMelee when ConditionsToAttack(equippedMelee):
                equippedMelee.Attack();
                break;
        }
    }

    private bool ConditionsToFire(IEquippedGun equippedGun) =>
        _playerCombatStates != PlayerCombatStates.RELOADING &&
        _playerCombatStates != PlayerCombatStates.RAISING &&
        _playerCombatStates != PlayerCombatStates.FIRING &&
        _playerCombatStates != PlayerCombatStates.CHARGING &&
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        equippedGun.CanFire;

    private bool ConditionsToAttack(IEquippedMelee equippedMelee) =>
        _playerCombatStates != PlayerCombatStates.INSPECTINGWEAPON &&
        equippedMelee.CanAttack;

    public void PerformReload()
    {
        if (!enabled) return;
        if (_equippedWeapon is IEquippedGun equippedGun && ConditionsToReload(equippedGun))
        {
            equippedGun.PerformReload();
            _playerCharacterAnimationsController.PlayReload();
            if (_mouseLook) _mouseLook.ZoomOut(); // Zoom out the camera if the player is reloading a gun
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
        if (!enabled) return;
        if (_equippedWeapon is IEquippedGun equippedGun)
        {
            int equippedGunAmmo = _playerGunAmmo[equippedGun.AmmoType];
            equippedGun.Reload(ref equippedGunAmmo);
            _playerGunAmmo[equippedGun.AmmoType] = equippedGunAmmo;
        }
    }

    public void ChargeWeapon(bool buttomPressed)
    {
        if (!enabled) return;
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
        if (!enabled) return;
        if (_equippedWeapon is DualWieldGunManager dualWieldGun && ConditionsToSuperFire(dualWieldGun))
        {
            dualWieldGun.FireBoth();
            _playerCharacterAnimationsController.PlayFireBoth();
            GetComponent<PlayerCharacterMovementController>()
                .ApplyImpulse(20f); // Apply backward impulse from camera when firing both guns
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
        // Set the value to null instead of removing the key
        if (_playerWeapons.ContainsKey(PlayerWeaponTypes.BANANASHOTGUN))
        {
            _playerWeapons[PlayerWeaponTypes.BANANASHOTGUN] = null;
        }

        _equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {
        BananaShotgun shotgunPrefab =
            WeaponsPrefabs.FirstOrDefault(w => w.prefab.WeaponType == PlayerWeaponTypes.BANANASHOTGUN)
                .prefab as BananaShotgun;
        if (shotgunPrefab == null) return;

        // Instantiate the shotgun prefab and add it to the player's weapons
        Weapon[] shotgunInstance = { Instantiate(shotgunPrefab), Instantiate(shotgunPrefab) };
        AddWeapon(shotgunInstance);

        EquipWeapon(_playerWeapons[PlayerWeaponTypes.BANANASHOTGUN]); // Equip the new shotguns
        RaiseWeapon(PlayerWeaponTypes.BANANASHOTGUN);

        var playerShotgunsAmmo = _playerGunAmmo[AmmoTypes.Banana] - 2;
        SetPlayerGunsAmmo(AmmoTypes.Banana, playerShotgunsAmmo);
    }

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

    private void OnTriggerEnter(Collider other)
    {
        var weapon = other.GetComponentsInChildren<Weapon>();

        if (weapon is not null)
        {
            AddWeapon(weapon);
            SwitchToWeapon(weapon.First().WeaponType);
        }
    }
}