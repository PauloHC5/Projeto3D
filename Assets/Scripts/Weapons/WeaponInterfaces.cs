using UnityEngine;

public interface IWeapon
{
    public PlayerWeaponTypes WeaponType { get; }

    public void EnableWeapon();

    public void DisableWeapon();
    
    public void AttatchToSocket(Transform rightHandSocket, Transform leftHandSocket);

    float WeaponRange { get; }
}

public interface IEquippedMelee
{
    public bool CanAttack { get; }

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

    AmmoTypes AmmoType { get; }

    void Reload(ref int playerGunAmmo);
}

public interface IChargeable
{
    public void PerformCharge(bool buttomPressed);

    public void PerformSuperFire();    
}
