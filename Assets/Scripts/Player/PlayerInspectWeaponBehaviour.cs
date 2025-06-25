using UnityEngine;

public class PlayerInspectWeaponBehaviour : StateMachineBehaviour
{    

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HUDManager.Disable();
        PlayerCharacterController.PlayerControls.Player.Move.Disable();
    }         
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HUDManager.Enable();
        PlayerCharacterController.PlayerControls.Player.Move.Enable();
    }    
}
