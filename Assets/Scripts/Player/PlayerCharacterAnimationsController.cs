using UnityEngine;

public class PlayerCharacterAnimationsController
{
    private Animator playerAnimator;

    private readonly int CurrentSpeed = Animator.StringToHash("CurrentSpeed");
    private readonly int UseWeaponTrigger = Animator.StringToHash("UseWeapon");
    private readonly int RaiseWeaponTrigger = Animator.StringToHash("RaiseWeapon");
    private readonly int WeaponIndex = Animator.StringToHash("WeaponIndex");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");
    private readonly int ShootR = Animator.StringToHash("ShootR");
    private readonly int ShootL = Animator.StringToHash("ShootL");    
    private readonly int GunAmmo = Animator.StringToHash("Gun Ammo");
    private readonly int ToggleAttack = Animator.StringToHash("ToggleAttack");

    private bool toggleFire = false;

    public PlayerCharacterAnimationsController(Animator animator)
    {
        playerAnimator = animator;
    }

    public void HandleLocomotion(float playerVelocityMagnitude, float playerMaxSpeed)
    {
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(playerVelocityMagnitude, 0f, playerMaxSpeed));        
    }

    public void HandleAmmo(int ammo)
    {
        playerAnimator.SetInteger(GunAmmo, ammo);
    }

    public void PlaySwitchToWeapon(WeaponTypes weapon)
    {                                  
        Debug.Log($"Switching to weapon: {(int)weapon}");
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);
        playerAnimator.SetTrigger(RaiseWeaponTrigger);        
    }    

    public void PlayeUseWeapon(Weapon equippedWeapon)
    {
        if (equippedWeapon is DualWieldGun)
        {
            ToggleDualWieldFire((DualWieldGun)equippedWeapon);
            return;
        }

        playerAnimator.SetTrigger(UseWeaponTrigger);        

        if(equippedWeapon?.WeaponType == WeaponTypes.Melee) HandleToggleAttackAnimation();
    }

    public void PlayFireBothGuns(DualWieldGun dualWieldGun)
    {        
        playerAnimator.SetTrigger(ShootL);
        playerAnimator.SetTrigger(ShootR);                    
    }

    private void ToggleDualWieldFire(DualWieldGun equippedGuns)
    {
        if (toggleFire == false)
        {
            if(!equippedGuns.GetGun(WhichGun.GunL).CanFire) return;

            playerAnimator.SetTrigger(ShootL);
            toggleFire = true;
        }
        else
        {
            if (!equippedGuns.GetGun(WhichGun.GunR).CanFire) return;

            playerAnimator.SetTrigger(ShootR);
            toggleFire = false;
        }
    }

    public void PlayReload()
    {
        playerAnimator.SetTrigger(ReloadTrigger);        
    }

    private void HandleToggleAttackAnimation()
    {        
        var toggleAttack = playerAnimator.GetInteger(ToggleAttack) + 1;
        if (toggleAttack > 2) toggleAttack = 1;
        playerAnimator.SetInteger(ToggleAttack, toggleAttack);
    }
}
