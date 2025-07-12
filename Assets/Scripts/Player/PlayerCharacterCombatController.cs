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
    public PlayerWeaponTypes WeaponSelected => _weaponSelected;

    [SerializeField] private Transform _rightHandSocket, _leftHandSocket;

    // Weapon prefabs and ammo amounts for the guns
    // This section is only visible in the Unity Editor for easy configuration
    // if you need access player weapons or ammo amounts in runtime, use _playerWeaponsSet and _playerGunAmmo
    [Header("Weapons Prefabs and Ammo")]
#if UNITY_EDITOR    
    [SerializeField] public List<WeaponPrefab> WeaponsPrefabs;
    [SerializeField] public GunAmmo[] GunsAmmo;
#endif    

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
    public PlayerCharacterAnimationsController PlayerCharacterAnimationsController => _playerCharacterAnimationsController;

    public static event Action onSwitchToWeapon;

    private void Awake()
    {
        _playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
        _mouseLook = GetComponentInChildren<MouseLook>();

        InitializePlayerWeapons();

        _playerGunAmmo = GunsAmmo.ToDictionary(g => g.AmmoType, g => g.AmmoAmount);
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
            AddWeapon(weaponPrefab.prefab);
        }
    }

    private void AddWeapon(Weapon newWeapon)
    {
        // Check if the weapon is null
        if (newWeapon == null)
        {
            Debug.LogWarning("Cannot add a null weapon to the player's weapons.");
            return; // If the weapon is null, do nothing
        }

        // Check if the weapon is already in the player's weapons
        if (_playerWeapons.ContainsKey(newWeapon.WeaponType) && _playerWeapons[newWeapon.WeaponType] is not null)
        {
            Debug.LogWarning($"Weapon {newWeapon.WeaponType} is already in the player's weapons.");
            return; // If the weapon is already in the player's weapons, do nothing
        }

        // Instantiate the weapon and attach it to the player's sockets

        var instantiatedWeapon = Instantiate(newWeapon) as IWeapon;

        if (instantiatedWeapon is CarnivovrousPlant carnivovrousPlant)
        {
            var dualWieldMelee = new DualWieldMeleeManager(
                instantiatedWeapon as CarnivovrousPlant,
                _playerCharacterAnimationsController
            );
            dualWieldMelee.AttatchToSocket(_rightHandSocket, _leftHandSocket); // Attach the dual wield melee to the sockets
            instantiatedWeapon = dualWieldMelee; // Replace the instantiated weapon with the dual wield melee manager
        }
        else
           if (newWeapon is BananaShotgun)
        {
            var dualWieldGun = new DualWieldGunManager(
                instantiatedWeapon as BananaShotgun,
                _playerCharacterAnimationsController
            );
            dualWieldGun.AttatchToSocket(_rightHandSocket, _leftHandSocket); // Attach the dual wield gun to the sockets
            instantiatedWeapon = dualWieldGun; // Replace the instantiated weapon with the dual wield gun manager            
        }
        else
            instantiatedWeapon.AttatchToSocket(_rightHandSocket, _leftHandSocket); // Attach the weapon to the sockets


        // Check if there is a null weapon in the player's weapons
        // If there is, replace it with the new weapon
        if (_playerWeapons.Any(w => w.Value is null))
        {
            // If there is a null weapon in the player's weapons, replace it with the new weapon
            var nullWeaponIndex = _playerWeapons.FirstOrDefault(w => w.Value == null).Key;
            _playerWeapons[nullWeaponIndex] = instantiatedWeapon;
            return; // Exit after replacing the null weapon
        }
        else // If there is no null weapon, add the new weapon to the player's weapons
            _playerWeapons.Add(instantiatedWeapon.WeaponType, instantiatedWeapon);
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
        if (!enabled) return;
        if (!ConditionToSwitchWeapon(weaponToSwitch)) return;
        
        if (_playerWeapons.TryGetValue(weaponToSwitch, out var weapon))
            RaiseWeapon(weaponToSwitch);
    }

    public void SwitchToWeapon(int index)
    {
        if (!enabled) return;
        if (index < 0 || index >= _playerWeapons.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {index}. Cannot switch to weapon.");
            return; // If the index is out of bounds, do nothing
        }

        var weaponList = _playerWeapons.ToList();
        var weapon = weaponList.ElementAtOrDefault(index).Value;

        if (weapon == null)
        {
            RetrieveNewShotguns();
            weapon = _playerWeapons[PlayerWeaponTypes.BANANASHOTGUN];
        }

        if (weapon != null && ConditionToSwitchWeapon(weapon.WeaponType))
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
        weaponToSwitch != _weaponSelected;


    private void RaiseWeapon(PlayerWeaponTypes weaponToRaise)
    {
        if (_playerWeapons.TryGetValue(weaponToRaise, out var weaponToEquip))
            EquipWeapon(weaponToEquip);

        _playerCharacterAnimationsController?.PlayRaiseWeapon(_weaponSelected);
        onSwitchToWeapon?.Invoke();
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

        _weaponSelected = _equippedWeapon.WeaponType;
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
        // Set the value to null instead of removing the key
        if (_playerWeapons.ContainsKey(PlayerWeaponTypes.BANANASHOTGUN))
        {
            _playerWeapons[PlayerWeaponTypes.BANANASHOTGUN] = null;
        }
        _equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {
        BananaShotgun shotgunPrefab = WeaponsPrefabs.FirstOrDefault(w => w.prefab.WeaponType == PlayerWeaponTypes.BANANASHOTGUN).prefab as BananaShotgun;
        if (shotgunPrefab == null) return;

        AddWeapon(shotgunPrefab);

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
}