using UnityEngine;

public class PlayerCharacterAnimationsController
{
    private Animator playerAnimator;

    private readonly int CurrentSpeed = Animator.StringToHash("CurrentSpeed");
    private readonly int UseWeaponTrigger = Animator.StringToHash("UseWeapon");
    private readonly int RaiseWeaponTrigger = Animator.StringToHash("RaiseWeapon");
    private readonly int WeaponIndex = Animator.StringToHash("WeaponIndex");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");                 
    private readonly int Toggle = Animator.StringToHash("Toggle");
    private readonly int AutoReload = Animator.StringToHash("AutoReload");
    private readonly int FireBoth = Animator.StringToHash("FireBoth");

    public PlayerCharacterAnimationsController(Animator animator)
    {
        playerAnimator = animator;
    }

    public void CheckAutoReload(int gunMagAmmo, int gunMagCapacity, int playerAmmo)
    {
        bool autoReloadCondition = gunMagAmmo < gunMagCapacity && playerAmmo > 0;
        playerAnimator.SetBool(AutoReload, autoReloadCondition);
    }

    public void HandleLocomotion(float playerVelocityMagnitude, float playerMaxSpeed)
    {
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(playerVelocityMagnitude, 0f, playerMaxSpeed));        
    }    
        
    public void PlaySwitchToWeapon(WeaponTypes weapon)
    {                                          
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);
        playerAnimator.SetTrigger(RaiseWeaponTrigger);        
    }    

    public void PlayUseWeapon()
    {        
        playerAnimator.SetTrigger(UseWeaponTrigger);                
    }

    public void PlayFireBoth()
    {
        playerAnimator.SetTrigger(FireBoth);
    }

    public void PlayReload()
    {
        playerAnimator.SetTrigger(ReloadTrigger);        
    }
    
    public void WeaponAltternation(bool toggle)
    {
        playerAnimator.SetBool(Toggle, toggle);
    }
}
