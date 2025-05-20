using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private static PlayerCharacterCombatController playerCharacterCombatController;
    private Gun equippedGun;                

    private const int FireLeftHandLayer = 1;
    private const int FireRightHandLayer = 2;

    private readonly int ToggleAttack = Animator.StringToHash("ToggleAttack");
    private readonly int CanReload = Animator.StringToHash("CanReload");

    private PlayerCombatStates FindCombatState(AnimatorStateInfo stateInfo)
    {     
        if (stateInfo.IsTag("RaiseWeapon")) return PlayerCombatStates.RAISING;
        if (stateInfo.IsTag("Attack")) return PlayerCombatStates.ATTACKING;        
        if (stateInfo.IsTag("Fire")) return PlayerCombatStates.FIRING;        
        if (stateInfo.IsTag("FireR") || stateInfo.IsTag("FireL")) return PlayerCombatStates.DUALWIELDFIRING;        
        if (stateInfo.IsTag("Reload")) return PlayerCombatStates.RELOADING;
        return PlayerCombatStates.DEFAULT;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerCharacterCombatController == null)
        {            
            playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        }                

        PlayerCombatStates newCombatState = FindCombatState(stateInfo);

        // Condition to prevent state change to default when one of the shoot layers is active
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING && newCombatState == PlayerCombatStates.DEFAULT)
        {
            // Check if one of the shoot layers is active by checking its weight
            if (animator.GetLayerWeight(FireLeftHandLayer) != 0f || animator.GetLayerWeight(FireRightHandLayer) != 0f) 
                return; // Prevent state change to default if both shoot layers are active
        }

        playerCharacterCombatController.PlayerCombatStates = newCombatState;        

        equippedGun = playerCharacterCombatController.EquippedWeapon as Gun;
        DualWieldGun equippedGuns = equippedGun as DualWieldGun;
        CarnivorousPlants carnivorousPlants = playerCharacterCombatController.EquippedWeapon as CarnivorousPlants;

        switch (playerCharacterCombatController.PlayerCombatStates)
        {
            case PlayerCombatStates.RAISING:
                animator.SetLayerWeight(FireLeftHandLayer, 0f);
                animator.SetLayerWeight(FireRightHandLayer, 0f);

                if (playerCharacterCombatController.WeaponSelected == WeaponTypes.Pistol && equippedGun.MagAmmo == 0)
                {
                    
                    playerCharacterCombatController.Reload();
                    
                }

                if (carnivorousPlants != null)
                {
                    carnivorousPlants.EnableCollisions();

                    carnivorousPlants.PlayRaiseWeapon();
                }

                animator.SetBool(CanReload, false);

                break;
            case PlayerCombatStates.ATTACKING:
                if (carnivorousPlants != null)
                {
                    carnivorousPlants.EnableCollisions();

                    switch (animator.GetInteger(ToggleAttack))
                    {
                        case 1:
                            carnivorousPlants.Attack(WhichPlant.PlantR);
                            break;
                        case 2:
                            carnivorousPlants.Attack(WhichPlant.PlantL);
                            break;
                    }
                }
                break;
            case PlayerCombatStates.FIRING:
                equippedGun.Fire();
                animator.SetBool(CanReload, false);
                break;
            case PlayerCombatStates.DUALWIELDFIRING:
                if (equippedGuns) HandleDualWieldState(equippedGuns, animator, stateInfo, layerIndex);
                animator.SetBool(CanReload, false);
                break;
            case PlayerCombatStates.RELOADING:
                if (equippedGuns) HandleDualWieldState(equippedGuns, animator, stateInfo, layerIndex);
                else
                {
                    equippedGun.PlayReload();
                }
                break;            
        }

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerCharacterCombatController == null)
        {
            playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        }

        if(playerCharacterCombatController && stateInfo.IsTag("Reload")) playerCharacterCombatController.PlayerCombatStates = PlayerCombatStates.RELOADING;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {                
        if (playerCharacterCombatController && playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.ATTACKING)
        {
            DisableCrowbarCollision();            
        }        

        animator.SetLayerWeight(layerIndex, 0f);

        if(playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING &&            
            animator.GetLayerWeight(1) == 0f &&
            animator.GetLayerWeight(2) == 0f)
        {
            if(playerCharacterCombatController.PlayerCombatStates != PlayerCombatStates.RELOADING) playerCharacterCombatController.PlayerCombatStates = PlayerCombatStates.DEFAULT;
            animator.SetBool(CanReload, true);

        }        
        else
        {
            animator.SetBool(CanReload, true);
        }

        if(stateInfo.IsTag("Reload")) playerCharacterCombatController.PlayerCombatStates = PlayerCombatStates.DEFAULT;
    }        
    

    private void HandleDualWieldState(DualWieldGun equippedGuns, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        animator.SetLayerWeight(layerIndex, 1f);

        if (stateInfo.IsTag("FireL"))
        {            
            equippedGuns.Fire(WhichGun.GunL);
        }

        if (stateInfo.IsTag("FireR"))
        {            
            equippedGuns.Fire(WhichGun.GunR);
        }

        if (stateInfo.IsTag("Reload") && playerCharacterCombatController.PlayerCombatStates != PlayerCombatStates.DUALWIELDFIRING)
        {
            equippedGuns.PlayReload();                      
        }
    }

    private void DisableCrowbarCollision()
    {
        Crowbar crowbar = playerCharacterCombatController.EquippedWeapon as Crowbar;
        if (crowbar != null)
        {
            crowbar.DisableCollision();
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}