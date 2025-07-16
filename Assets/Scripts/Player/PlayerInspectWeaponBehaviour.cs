using UnityEngine;

public class PlayerInspectWeaponBehaviour : StateMachineBehaviour
{   
    PlayerCharacterCombatController _playerCharacterCombatController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        HUDManager.Disable();
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.DISABLED);
    }         
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        _playerCharacterCombatController = animator.GetComponentInParent<PlayerCharacterCombatController>();
        
        var weaponTutorialType = _playerCharacterCombatController.EquippedWeapon?.WeaponType switch
        {
            PlayerWeaponTypes.CARNIVOROUSPLANTS => WeaponTutorialType.CARNIVOROUSPLANT,
            PlayerWeaponTypes.ACORNGUN => WeaponTutorialType.ACORN,
            PlayerWeaponTypes.BANANASHOTGUN => WeaponTutorialType.BANANASHOTGUN,
            PlayerWeaponTypes.CACTUSSCROSSBOW => WeaponTutorialType.CACTUSCROSSBOW,            
            _ => WeaponTutorialType.NONE  
        };

        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);

        if(!GameManager.SkipPlayerTutorial) TutorialManager.PlayTutorial(weaponTutorialType);
    }    
}
