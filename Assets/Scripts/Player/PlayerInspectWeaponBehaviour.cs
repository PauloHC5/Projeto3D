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
            PlayerWeaponTypes.CARNIVOROUSPLANTS => WeaponTutorialType.CARNIVOROUSPLANT,
            PlayerWeaponTypes.ACORNGUN => WeaponTutorialType.ACORN,
            PlayerWeaponTypes.BANANASHOTGUN => WeaponTutorialType.BANANASHOTGUN,
            PlayerWeaponTypes.CACTUSSCROSSBOW => WeaponTutorialType.CACTUSCROSSBOW,            
            _ => WeaponTutorialType.NONE  
        };

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);

        TutorialManager.PlayTutorial(weaponTutorialType);
    }    
}
