using UnityEngine;

public class PlayerInspectWeaponBehaviour : StateMachineBehaviour
{   
    PlayerCharacterCombatController _playerCharacterCombatController;
    PlayerCharacterController _playerCharacterController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        _playerCharacterController = animator.GetComponentInParent<PlayerCharacterController>();
        
        HUDManager.Disable();
        _playerCharacterController.SwitchPlayerControlType(PlayerControlTypes.DISABLED);
    }         
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        _playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        _playerCharacterController = animator.GetComponentInParent<PlayerCharacterController>();
        
        var weaponTutorialType = _playerCharacterCombatController.EquippedWeapon?.WeaponType switch
        {
            PlayerWeaponTypes.CARNIVOROUSPLANTS => WeaponTutorialType.CARNIVOROUSPLANT,
            PlayerWeaponTypes.ACORNGUN => WeaponTutorialType.ACORN,
            PlayerWeaponTypes.BANANASHOTGUN => WeaponTutorialType.BANANASHOTGUN,
            PlayerWeaponTypes.CACTUSSCROSSBOW => WeaponTutorialType.CACTUSCROSSBOW,            
            _ => WeaponTutorialType.NONE  
        };

        _playerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);

        if(!GameManager.SkipPlayerTutorial) TutorialManager.PlayTutorial(weaponTutorialType);
    }    
}
