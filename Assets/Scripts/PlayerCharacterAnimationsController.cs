using UnityEngine;

public class PlayerCharacterAnimationsController : MonoBehaviour
{
    
    [SerializeField] private Animator playerAnimator;

    private int CurrentSpeed = Animator.StringToHash("CurrentSpeed");
    private int UseWeaponTrigger = Animator.StringToHash("UseWeapon");
    private int RaiseWeaponTrigger = Animator.StringToHash("RaiseWeapon");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");
    private int ReloadTrigger = Animator.StringToHash("Reload");
    private int ShootR = Animator.StringToHash("ShootR");
    private int ShootL = Animator.StringToHash("ShootL");    


    public void HandleMovement(float speed)
    {        
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(speed, 0f, 10f));
    }

    protected void PlaySwitchToWeapon(PlayerWeapon weapon)
    {                                    
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);
        playerAnimator.SetTrigger(RaiseWeaponTrigger);        
    }    

    protected void PlayUseWeapon()
    {        
        playerAnimator.SetTrigger(UseWeaponTrigger);
    }
    
    protected void PlayUseWeapon(WhichGun whichGun)
    {
        if (whichGun == WhichGun.GunR) playerAnimator.SetTrigger(ShootR);
        else playerAnimator.SetTrigger(ShootL);
    }

    protected void PlayReload()
    {                
        playerAnimator.SetTrigger(ReloadTrigger);        
    }
}
