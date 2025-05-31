using UnityEngine;

public class AcorngunSuperFire : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacterCombatController playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        var equippedChargeable = playerCharacterCombatController?.EquippedWeapon as IChargeable;

        if(equippedChargeable != null) equippedChargeable.PerformSuperFire();

    }            
}
