using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private PlayerCharacterController playerCharacter;
    private Gun equippedGun;        

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
        playerCharacter = animator.GetComponentInParent<PlayerCharacterController>();
        equippedGun = playerCharacter.EquippedWeapon as Gun;
        DualWieldGun equippedGuns = equippedGun as DualWieldGun;        

        playerCharacter.PlayerCombatStates = FindCombatState(stateInfo);

        switch (playerCharacter.PlayerCombatStates)
        {
            case PlayerCombatStates.RAISING:                
                break;
            case PlayerCombatStates.ATTACKING:
                HandleAttackState(animator);
                break;
            case PlayerCombatStates.FIRING:
                equippedGun.Fire();
                break;
            case PlayerCombatStates.DUALWIELDFIRING:                 
                if(equippedGuns) HandleDualWieldState(equippedGuns, animator, stateInfo, layerIndex);
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

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {                
        if (playerCharacter && playerCharacter.PlayerCombatStates == PlayerCombatStates.ATTACKING)
        {
            DisableCrowbarCollision();            
        }

        if (playerCharacter && playerCharacter.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING)
        {
            
            Debug.Log("Resetting layer weights");
        }

        animator.SetLayerWeight(layerIndex, 0f);        
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

    private void SetPlayerState(PlayerCombatStates state)
    {
        playerCharacter.PlayerCombatStates = state;
    }

    private void HandleAttackState(Animator animator)
    {
        SetPlayerState(PlayerCombatStates.ATTACKING);
        var attackVar = animator.GetInteger("AttackVar") + 1;
        if (attackVar > 3) attackVar = 1;
        animator.SetInteger("AttackVar", attackVar);

        Crowbar crowbar = playerCharacter.EquippedWeapon as Crowbar;
        if (crowbar != null)
        {
            crowbar.EnableCollision();
        }
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

        if (stateInfo.IsTag("Reload"))
        {
            equippedGuns.PlayReload();                      
        }
    }

    private void DisableCrowbarCollision()
    {
        Crowbar crowbar = playerCharacter.EquippedWeapon as Crowbar;
        if (crowbar != null)
        {
            crowbar.DisableCollision();
        }
    }    
}