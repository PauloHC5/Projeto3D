using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Collections;

[Serializable]
public struct WeaponAmmoPair
{
    public WeaponTypes weapon;
    public Int32 ammo;
}

public enum PlayerCombatStates
{
    RAISING,
    RELOADING,
    ATTACKING,
    FIRING,
    DUALWIELDFIRING,

    DEFAULT
}

public enum WeaponSocket
{
    RightHandSocket,
    LeftHandSocket    
}

public class PlayerCharacterCombatController : MonoBehaviour
{    
    [SerializeField] private WeaponTypes weaponSelected;    
    [SerializeField] private Weapon[] weaponsSet = new Weapon[5];
    [SerializeField] private Transform rightHandSocket, leftHandSocket;    
   
    [SerializeField]
    private List<WeaponAmmoPair> weaponAmmoList = new List<WeaponAmmoPair>
    {
        new WeaponAmmoPair { weapon = WeaponTypes.Melee, ammo = 0 },
        new WeaponAmmoPair { weapon = WeaponTypes.Pistol, ammo = 50 },
        new WeaponAmmoPair { weapon = WeaponTypes.Shotgun, ammo = 20 },
        new WeaponAmmoPair { weapon = WeaponTypes.Smg, ammo = 300 },
        new WeaponAmmoPair { weapon = WeaponTypes.Crossbow, ammo = 150 }
    };

    private IWeapon equippedWeapon;
    private List<IWeapon> weaponsInventory = new List<IWeapon>();
    private Dictionary<WeaponTypes, Int32> playerGunAmmo;
    private PlayerCombatStates playerCombatStates = PlayerCombatStates.DEFAULT;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;
    private MouseLook mouseLook;
    

    public WeaponTypes WeaponSelected => weaponSelected;
    public int WeaponsInventoryCount => weaponsInventory.Count;
    public object EquippedWeapon => equippedWeapon;
    public Dictionary<WeaponTypes, Int32> WeaponAmmo
    {
        get => playerGunAmmo;

        set
        {
            // set only of value is greater than 0
            foreach (var pair in value)
            {
                if (pair.Value >= 0) playerGunAmmo[pair.Key] = pair.Value;
                if(pair.Value < 0) playerGunAmmo[pair.Key] = 0;
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
        if (Keyboard.current.kKey.wasPressedThisFrame) playerGunAmmo[weaponSelected] += 3;

        playerCharacterAnimationsController.CheckAutoReload(
            equippedWeapon is IEquippedGun equippedGun ? equippedGun.MagAmmo : 0,
            equippedWeapon is IEquippedGun gun ? gun.MagCapacity : 0,
            playerGunAmmo[weaponSelected]
        );
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
        playerGunAmmo = weaponAmmoList.ToDictionary(pair => pair.weapon, pair => pair.ammo);
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
            equippedGun.Fire();
            playerCharacterAnimationsController.PlayUseWeapon();            
        }
        else if (equippedWeapon is IEquippedMelee equippedMelee)
        {
            equippedMelee.Attack();            
        }
    }

    private bool ConditionsToFire(IEquippedGun equippedGun) =>
        playerCombatStates != PlayerCombatStates.RELOADING &&        
        playerCombatStates != PlayerCombatStates.RAISING &&        
        equippedGun.CanFire &&
        equippedGun.MagAmmo > 0;

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
        playerGunAmmo[weaponSelected] > 0 &&
        playerCombatStates != PlayerCombatStates.RELOADING &&
        playerCombatStates != PlayerCombatStates.FIRING;        

    public void Reload()
    {        
        IEquippedGun equippedGun = (IEquippedGun)equippedWeapon;

        int ammoAmountToReload = equippedGun.MagCapacity - equippedGun.MagAmmo; // Calculate the ammo to reload
        if (playerGunAmmo[weaponSelected] < ammoAmountToReload) // If the ammo to reload is greater than the player ammo
        {
            ammoAmountToReload = playerGunAmmo[weaponSelected]; // Set the ammo to reload to the player ammo
        }

        equippedGun.MagAmmo += ammoAmountToReload; // Set the mag ammo to the ammo to reload
        playerGunAmmo[weaponSelected] -= ammoAmountToReload; // Subtract the ammo from the player ammo
    }            
        

    public void PerformSecondaryAction()
    {   
        if(equippedWeapon is DualWieldGunManager dualWieldGun && ConditionsToFire(dualWieldGun))
        {
            dualWieldGun.FireBoth();
            playerCharacterAnimationsController.PlayFireBoth();
            GetComponent<PlayerCharacterMovementController>().ApplyImpulse(20f); // Apply backward impulse from camera when firing both guns
            return;
        }

        if (equippedWeapon is ISecondaryAction equippedGun) equippedGun.Perform();
    }    

    private void DropShotgun()
    {        
        weaponsInventory[2] = null; // Assuming Shotgun is at index 2

        equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {                
        IWeapon shotguns = new DualWieldGunManager(weaponsSet[2] as BananaShotgun, rightHandSocket, leftHandSocket, playerCharacterAnimationsController);
        shotguns.EnableWeapon();

        weaponsInventory[2] = shotguns;

        equippedWeapon = shotguns;
        playerGunAmmo[WeaponTypes.Shotgun] -= 2;
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