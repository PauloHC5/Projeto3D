using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

[Serializable]
public struct WeaponAmmoPair
{
    public PlayerWeapon weapon;
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
    [SerializeField] private PlayerWeapon weaponSelected;    
    [SerializeField] private Weapon[] weapons = new Weapon[5];
    [SerializeField] private Transform rightHandSocket, leftHandSocket;    
   
    [SerializeField]
    private List<WeaponAmmoPair> weaponAmmoList = new List<WeaponAmmoPair>
    {
        new WeaponAmmoPair { weapon = PlayerWeapon.Melee, ammo = 0 }, // Crowbar doesn't use ammo
        new WeaponAmmoPair { weapon = PlayerWeapon.Pistol, ammo = 50 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Shotgun, ammo = 20 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Smg, ammo = 300 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Crossbow, ammo = 150 }
    };

    private Weapon equippedWeapon;
    private Dictionary<PlayerWeapon, Int32> playerWeaponAmmo;
    private PlayerCombatStates playerCombatStates = PlayerCombatStates.DEFAULT;
    private PlayerCharacterAnimationsController playerCharacterAnimationsController;


    public PlayerWeapon WeaponSelected => weaponSelected;
    public Weapon EquippedWeapon => equippedWeapon;
    public Dictionary<PlayerWeapon, Int32> WeaponAmmo
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

    public static event Action<PlayerWeapon> onSwitchToWeapon;
    public static event Action<PlayerWeapon> onReload;

    private void Awake()
    {
        playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());

        InitializeWeapons();
        InitializeWeaponAmmo();        
    }

    private void Start()
    {
        // Try to switch to the first valid weapon that you have in the array
        foreach (Weapon weapon in weapons)
        {
            if(weapon != null)
            {
                // Switch to the first weapon that you have found
                SwitchToWeapon(Array.IndexOf(weapons, weapon));
                break; // Break the loop after switching to the first weapon
            }                               
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
        foreach (var weapon in weapons)
        {
            if(weapon == null) continue; // Skip if the weapon is null

            Weapon weaponSpawned; // Will hold the spawned weapon

            // If the weapon is a DualWieldGun, it needs to be initialized differently
            if (weapon.WeaponType == PlayerWeapon.Melee || weapon.WeaponType == PlayerWeapon.Shotgun)
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

            // Get the correct index of the weapon in the array, and set it to the spawned weapon
            // Othwerwise, it will be refferencing the prefab
            weapons[Array.IndexOf(weapons, weapon)] = weaponSpawned;
        }
    }

    private Weapon InitializeDualWieldGun(Weapon weaponToSpawn)
    {
        Weapon weapons;

        if(weaponToSpawn.WeaponType == PlayerWeapon.Melee)
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
        if (weapons[weaponsIndex] == null)
        {
            Debug.LogWarning($"There is no Weapon at index {weaponsIndex}.");
            return;
        }

        if (weapons[weaponsIndex] == equippedWeapon) return; // If the weapon is already equipped, do nothing

        // If there is an active weapon, disable it
        if (equippedWeapon) equippedWeapon.gameObject.SetActive(false);
        
        equippedWeapon = weapons[weaponsIndex]; // Set the equipped weapon using the index
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
        if (equippedWeapon is Gun equippedGun && !equippedGun.CanFire) return;

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
}
