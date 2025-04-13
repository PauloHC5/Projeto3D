using UnityEngine;

public abstract class PlayerCharacterAnimationsController : PlayerCharacterCombatController
{
    
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
        
        playerAnimator.SetTrigger(UseWeaponTrigger);            

        if(weaponSelected == PlayerWeapon.Crowbar) HandleToggleAttackAnimation();
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
