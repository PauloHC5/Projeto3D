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
        new WeaponAmmoPair { weapon = WeaponTypes.Melee, ammo = 0 }, // Crowbar doesn't use ammo
        new WeaponAmmoPair { weapon = WeaponTypes.Pistol, ammo = 50 },
        new WeaponAmmoPair { weapon = WeaponTypes.Shotgun, ammo = 20 },
        new WeaponAmmoPair { weapon = WeaponTypes.Smg, ammo = 300 },
        new WeaponAmmoPair { weapon = WeaponTypes.Crossbow, ammo = 150 }
    };

    private Weapon equippedWeapon;
    private List<Weapon> weaponsInventory = new List<Weapon>();
    private Dictionary<WeaponTypes, Int32> playerWeaponAmmo;
    private PlayerCombatStates playerCombatStates = PlayerCombatStates.DEFAULT;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;


    public WeaponTypes WeaponSelected => weaponSelected;
    public int WeaponsInventoryCount => weaponsInventory.Count;
    public Weapon EquippedWeapon => equippedWeapon;
    public Dictionary<WeaponTypes, Int32> WeaponAmmo
    {
        get => playerWeaponAmmo;

        set
        {
            // set only of value is greater than 0
            foreach (var pair in value)
            {
                if (pair.Value >= 0) playerWeaponAmmo[pair.Key] = pair.Value;
                if(pair.Value < 0) playerWeaponAmmo[pair.Key] = 0;
            }
        }
    }
    public PlayerCombatStates PlayerCombatStates
    {
        get { return playerCombatStates; }
        set { playerCombatStates = value; }
    }

    public static event Action<WeaponTypes> onSwitchToWeapon;
    public static event Action<WeaponTypes> onReload;

    private void Awake()
    {
        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());

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
        if (Keyboard.current.kKey.wasPressedThisFrame) playerWeaponAmmo[weaponSelected] += 3;
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

            Weapon weaponSpawned; // Will hold the spawned weapon

            // If the weapon is a DualWieldGun, it needs to be initialized differently
            if (weapon.WeaponType == WeaponTypes.Melee || weapon.WeaponType == WeaponTypes.Shotgun)
            {
                // Will create a new instance of the DualWieldGuns
                weaponSpawned = InitializeDualWieldGun(weapon);                
            }            
            else // Proceed with the normal weapon initialization
            {
                // Get the socket to attach the weapon
                Transform socketToAttach = GetSocketTransform(weapon.GetSocketToAttach);

                // Instantiate the weapon and set it as inactive
                weaponSpawned = Instantiate(weapon, socketToAttach);
                weaponSpawned.gameObject.SetActive(false);
            }

            if(weaponSpawned == null)
            {
                Debug.LogError($"Weapon {weapon.name} could not be spawned.");                
            }

            // add the weapon spawned to the weapons inventory
            weaponsInventory.Add(weaponSpawned);
        }
    }

    private Weapon InitializeDualWieldGun(Weapon weaponToSpawn)
    {
        Weapon weapons;

        if(weaponToSpawn.WeaponType == WeaponTypes.Melee)
        {
            CarnivorousPlants carnivovrousPlants = new GameObject("CarnivovrousPlants").AddComponent<CarnivorousPlants>();
            carnivovrousPlants.transform.SetParent(transform);

            Transform socketRight = GetSocketTransform(carnivovrousPlants.GetSocketToAttach(WhichPlant.PlantR));
            Transform socketLeft = GetSocketTransform(carnivovrousPlants.GetSocketToAttach(WhichPlant.PlantL));

            carnivovrousPlants.Initialize(
                (CarnivovrousPlant)Instantiate(weaponToSpawn, socketRight),
                (CarnivovrousPlant)Instantiate(weaponToSpawn, socketLeft)
            );

            weapons = carnivovrousPlants;
        }
        else
        {
            DualWieldGun guns = new GameObject("DualWieldGun").AddComponent<DualWieldGun>();
            guns.transform.SetParent(transform);

            Transform socketRight = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunR));
            Transform socketLeft = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunL));

            guns.Initialize(
            (Gun)Instantiate(weaponToSpawn, socketRight),
            (Gun)Instantiate(weaponToSpawn, socketLeft)
            );

            guns.gameObject.SetActive(false);

            weapons = guns;
        }        

        return weapons;
    }

    private void InitializeWeaponAmmo()
    {
        playerWeaponAmmo = weaponAmmoList.ToDictionary(pair => pair.weapon, pair => pair.ammo);
    }        

    public virtual void SwitchToWeapon(int weaponsIndex)
    {        
        if (weaponsInventory[weaponsIndex] == null)
        {
            Debug.LogWarning($"There is no Weapon at index {weaponsIndex}.");
            return;
        }

        if (weaponsInventory[weaponsIndex] == equippedWeapon) return; // If the weapon is already equipped, do nothing

        // If there is an active weapon, disable it
        if (equippedWeapon) equippedWeapon.gameObject.SetActive(false);
        
        equippedWeapon = weaponsInventory[weaponsIndex]; // Set the equipped weapon using the index
        weaponSelected = equippedWeapon.WeaponType;

        // If equipped weapon is a dual wield gun, enable both guns
        if (equippedWeapon is DualWieldGun equippedGuns) equippedGuns.gameObject.SetActive(true);
        else if(equippedWeapon is CarnivorousPlants carnivorousPlants) carnivorousPlants.gameObject.SetActive(true);        
        else equippedWeapon?.gameObject.SetActive(true); // Enable the equipped weapon

        // Reset the weapon position
        if (equippedWeapon) equippedWeapon.transform.localPosition = Vector3.zero;

        playerCharacterAnimationsController.PlaySwitchToWeapon(weaponSelected); // Play the switch to weapon animation

        onSwitchToWeapon?.Invoke(weaponSelected);
    }

    public void UseWeapon()
    {
        if (equippedWeapon == null || (equippedWeapon is Gun equippedGun && !equippedGun.CanFire)) return;

        playerCharacterAnimationsController.PlayeUseWeapon(equippedWeapon);
    }    

    private Transform GetSocketTransform(WeaponSocket weaponSocketToAttach)
    {
        return weaponSocketToAttach == WeaponSocket.RightHandSocket ? rightHandSocket : leftHandSocket;
    }    

    public void Reload()
    {        
        if (equippedWeapon is not Gun equippedGun || !CanReload(equippedGun)) return;

        playerCharacterAnimationsController.PlayReload();
        onReload?.Invoke(weaponSelected);
    }

    public void UseWeaponGadget()
    {        
        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            if (!equippedGuns.CanFire) return;

            playerCharacterAnimationsController.PlayFireBothGuns(equippedGuns);
            return;
        }

        equippedWeapon?.GetComponent<ISecondaryAction>()?.Perform();
    }        

    private bool CanReload(Gun equippedGun)
    {
        if(equippedGun is DualWieldGun equippedGuns)
        {
            // Get the guns to reload
            Gun[] gunsToReload = new Gun[] { equippedGuns.GetGun(WhichGun.GunR), equippedGuns.GetGun(WhichGun.GunL) };

            // If any of the guns can be reloaded, return true
            if (gunsToReload.Any(gun => CanReload(gun))) return true;
            else return false;
        }
        
        // If the Mag Ammo is equal to the Max Ammo, means that the gun is full
        // If the weapon ammo is less than or equal to 0, means that player has no ammo to reload
        return equippedGun.MagAmmo != equippedGun.MaxAmmo && playerWeaponAmmo[weaponSelected] > 0;
    }

    private void DropShotgun()
    {
        Weapon ShotgunsSpawned = InitializeDualWieldGun(weaponsSet[2]); // Assuming Shotgun is at index 2

        // Find where the shotuns are in the inventory and set them to the shotguns spawned       /
        int index = weaponsInventory.FindIndex(weapon => weapon.WeaponType == WeaponTypes.Shotgun);
        if (index != -1)
        {
            weaponsInventory[index] = ShotgunsSpawned;
        }
        else
        {
            Debug.LogWarning("Shotgun not found in the inventory.");
            return;
        }



        weaponsInventory.Remove(equippedWeapon);
        

        equippedWeapon = null;
    }

    private void RetrieveNewShotguns()
    {
        Weapon ShotgunsSpawned = InitializeDualWieldGun(weaponsSet[2]); // Assuming Shotgun is at index 2
        ShotgunsSpawned.gameObject.SetActive(true);
        weaponsInventory.Add(ShotgunsSpawned);

        equippedWeapon = ShotgunsSpawned;
    }

    private void OnEnable()
    {
        AnimationTriggerEvents.onDropShotgun += DropShotgun;
        AnimationTriggerEvents.onReTrieveNewShotguns += RetrieveNewShotguns;
    }

    private void OnDisable()
    {
        AnimationTriggerEvents.onDropShotgun -= DropShotgun;
        AnimationTriggerEvents.onReTrieveNewShotguns -= RetrieveNewShotguns;
    }
}
