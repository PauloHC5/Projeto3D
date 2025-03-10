using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private PlayerCharacter playerCharacter;

    private enum AnimationState
    {
        None,
        RaiseWeapon,
        Attack,
        DualWieldFire,
        Fire,
        Reload
    }

    private AnimationState GetAnimationState(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsTag("RaiseWeapon")) return AnimationState.RaiseWeapon;
        if (stateInfo.IsTag("Attack")) return AnimationState.Attack;
        if (stateInfo.IsTag("DualWieldFire")) return AnimationState.DualWieldFire;
        if (stateInfo.IsTag("Fire")) return AnimationState.Fire;
        if (stateInfo.IsTag("Reload")) return AnimationState.Reload;
        return AnimationState.None;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerCharacter = animator.GetComponentInParent<PlayerCharacter>();

        switch(GetAnimationState(stateInfo))
        {
            case AnimationState.RaiseWeapon:
                SetPlayerState(PlayerStates.RAISING);
                break;
            case AnimationState.Attack:
                HandleAttackState(animator);
                break;
            case AnimationState.DualWieldFire:
            case AnimationState.Fire:
                SetPlayerState(PlayerStates.FIRING);
                HandleDualWieldFireState(animator, stateInfo);
                break;
            case AnimationState.Reload:
                SetPlayerState(PlayerStates.RELOADING);
                break;
        }        
        
    }    

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsTag("RaiseWeapon") || stateInfo.IsTag("Attack") || stateInfo.IsTag("Fire") || stateInfo.IsTag("DualWieldFire") || stateInfo.IsTag("Reload"))
        {
            playerCharacter.PlayerStates = PlayerStates.DEFAULT;            
        }

        if (GetAnimationState(stateInfo) == AnimationState.Attack)
        {
            DisableCrowbarCollision();
        }

        if (GetAnimationState(stateInfo) == AnimationState.DualWieldFire)
        {
            ResetLayerWeights(animator);
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

    private void SetPlayerState(PlayerStates state)
    {
        playerCharacter.PlayerStates = state;
    }

    private void HandleAttackState(Animator animator)
    {
        SetPlayerState(PlayerStates.ATTACKING);
        var attackVar = animator.GetInteger("AttackVar") + 1;
        if (attackVar > 3) attackVar = 1;
        animator.SetInteger("AttackVar", attackVar);

        Crowbar crowbar = playerCharacter.EquippedWeapon as Crowbar;
        if (crowbar != null)
        {
            crowbar.EnableCollision();
        }
    }

    private void HandleDualWieldFireState(Animator animator, AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsTag("DualWieldFire"))
        {
            if (playerCharacter.LmbPressed) animator.SetLayerWeight(1, 1f);
            if (playerCharacter.RmbPressed) animator.SetLayerWeight(2, 1f);
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

    private void ResetLayerWeights(Animator animator)
    {
        animator.SetLayerWeight(1, 0f);
        animator.SetLayerWeight(2, 0f);
    }
}