using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum PlayerStates
{
    RAISING,
    RELOADING,
    ATTACKING,
    FIRING,

    DEFAULT
}

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private PlayerCharacter playerCharacter;
    private Gun equippedGun;

    private enum AnimationState
    {
        None,
        RaiseWeapon,
        Attack,        
        Fire,
        FireR,
        FireL,
        Reload
    }    

    private PlayerStates GetAnimationState(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsTag("RaiseWeapon")) return PlayerStates.RAISING;
        if (stateInfo.IsTag("Attack")) return PlayerStates.ATTACKING;        
        if (stateInfo.IsTag("Fire") || stateInfo.IsTag("FireR") || stateInfo.IsTag("FireL")) return PlayerStates.FIRING;        
        if (stateInfo.IsTag("Reload")) return PlayerStates.RELOADING;
        return PlayerStates.DEFAULT;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerCharacter = animator.GetComponentInParent<PlayerCharacter>();
        equippedGun = playerCharacter.EquippedWeapon as Gun;

        switch (GetAnimationState(stateInfo))
        {
            case PlayerStates.RAISING:
                SetPlayerState(PlayerStates.RAISING);
                break;
            case PlayerStates.ATTACKING:
                HandleAttackState(animator);
                break;
            case PlayerStates.FIRING:            
                SetPlayerState(PlayerStates.FIRING);
                HandleDualWieldFireState(animator, stateInfo);                
                break;
            case PlayerStates.RELOADING:
                SetPlayerState(PlayerStates.RELOADING);                
                if (equippedGun != null) equippedGun.Reload();                
                break;
        }        
        
    }    

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        if (GetAnimationState(stateInfo) == PlayerStates.ATTACKING)
        {
            DisableCrowbarCollision();            
        }

        if (GetAnimationState(stateInfo) == PlayerStates.FIRING)
        {
            ResetLayerWeights(animator, stateInfo);
        }

        playerCharacter.PlayerStates = PlayerStates.DEFAULT;
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
        if (stateInfo.IsTag("FireL"))
        {
            if (playerCharacter.LmbPressed) animator.SetLayerWeight(1, 1f);            \            
        }

        if (stateInfo.IsTag("FireR"))
        {            
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

    private void ResetLayerWeights(Animator animator, AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsTag("FireL")) animator.SetLayerWeight(1, 0f);
        if (stateInfo.IsTag("FireR")) animator.SetLayerWeight(2, 0f);
    }
}