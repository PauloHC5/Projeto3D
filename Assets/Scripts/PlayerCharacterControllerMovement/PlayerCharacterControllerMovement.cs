using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterControllerMovement : PlayerCharacter
{    
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] protected CharacterController characterController;

    private Vector2 playerMovementInput;    
    Vector3 velocity;    

    // Update is called once per frame
    void Update()
    {
        if (lmbPressed) PerformPrimaryAction();
        if (rmbPressed && weaponSelected == PlayerWeapon.SHOTGUNS) PerformSecondaryAction();

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);        

        if (isGrounded && velocity.y < 0) velocity.y = -2f;        

        playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector3 move = transform.right * playerMovementInput.x + transform.forward * playerMovementInput.y;
        characterController.Move(move * speed * Time.deltaTime);

        if (playerAnimator) playerAnimator.SetFloat("CurrentSpeed", Mathf.Clamp(characterController.velocity.magnitude, 0f, 10f));

        if (Input.GetButtonDown("Jump") && isGrounded) velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);        
    }    

}
