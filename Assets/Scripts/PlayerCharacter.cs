using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerWeapon
{
    CROWBAR = 0,
    PISTOL = 1,
    SHOTGUNS = 2,
    THOMPSOM = 3,
    CROSSBOW = 4
}

public enum PlayerStates
{
    RAISING,
    RELOADING,
    ATTACKING,
    FIRING,

    DEFAULT
}

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] protected GameObject playerMesh;    

    [Space]
    [Header("Combat")]
    [SerializeField] protected PlayerWeapon weaponSelected;
    [SerializeField] protected Weapon[] weapons;        

    [Space]
    [Header("Animation")]
    [SerializeField] protected Animator playerAnimator;          

    [Space]
    [Header("Movement")]
    [SerializeField] protected float speed;
    [SerializeField] protected float jumpForce;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundDistance = 0.4f;
    [SerializeField] protected LayerMask groundMask;

    protected bool isGrounded;

    private Weapon equippedWeapon;
    public Weapon EquippedWeapon { get { return equippedWeapon; } }

    private PlayerInputActions playerControls;    
    private PlayerStates playerStates = PlayerStates.DEFAULT;
    private float defaultFoV;

    public PlayerStates PlayerStates { 
        get { return playerStates; }
        set { playerStates = value; }
    }
    
    protected bool lmbPressed = false;
    protected bool rmbPressed = false;

    public bool LmbPressed { get { return lmbPressed; } }
    public bool RmbPressed { get { return rmbPressed; } }

    private void Awake()
    {
        playerControls = new PlayerInputActions();

        playerControls.Player.PrimaryAction.started += ctx => lmbPressed = true;
        playerControls.Player.PrimaryAction.canceled += ctx => lmbPressed = false;

        playerControls.Player.SecondaryAction.started += ctx => rmbPressed = true;
        playerControls.Player.SecondaryAction.canceled += ctx => rmbPressed = false;

        playerControls.Player.SecondaryAction.performed += ctx => PerformSecondaryAction();

        playerControls.Player.Reload.performed += ctx => Reload();
        playerControls.Player.Weapon1.performed += ctx => SwitchToWeapon(PlayerWeapon.CROWBAR);
        playerControls.Player.Weapon2.performed += ctx => SwitchToWeapon(PlayerWeapon.PISTOL);        
        playerControls.Player.Weapon3.performed += ctx => SwitchToWeapon(PlayerWeapon.SHOTGUNS);        
        playerControls.Player.Weapon4.performed += ctx => SwitchToWeapon(PlayerWeapon.THOMPSOM);        
        playerControls.Player.Weapon5.performed += ctx => SwitchToWeapon(PlayerWeapon.CROSSBOW);        
    }    

    private void Start()
    {
        SwitchToWeapon(weaponSelected);
    }

    private void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (playerStates != PlayerStates.DEFAULT || playerStates == PlayerStates.FIRING || (weapon == weaponSelected && weapon != PlayerWeapon.CROWBAR)) return;

        if (equippedWeapon) Destroy(equippedWeapon.gameObject);

        weaponSelected = weapon;
        Weapon weaponToSpawn = weapons[(int)weapon];

        if (weaponSelected == PlayerWeapon.SHOTGUNS) equippedWeapon = SetDualWieldGun(weaponToSpawn);
        else
        {            
            Transform socketToAttach = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(weaponToSpawn.SocketToAttach.ToString()));
            equippedWeapon = Instantiate(weaponToSpawn, socketToAttach);
        }

        if (equippedWeapon)
        {
            equippedWeapon.transform.localPosition = Vector3.zero;
            playerAnimator.SetInteger("WeaponIndex", (int)weaponSelected);
            playerAnimator.SetTrigger("RaiseWeapon");            
        }
    }
    
    private Gun SetDualWieldGun(Weapon weaponToSpawn)
    {
        DualWieldGun guns = new GameObject("DualWieldGun").AddComponent<DualWieldGun>();
        guns.transform.SetParent(transform);    
        
        Transform socketRight = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(guns.SocketToAttach(WhichGun.GunR).ToString()));
        Transform socketLeft = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(guns.SocketToAttach(WhichGun.GunL).ToString()));

        guns.Initialize(
        (Gun)Instantiate(weaponToSpawn, socketRight),
        (Gun)Instantiate(weaponToSpawn, socketLeft)
        );

        return guns;
    }

    protected void PerformPrimaryAction()
    {
        if (playerStates == PlayerStates.RELOADING || playerStates == PlayerStates.ATTACKING || playerStates == PlayerStates.RAISING) return;

        var equippedGun = equippedWeapon.GetComponent<Gun>();

        var equippedGuns = equippedGun as DualWieldGun;

        if(equippedGuns)
        {
            if (!equippedGuns.CanFire(WhichGun.GunL)) return;
            equippedGuns.FireL();
            playerAnimator.SetTrigger("ShootL");
            return;
        }
        else if (equippedGun)
        {
            if (!equippedGun.CanFire) return;
            equippedGun.Fire();                        
        }

        playerAnimator.SetTrigger("UseWeapon");
    }    

    protected void PerformSecondaryAction()
    {
        if (playerStates != PlayerStates.DEFAULT) return;
        
        bool? performed = equippedWeapon.GetComponent<ISecondaryAction>()?.Perform();
        
        if (performed.HasValue)
        {
            if ((bool)performed && equippedWeapon as DualWieldGun) playerAnimator.SetTrigger("ShootR");            
        }        
    }

    protected void Reload()
    {
        if (playerStates != PlayerStates.DEFAULT) return;

        Gun equippedGun = equippedWeapon as Gun;

        if (equippedGun)
        {
            playerAnimator.SetTrigger("Reload");
            equippedGun.Reload();
        }             
    }

    private void OnEnable()
    {
        playerControls.Enable();        
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

}
