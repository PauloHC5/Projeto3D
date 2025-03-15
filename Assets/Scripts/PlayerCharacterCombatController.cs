using UnityEngine;
using System.Linq;

public class PlayerCharacterCombatController : PlayerCharacterMovementController
{
    [Space]
    [Header("Combat")]
    [SerializeField] protected PlayerWeapon weaponSelected;
    [SerializeField] protected Weapon[] weapons = new Weapon[5];
    [SerializeField] protected GameObject playerMesh;

    protected Weapon equippedWeapon;
    public Weapon EquippedWeapon => equippedWeapon;    

    protected virtual void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (equippedWeapon) Destroy(equippedWeapon.gameObject);

        weaponSelected = weapon;
        Weapon weaponToSpawn = weapons[(int)weapon];

        if (weaponSelected == PlayerWeapon.SHOTGUNS) equippedWeapon = SetDualWieldGun(weaponToSpawn);
        else
        {
            Transform socketToAttach = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(weaponToSpawn.GetSocketToAttach.ToString()));
            equippedWeapon = Instantiate(weaponToSpawn, socketToAttach);
        }

        if (equippedWeapon) equippedWeapon.transform.localPosition = Vector3.zero;

        PlaySwitchToWeapon(weapon);
    }

    private Gun SetDualWieldGun(Weapon weaponToSpawn)
    {
        DualWieldGun guns = new GameObject("DualWieldGun").AddComponent<DualWieldGun>();
        guns.transform.SetParent(transform);

        Transform socketRight = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunR));
        Transform socketLeft = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunL));

        guns.Initialize(
        (Gun)Instantiate(weaponToSpawn, socketRight),
        (Gun)Instantiate(weaponToSpawn, socketLeft)
        );

        return guns;
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
            HandleDualWieldGun(equippedGuns, WhichGun.GunL);
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
        PlayReload();        
    }

    protected void UseWeaponGadget()
    {
        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldGun(equippedGuns, WhichGun.GunR);
            return;
        }

        equippedWeapon.GetComponent<ISecondaryAction>()?.Perform();
    }    

    private void HandleDualWieldGun(DualWieldGun equippedGuns, WhichGun whichGun)
    {
        if(whichGun == WhichGun.GunL && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunL);
        if(whichGun == WhichGun.GunR && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunR);
    }
}
