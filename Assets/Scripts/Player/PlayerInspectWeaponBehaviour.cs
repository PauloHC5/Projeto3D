using UnityEngine;

public class PlayerInspectWeaponBehaviour : StateMachineBehaviour
{   
    PlayerCharacterCombatController _playerCharacterCombatController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HUDManager.Disable();
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.CUTSCENE);
    }         
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        
        //PlayerCharacterController.PlayerControls.Player.Move.Enable();
        var weaponTutorialType = _playerCharacterCombatController.WeaponSelected switch
        {
            WeaponTypes.Melee => WeaponTutorialType.CARNIVOROUSPLANT,
            WeaponTypes.Pistol => WeaponTutorialType.ACORN,
            WeaponTypes.Shotgun => WeaponTutorialType.BANANASHOTGUN,
            WeaponTypes.Crossbow => WeaponTutorialType.CACTUSCROSSBOW,            
            _ => WeaponTutorialType.NONE  
        };

        TutorialManager.PlayTutorial(weaponTutorialType);
    }    
}
