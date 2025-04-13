using UnityEngine;

public abstract class PlayerCharacterAnimationsController : PlayerCharacterCombatController
{
    [Space]
    [Header("Combat")]
    [SerializeField] private Animator playerAnimator;

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

    protected void HandleLocomotion()
    {
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(PlayerVelocityMagnitude, 0f, PlayerMaxSpeed));        
    }

    protected void HandleAmmo(int ammo)
    {
        playerAnimator.SetInteger(GunAmmo, ammo);
    }

    protected override void SwitchToWeapon(PlayerWeapon weapon)
    {                                    
        base.SwitchToWeapon(weapon);
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);
        playerAnimator.SetTrigger(RaiseWeaponTrigger);        
    }    

    protected void UseWeapon()
    {
        if (equippedWeapon is Gun equippedGun && !equippedGun.CanFire) return;

        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            ToggleDualWieldFire(equippedGuns);
            return;
        }

        playerAnimator.SetTrigger(UseWeaponTrigger);            

        if(weaponSelected == PlayerWeapon.Crowbar) HandleToggleAttackAnimation();
    }

    protected override void UseWeaponGadget()
    {        
        if (equippedWeapon is DualWieldGun equippedGuns && equippedGuns.CanFire)
        {
            playerAnimator.SetTrigger(ShootL);
            playerAnimator.SetTrigger(ShootR);
        }
            
        else
            base.UseWeaponGadget();
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

    protected override void Reload()
    {
        if (equippedWeapon is Gun equippedGun && !CanReload(equippedGun)) return;

        playerAnimator.SetTrigger(ReloadTrigger);
        base.Reload();
    }

    private void HandleToggleAttackAnimation()
    {        
        var toggleAttack = playerAnimator.GetInteger(ToggleAttack) + 1;
        if (toggleAttack > 3) toggleAttack = 1;
        playerAnimator.SetInteger(ToggleAttack, toggleAttack);
    }
}
