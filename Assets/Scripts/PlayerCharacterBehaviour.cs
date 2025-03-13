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
    DUALWIELDFIRING,

    DEFAULT
}

public class PlayerCharacterBehaviour : StateMachineBehaviour
{
    private PlayerCharacterController playerCharacter;
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
        if (stateInfo.IsTag("Fire")) return PlayerStates.FIRING;        
        if (stateInfo.IsTag("FireR") || stateInfo.IsTag("FireL")) return PlayerStates.DUALWIELDFIRING;        
        if (stateInfo.IsTag("Reload")) return PlayerStates.RELOADING;
        return PlayerStates.DEFAULT;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerCharacter = animator.GetComponentInParent<PlayerCharacterController>();
        equippedGun = playerCharacter.EquippedWeapon as Gun;
        DualWieldGun equippedGuns = equippedGun as DualWieldGun;        

        playerCharacter.PlayerStates = GetAnimationState(stateInfo);

        switch (playerCharacter.PlayerStates)
        {
            case PlayerStates.RAISING:                
                break;
            case PlayerStates.ATTACKING:
                HandleAttackState(animator);
                break;
            case PlayerStates.FIRING:                            
                equippedGun.Fire();
                break;
            case PlayerStates.DUALWIELDFIRING:                 
                if(equippedGuns) HandleDualWieldState(equippedGuns, animator, stateInfo);
                break;
            case PlayerStates.RELOADING:
                if (equippedGuns) HandleDualWieldState(equippedGuns, animator, stateInfo);
                else equippedGun.Reload();                
                break;
        }        
        
    }    

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        if (playerCharacter && playerCharacter.PlayerStates == PlayerStates.ATTACKING)
        {
            DisableCrowbarCollision();            
        }

        if (playerCharacter && playerCharacter.PlayerStates == PlayerStates.FIRING)
        {
            ResetLayerWeights(animator, stateInfo);
        }

        if(playerCharacter) playerCharacter.PlayerStates = PlayerStates.DEFAULT;
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

    private void HandleDualWieldState(DualWieldGun equippedGuns, Animator animator, AnimatorStateInfo stateInfo)
    {        
        if (stateInfo.IsTag("FireL"))
        {
            if (playerCharacter.LmbPressed) animator.SetLayerWeight(1, 1f);
            equippedGuns.Fire(WhichGun.GunL);
        }

        if (stateInfo.IsTag("FireR"))
        {            
            if (playerCharacter.RmbPressed) animator.SetLayerWeight(2, 1f);
            equippedGuns.Fire(WhichGun.GunR);
        }

        if (stateInfo.IsTag("Reload"))
        {
            equippedGuns.Reload();            
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