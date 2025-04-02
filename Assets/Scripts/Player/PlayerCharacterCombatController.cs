using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

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

public class PlayerCharacterCombatController : PlayerCharacterMovementController
{
    [Space]
    [Header("Combat")]
    [SerializeField] protected PlayerWeapon weaponSelected;
    public PlayerWeapon WeaponSelected => weaponSelected;

    [SerializeField] protected Weapon[] weapons = new Weapon[5];    

    protected Weapon equippedWeapon;
    public Weapon EquippedWeapon => equippedWeapon;

    [SerializeField]
    private List<WeaponAmmoPair> weaponAmmoList = new List<WeaponAmmoPair>
    {
        new WeaponAmmoPair { weapon = PlayerWeapon.Crowbar, ammo = 0 }, // Crowbar doesn't use ammo
        new WeaponAmmoPair { weapon = PlayerWeapon.ACornGun, ammo = 50 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Shotgun, ammo = 20 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Thompson, ammo = 300 },
        new WeaponAmmoPair { weapon = PlayerWeapon.Crossbow, ammo = 150 }
    };

    protected Dictionary<PlayerWeapon, Int32> playerWeaponAmmo;
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

    public static event Action<PlayerWeapon> onSwitchToWeapon;
    public static event Action<PlayerWeapon> onReload;

    protected void Awake()
    {
        InitializeWeapons();
        InitializeWeaponAmmo();        
    }

    private void InitializeWeapons()
    {        
        // Fill the weapons array with the weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            // Load the weapon prefab from the resources folder
            Weapon weaponToSpawn = Resources.Load<Weapon>($"Weapons/{(PlayerWeapon)i}");

            if(weaponToSpawn.WeaponType == PlayerWeapon.Shotgun) // Shotgun is a dual wield weapon, so we need to be handled differently
            {
                weapons[i] = InitializeDualWieldGun(weaponToSpawn); // Set the dual wield gun
                continue; // Skip the rest of the loop
            }
            // Get the socket to attach the weapon
            Transform socketToAttach = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(weaponToSpawn.GetSocketToAttach.ToString()));

            // Instantiate the weapon and set it as inactive
            weapons[i] = Instantiate(weaponToSpawn, socketToAttach);
            weapons[i].gameObject.SetActive(false);
        }
    }

    private Gun InitializeDualWieldGun(Weapon weaponToSpawn)
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

        return guns;
    }

    private void InitializeWeaponAmmo()
    {
        playerWeaponAmmo = weaponAmmoList.ToDictionary(pair => pair.weapon, pair => pair.ammo);
    }        

    protected virtual void SwitchToWeapon(PlayerWeapon weapon)
    {
        // If there is an active weapon, disable it
        if (equippedWeapon) equippedWeapon.gameObject.SetActive(false);

        weaponSelected = weapon; // Set the weapon selected to the weapon passed as parameter
        equippedWeapon = weapons[(int)weaponSelected]; // Set the equipped weapon to the weapon selected

        // If equipped weapon is a dual wield gun, enable both guns
        if (equippedWeapon is DualWieldGun equippedGuns) equippedGuns.gameObject.SetActive(true);
        else equippedWeapon.gameObject.SetActive(true); // Enable the equipped weapon

        // Reset the weapon position
        if (equippedWeapon) equippedWeapon.transform.localPosition = Vector3.zero;

        PlaySwitchToWeapon(weapon);
        onSwitchToWeapon?.Invoke(weaponSelected);
    }

    

    private Transform GetSocketTransform(WeaponSocket socket)
    {
        return playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject.CompareTag(socket.ToString()));
    }

    protected void UseWeapon()
    {
        var equippedGun = equippedWeapon.GetComponent<Gun>();        

        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldGunFire(equippedGuns, WhichGun.GunL);
            return;
        }
        else if (equippedGun)
        {
            if (!equippedGun.CanFire) return;
        }        

        PlayUseWeapon();
    }

    protected virtual void Reload()
    {
        if (equippedWeapon is DualWieldGun equippedGuns) HandleDualWieldGunReload(equippedGuns);
        else
        {
            Gun equippedGun = equippedWeapon as Gun;

            // IF can't reload, return
            if (!CanReload(equippedGun)) return;

            PlayReload();
            onReload?.Invoke(weaponSelected);
        }
    }

    private void HandleDualWieldGunReload(DualWieldGun equippedGuns)
    {
        // Get the guns to reload
        Gun[] gunsToReload = new Gun[] { equippedGuns.GetGun(WhichGun.GunR), equippedGuns.GetGun(WhichGun.GunL) };

        // If GunR or GunL can't reload, return
        if (!gunsToReload.Any(gun => CanReload(gun))) return;

        PlayReload();
    }

    protected void UseWeaponGadget()
    {
        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldGunFire(equippedGuns, WhichGun.GunR);
            return;
        }

        equippedWeapon.GetComponent<ISecondaryAction>()?.Perform();
    }    

    private void HandleDualWieldGunFire(DualWieldGun equippedGuns, WhichGun whichGun)
    {
        if(whichGun == WhichGun.GunL && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunL);
        if(whichGun == WhichGun.GunR && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunR);
    }    

    private bool CanReload(Gun equippedGun)
    {
        // If the Mag Ammo is equal to the Max Ammo, means that the gun is full
        // If the weapon ammo is less than or equal to 0, means that player has no ammo to reload
        return equippedGun.MagAmmo != equippedGun.MaxAmmo && playerWeaponAmmo[weaponSelected] > 0;
    }
}
