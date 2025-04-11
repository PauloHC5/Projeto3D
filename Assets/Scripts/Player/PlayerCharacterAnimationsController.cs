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
    private int GunAmmo = Animator.StringToHash("Gun Ammo");    


    public void HandleLocomotion(float speed, float maxSpeed)
    {
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(speed, 0f, maxSpeed));        
    }

    public void HandleAmmo(int ammo)
    {
        playerAnimator.SetInteger(GunAmmo, ammo);
    }

    protected void PlaySwitchToWeapon(PlayerWeapon weapon)
    {                                    
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);
        playerAnimator.SetTrigger(RaiseWeaponTrigger);        
    }    

    protected virtual void UseWeapon()
    {        
        playerAnimator.SetTrigger(UseWeaponTrigger);
    }        

    protected void PlayReload()
    {                
        playerAnimator.SetTrigger(ReloadTrigger);        
    }
}
