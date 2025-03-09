using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidbodyMovement : PlayerCharacter
{    
    [SerializeField] private Rigidbody PlayerBody;

    private Vector2 playerMovementInput;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        MovePlayer();        
    }    

    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(playerMovementInput * speed);
        PlayerBody.velocity = new Vector3(MoveVector.x, PlayerBody.velocity.y, MoveVector.z);        

        if(Input.GetButtonDown("Jump") && isGrounded) PlayerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
