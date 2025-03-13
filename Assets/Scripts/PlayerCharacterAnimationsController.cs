using UnityEngine;

public class PlayerCharacterAnimationsController : PlayerCharacterControllerMovement
{
    [Space]
    [Header("Animation")]
    [SerializeField] protected Animator playerAnimator;

    private int UseWeaponTrigger = Animator.StringToHash("UseWeapon");
    private int RaiseWeaponTrigger = Animator.StringToHash("RaiseWeapon");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");
    private int ReloadTrigger = Animator.StringToHash("Reload");
    private int ShootR = Animator.StringToHash("ShootR");
    private int ShootL = Animator.StringToHash("ShootL");


    protected override void HandleMovement(Vector3 playerMovementInput)
    {
        base.HandleMovement(playerMovementInput);

        if (playerAnimator) playerAnimator.SetFloat("CurrentSpeed", Mathf.Clamp(characterController.velocity.magnitude, 0f, 10f));
    }

    protected virtual void SwitchToWeapon(PlayerWeapon weapon)
    {        
        if (equippedWeapon)
        {            
            playerAnimator.SetInteger(WeaponIndex, (int)weaponSelected);
            playerAnimator.SetTrigger(RaiseWeaponTrigger);
        }
    }    

    protected void UseWeapon()
    {
        if (equippedWeapon) playerAnimator.SetTrigger(UseWeaponTrigger);
    }

    protected virtual void HandleDualWieldAction(DualWieldGun equippedGuns, WhichGun whichGun)
    {
        if (whichGun == WhichGun.GunL)
        {
            playerAnimator.SetLayerWeight(1, 1f);
            playerAnimator.SetTrigger(ShootL);
        }
        else
        {
            playerAnimator.SetLayerWeight(2, 1f);
            playerAnimator.SetTrigger(ShootR);
        }
    }

    protected virtual void Reload()
    {
        if (playerStates == PlayerStates.RELOADING) return;
        if (equippedWeapon is Gun)
        {
            playerAnimator.SetTrigger(ReloadTrigger);
        }
    }
}
