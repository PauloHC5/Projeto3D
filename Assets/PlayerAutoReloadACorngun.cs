using UnityEngine;

public class PlayerAutoReloadACorngun : StateMachineBehaviour
{    
    private readonly int AutoReload = Animator.StringToHash("AutoReload");    

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        PlayerCharacterCombatController playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();

        if (playerCharacterCombatController == null || playerCharacterCombatController.WeaponSelected != WeaponTypes.Pistol) return;

        var equippedGun = playerCharacterCombatController.EquippedWeapon as IEquippedGun;

        if (equippedGun?.MagAmmo == 0 && animator.GetBool(AutoReload))
        {                     
            equippedGun?.PerformReload();
        }
    }    
}
