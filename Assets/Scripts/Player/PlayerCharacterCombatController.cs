using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Collections;

public enum PlayerCombatStates
{
    RAISING,
    RELOADING,
    ATTACKING,
    FIRING,
    DUALWIELDFIRING,
    CHARGING,

    DEFAULT
}

public enum WeaponSocket
{
    RightHandSocket,
    LeftHandSocket    
}

[RequireComponent(typeof(PlayerGunAmmoInitializer))]
public class PlayerCharacterCombatController : MonoBehaviour
{    
    [SerializeField] private WeaponTypes weaponSelected;    
    [SerializeField] private Weapon[] weaponsSet = new Weapon[5];
    [SerializeField] private Transform rightHandSocket, leftHandSocket;    

    private IWeapon equippedWeapon;
    private List<IWeapon> weaponsInventory = new List<IWeapon>();
    private Dictionary<AmmoTypes, Int32> playerGunAmmos;
    private PlayerCombatStates playerCombatStates = PlayerCombatStates.RAISING;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;
    private MouseLook mouseLook;
    private PlayerGunAmmoInitializer playerGunAmmoComponent;


    public WeaponTypes WeaponSelected => weaponSelected;
    public int WeaponsInventoryCount => weaponsInventory.Count;
    public IWeapon EquippedWeapon => equippedWeapon;
    public Dictionary<AmmoTypes, Int32> WeaponAmmo
    {
        get => playerGunAmmos;

        set
        {
            // set only of value is greater than 0
            foreach (var pair in value)
            {
                if (pair.Value >= 0) playerGunAmmos[pair.Key] = pair.Value;
                if(pair.Value < 0) playerGunAmmos[pair.Key] = 0;
            }
        }
    }
    public PlayerCombatStates PlayerCombatStates
    {
        get { return playerCombatStates; }
        set { playerCombatStates = value; }
    }
    public PlayerCharacterAnimationsController PlayerCharacterAnimationsController => playerCharacterAnimationsController;

    public static event Action onSwitchToWeapon;               

    private void Awake()
    {
        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
        mouseLook = GetComponentInChildren<MouseLook>();
        playerGunAmmoComponent = GetComponent<PlayerGunAmmoInitializer>();

        InitializeWeapons();
        InitializeWeaponAmmo();        
    }

    private void Start()
    {
        // Switch to the first weapon in the inventory
        if (weaponsInventory.Count > 0)
        {
            SwitchToWeapon(0);            
        }
        else
        {
            Debug.LogWarning("No weapons in the inventory.");
        }
    }

    private void Update()
    {
        // if k button is pressed, add 3 to playerWeaponAmmo[weaponSelected]
        if (equippedWeapon is IEquippedGun equippedGun)
        {            
            if (Keyboard.current.kKey.wasPressedThisFrame) playerGunAmmos[equippedGun.AmmoType] += 3;

            playerCharacterAnimationsController.CheckAutoReload(equippedGun.MagAmmo, equippedGun.MagCapacity, playerGunAmmos[equippedGun.AmmoType]);
        }                
    }

    private void InitializeWeapons()
    {
        if (rightHandSocket == null || leftHandSocket == null)
        {
            Debug.LogError("Right hand socket or left hand socket is not set.");
            return;
        }        

        // Intialize each weapon based on the weapons setted in the inspector
        foreach (var weapon in weaponsSet)
        {
            if(weapon == null) continue; // Skip if the weapon is null

            IWeapon weaponSpawned; // Will hold the spawned weapon

            // If the weapon is a DualWieldGun, it needs to be initialized differently
            if (weapon is CarnivovrousPlant carnivovrousPlant)
            {                
                weaponSpawned = new DualWieldMeleeManager(carnivovrousPlant, rightHandSocket, leftHandSocket, playerCharacterAnimationsController);
            }
            else if(weapon is Gun gun && gun.WeaponType == WeaponTypes.Shotgun)
            {
                weaponSpawned = new DualWieldGunManager(gun, rightHandSocket, leftHandSocket, playerCharacterAnimationsController);                
            }
            else // Proceed with the normal weapon initialization
            {
                // Get the socket to attach the weapon
                Transform socketToAttach = GetSocketTransform(weapon.GetSocketToAttach);

                // Instantiate the weapon and set it as inactive
                weaponSpawned = Instantiate(weapon, socketToAttach);
            }

            // disaable the weapon spawned gameobject
            weaponSpawned.DisableWeapon();

            if (weaponSpawned == null)
            {
                Debug.LogError($"Weapon {weapon.name} could not be spawned.");                
            }

            // add the weapon spawned to the weapons inventory
            weaponsInventory.Add(weaponSpawned);
        }
    }    

    private void InitializeWeaponAmmo()
    {
        playerGunAmmos = playerGunAmmoComponent.WeaponAmmoList.ToDictionary(pair => pair.ammoType, pair => pair.AmmoAmount);
    }        

