using UnityEngine;

public interface IWeapon
{
    public WeaponTypes WeaponType { get; }

    public void EnableWeapon();

    public void DisableWeapon();
}

public interface IEquippedMelee
{
    public void Attack();
}

public interface IEquippedGun
{
    int MagAmmo { get; set; }

    int MagCapacity { get; }

    void Fire();

    void PerformReload();

    bool CanFire { get; }

    bool CanReload();
}

public interface IChargeable
{
    public void PerformCharge(bool buttomPressed);

    public void PerformSuperFire();
}
