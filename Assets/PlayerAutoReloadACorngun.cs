using UnityEngine;

public class PlayerAutoReloadACorngun : StateMachineBehaviour
{    
    private readonly int AutoReload = Animator.StringToHash("AutoReload");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        if (animator.GetBool(AutoReload))
        {
            PlayerCharacterCombatController playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
            IEquippedGun equippedGun = playerCharacterCombatController?.EquippedWeapon as IEquippedGun;

            equippedGun?.PerformReload();
        }
    }    
}