    public virtual void SwitchToWeapon(WeaponTypes weaponToSwitch)
    {                        
        if(equippedWeapon != null) equippedWeapon.DisableWeapon(); // Disable the currently equipped weapon

        switch (weaponToSwitch)
        {
            case WeaponTypes.Melee:
                // Find which weapon in the inventory is a melee weapon
                equippedWeapon = weaponsInventory.Find(w => w.WeaponType == WeaponTypes.Melee);
                break;
            case WeaponTypes.Pistol:
                // Find which weapon in the inventory is a pistol
                equippedWeapon = weaponsInventory.Find(w => w.WeaponType == WeaponTypes.Pistol);
                break;
            case WeaponTypes.Shotgun:
                // Find which weapon in the inventory is a shotgun
                equippedWeapon = weaponsInventory.Find(w => w?.WeaponType == WeaponTypes.Shotgun);
                if (equippedWeapon == null) RetrieveNewShotguns();                
                break;
            case WeaponTypes.Crossbow:
                // Find which weapon in the inventory is a crossbow
                equippedWeapon = weaponsInventory.Find(w => w?.WeaponType == WeaponTypes.Crossbow);
                break;

            default:
                Debug.LogWarning($"Weapon {weaponToSwitch} not found or its not supported.");
                return; // If the weapon type is not supported, do nothing
        }        
                
        weaponSelected = equippedWeapon.WeaponType; // Set the weapon selected to the equipped weapon type

        equippedWeapon.EnableWeapon(); // Enable the equipped weapon                

        playerCharacterAnimationsController.PlaySwitchToWeapon(weaponSelected); // Play the switch to weapon animation

        onSwitchToWeapon?.Invoke();
    }        

    public void PerformPrimaryAction()
    {
        // Check if the equipped weapon implements the IEquippedGun interface
        if (equippedWeapon is IEquippedGun equippedGun && ConditionsToFire(equippedGun))
        {
            if (equippedGun.MagAmmo > 0)
            {
                equippedGun.Fire();
                playerCharacterAnimationsController.PlayUseWeapon();
            }
            else
            {
                PerformReload(); // If the weapon has no ammo, perform a reload
            }            
        }
        else if (equippedWeapon is IEquippedMelee equippedMelee && equippedMelee.CanAttack)
        {
            equippedMelee.Attack();
        }
    }

    private bool ConditionsToFire(IEquippedGun equippedGun) =>
        playerCombatStates != PlayerCombatStates.RELOADING &&        
        playerCombatStates != PlayerCombatStates.RAISING &&
        playerCombatStates != PlayerCombatStates.FIRING &&
        playerCombatStates != PlayerCombatStates.CHARGING &&
        equippedGun.CanFire;

    private Transform GetSocketTransform(WeaponSocket weaponSocketToAttach)
    {
        return weaponSocketToAttach == WeaponSocket.RightHandSocket ? rightHandSocket : leftHandSocket;
    }        

    public void PerformReload()
    {
        if(equippedWeapon is IEquippedGun equippedGun && ConditionsToReload(equippedGun))
        {
            equippedGun.PerformReload();
            playerCharacterAnimationsController.PlayReload();
            if(mouseLook) mouseLook.ZoomOut(); // Zoom out the camera if the player is reloading a gun
        }
    }

    private bool ConditionsToReload(IEquippedGun equippedGun) =>
        equippedGun.CanReload() &&
        playerGunAmmos[equippedGun.AmmoType] > 0 &&
        playerCombatStates != PlayerCombatStates.RELOADING &&
        playerCombatStates != PlayerCombatStates.FIRING;

    public void Reload()
    {        
        if(equippedWeapon is IEquippedGun equippedGun)
        {
            int equippedGunAmmo = playerGunAmmos[equippedGun.AmmoType];
            equippedGun.Reload(ref equippedGunAmmo);
            playerGunAmmos[equippedGun.AmmoType] = equippedGunAmmo;
        }
    }
       
    public void ChargeWeapon(bool buttomPressed)
    {
        var equippedGun = equippedWeapon as IEquippedGun;

        if (ConditionsToCharge(equippedGun) && equippedWeapon is IChargeable chargeableWeapon)
        {
            chargeableWeapon.PerformCharge(buttomPressed);
            playerCharacterAnimationsController.ChargeWeapon(buttomPressed);
        }        
    }

    private bool ConditionsToCharge(IEquippedGun equippedGun) =>
        equippedGun != null &&
        equippedGun is IChargeable &&
        equippedGun.CanFire &&
        equippedGun.MagAmmo == equippedGun.MagCapacity &&
        playerCombatStates != PlayerCombatStates.RELOADING &&
        playerCombatStates != PlayerCombatStates.ATTACKING &&
        playerCombatStates != PlayerCombatStates.RAISING;


    public void PerformSecondaryAction()
    {   
        if(equippedWeapon is DualWieldGunManager dualWieldGun && ConditionsToSuperFire(dualWieldGun))
        {
            dualWieldGun.FireBoth();
            playerCharacterAnimationsController.PlayFireBoth();
            GetComponent<PlayerCharacterMovementController>().ApplyImpulse(20f); // Apply backward impulse from camera when firing both guns
            return;
        }        

        if (equippedWeapon is ISecondaryAction equippedGun) equippedGun.Perform();
    }

    private bool ConditionsToSuperFire(IEquippedGun equippedGun) =>
        playerCombatStates != PlayerCombatStates.RELOADING &&
        playerCombatStates != PlayerCombatStates.RAISING &&
        playerCombatStates != PlayerCombatStates.FIRING &&
        playerCombatStates != PlayerCombatStates.CHARGING &&
        equippedGun.CanFire &&
        equippedGun.MagAmmo == equippedGun.MagCapacity;

    private void DropShotgun()
    {        
        weaponsInventory[2] = null; // Assuming Shotgun is at index 2

        equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {                
        DualWieldGunManager shotguns = new DualWieldGunManager(weaponsSet[2] as BananaShotgun, rightHandSocket, leftHandSocket, playerCharacterAnimationsController);
        shotguns.EnableWeapon();

        weaponsInventory[2] = shotguns;

        equippedWeapon = shotguns;
        var playerShotgunsAmmo = playerGunAmmos[shotguns.AmmoType];
        shotguns.Reload(ref playerShotgunsAmmo);
        playerGunAmmos[shotguns.AmmoType] = playerShotgunsAmmo;
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