using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private PlayerCharacter playerCharacter;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        playerCharacter = animator.GetComponentInParent<PlayerCharacter>();

        if(stateInfo.IsTag("RaiseWeapon"))
        {
            playerCharacter.PlayerStates = PlayerStates.RAISING;            
        }

        if (stateInfo.IsTag("Attack"))
        {
            playerCharacter.PlayerStates = PlayerStates.ATTACKING;
            var attackVar = animator.GetInteger("AttackVar") + 1;
            if (attackVar > 3) attackVar = 1;

            animator.SetInteger("AttackVar", attackVar);

            Crowbar crowbar = playerCharacter.EquippedWeapon as Crowbar;
            crowbar.EnableCollision();
        }

        if (stateInfo.IsTag("DualWieldFire") || stateInfo.IsTag("Fire"))
        {
            playerCharacter.PlayerStates = PlayerStates.FIRING;            
        }

        if (stateInfo.IsTag("DualWieldFire"))
        {
            if(playerCharacter.LmbPressed) animator.SetLayerWeight(1, 1f);            
            if(playerCharacter.RmbPressed) animator.SetLayerWeight(2, 1f);            
        }

        if(stateInfo.IsTag("Reload"))
        {
            playerCharacter.PlayerStates = PlayerStates.RELOADING; 
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsTag("RaiseWeapon") || stateInfo.IsTag("Attack") || stateInfo.IsTag("Fire") || stateInfo.IsTag("DualWieldFire") || stateInfo.IsTag("Reload"))
        {
            playerCharacter.PlayerStates = PlayerStates.DEFAULT;            
        }        

        if (stateInfo.IsTag("Attack"))
        {
            Crowbar crowbar = playerCharacter.EquippedWeapon as Crowbar;
            crowbar.DisableCollision();
        }

        if(stateInfo.IsTag("DualWieldFire"))
        {
            animator.SetLayerWeight(1, 0f);            
            animator.SetLayerWeight(2, 0f);            
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

//animator.GetCurrentAnimatorClipInfo(0)[0].clip.name