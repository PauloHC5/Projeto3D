using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private static PlayerCharacterController playerCharacter;
    private Gun equippedGun;    

    private readonly int AttackAlternation = Animator.StringToHash("AttackAlternation");

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
        if (playerCharacter == null)
        {            
            playerCharacter = animator.GetComponentInParent<PlayerCharacterController>();
        }                

        PlayerCombatStates newCombatState = FindCombatState(stateInfo);

        // Condition to prevent state change to default when one of the shoot layers is active
        if (playerCharacter.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING && newCombatState == PlayerCombatStates.DEFAULT)
        {
            // Check if one of the shoot layers is active by checking its weight
            if (animator.GetLayerWeight(1) != 0f || animator.GetLayerWeight(2) != 0f) 
                return; // Prevent state change to default if both shoot layers are active
        }

        playerCharacter.PlayerCombatStates = newCombatState;

        equippedGun = playerCharacter.EquippedWeapon as Gun;
        DualWieldGun equippedGuns = equippedGun as DualWieldGun;                                
                
        switch (playerCharacter.PlayerCombatStates)
        {
            case PlayerCombatStates.RAISING:
                animator.SetLayerWeight(1, 0f);
                animator.SetLayerWeight(2, 0f);
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

        animator.SetLayerWeight(layerIndex, 0f);        
    }    

    private void SetPlayerState(PlayerCombatStates state)
    {
        playerCharacter.PlayerCombatStates = state;
    }

    private void HandleAttackState(Animator animator)
    {        
        SetPlayerState(PlayerCombatStates.ATTACKING);
        var alternateAttack = animator.GetInteger(AttackAlternation) + 1;
        if (alternateAttack > 3) alternateAttack = 1;
        animator.SetInteger(AttackAlternation, alternateAttack);

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