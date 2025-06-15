using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterCombatBehaviour : StateMachineBehaviour
{
    private static PlayerCharacterCombatController playerCharacterCombatController;    

    private const int FireLeftHandLayerIndex = 1;
    private const int FireRightHandLayerIndex = 2;
    
    private readonly int CanReload = Animator.StringToHash("CanReload");

    private PlayerCombatStates FindCombatState(AnimatorStateInfo stateInfo)
    {     
        if (stateInfo.IsTag("RaiseWeapon")) return PlayerCombatStates.RAISING;
        if (stateInfo.IsTag("Attack")) return PlayerCombatStates.ATTACKING;        
        if (stateInfo.IsTag("Fire")) return PlayerCombatStates.FIRING;        
        if (stateInfo.IsTag("FireR") || stateInfo.IsTag("FireL")) return PlayerCombatStates.DUALWIELDFIRING;        
        if (stateInfo.IsTag("Reload")) return PlayerCombatStates.RELOADING;
        if(stateInfo.IsTag("Charging")) return PlayerCombatStates.CHARGING;
        return PlayerCombatStates.DEFAULT;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerCharacterCombatController == null)
        {            
            playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
            if (playerCharacterCombatController == null)
            {
                Debug.LogWarning("PlayerCharacterCombatController not found in parent. Ensure it is attached to the player character if you want the PlayerCharacterCombatBehaviour works.");
                return;
            }
        }                

        PlayerCombatStates newCombatState = FindCombatState(stateInfo);

        // Condition to prevent state change to default when one of the shoot layers is active
        if (playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING && newCombatState == PlayerCombatStates.DEFAULT)
        {
            // Check if one of the shoot layers is active by checking its weight
            if (animator.GetLayerWeight(FireLeftHandLayerIndex) != 0f || animator.GetLayerWeight(FireRightHandLayerIndex) != 0f) 
                return; // Prevent state change to default if both shoot layers are active
        }

        playerCharacterCombatController.PlayerCombatStates = newCombatState;                

        switch (playerCharacterCombatController.PlayerCombatStates)
        {
            case PlayerCombatStates.RAISING:
                animator.SetLayerWeight(FireLeftHandLayerIndex, 0f);
                animator.SetLayerWeight(FireRightHandLayerIndex, 0f);                

                animator.SetBool(CanReload, false);

                break;
            case PlayerCombatStates.ATTACKING:                
                break;
            case PlayerCombatStates.FIRING:                                
                break;
            case PlayerCombatStates.DUALWIELDFIRING:                
                animator.SetBool(CanReload, false);

                break;
            case PlayerCombatStates.RELOADING:                  
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

        if(animator.GetLayerWeight(FireLeftHandLayerIndex) == 1f || animator.GetLayerWeight(FireRightHandLayerIndex) == 1f)
        {
            animator.SetBool(CanReload, false);
        }
        else animator.SetBool(CanReload, true);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {   
        bool isExitingFromDualWieldFiring = playerCharacterCombatController.PlayerCombatStates == PlayerCombatStates.DUALWIELDFIRING;        

        if (isExitingFromDualWieldFiring)
        {            
            playerCharacterCombatController.PlayerCombatStates = PlayerCombatStates.DEFAULT;            
        }                

        if(stateInfo.IsTag("Reload")) playerCharacterCombatController.PlayerCombatStates = PlayerCombatStates.DEFAULT;        
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