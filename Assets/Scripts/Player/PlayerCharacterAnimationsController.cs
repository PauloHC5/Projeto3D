using System;
using System.Collections.Generic;
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
    private readonly int Charge = Animator.StringToHash("Charge");
    private readonly int CheckGunTrigger = Animator.StringToHash("CheckGun");

    private Dictionary<WeaponTypes, bool> _weaponChecked = new Dictionary<WeaponTypes, bool>
    {
        { WeaponTypes.Melee, false },
        { WeaponTypes.Pistol, false },
        { WeaponTypes.Shotgun, false },
        { WeaponTypes.Crossbow, false },
        { WeaponTypes.Smg, false }
    };

    public PlayerCharacterAnimationsController(Animator animator)
    {
        playerAnimator = animator;
    }

    public void CheckAutoReload(int gunMagAmmo, int gunMagCapacity, int playerAmmo)
    {
        bool autoReloadCondition = gunMagAmmo == 0 && playerAmmo > 0;
        playerAnimator.SetBool(AutoReload, autoReloadCondition);
    }

    public void HandleLocomotion(float playerVelocityMagnitude, float playerMaxSpeed)
    {
        playerAnimator.SetFloat(CurrentSpeed, Mathf.Clamp(playerVelocityMagnitude, 0f, playerMaxSpeed));        
    }    
        
    public void PlaySwitchToWeapon(WeaponTypes weapon)
    {
        playerAnimator.SetInteger(WeaponIndex, (int)weapon);

        if (_weaponChecked[weapon])        
            playerAnimator.SetTrigger(RaiseWeaponTrigger);        
        else
        {
            _weaponChecked[weapon] = true; // Mark the weapon as checked after the first use
            playerAnimator.SetTrigger(CheckGunTrigger);  
            Debug.Log($"Weapon {weapon} checked for the first time.");
        }            
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

    internal void ChargeWeapon(bool buttomPressed)
    {
        playerAnimator.SetBool(Charge, buttomPressed);
    }
}
