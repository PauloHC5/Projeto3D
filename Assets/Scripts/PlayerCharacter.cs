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

public enum WeaponSocket
{
    RightHand,
    LeftHand,
    Back
}

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] protected GameObject playerMesh;    

    [Space]
    [Header("Combat")]
    [SerializeField] protected PlayerWeapon weaponSelected;
    [SerializeField] protected Weapon[] weapons = new Weapon[5];

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

    protected bool lmbPressed = false;
    protected bool rmbPressed = false;

    protected bool isGrounded;
    private Weapon equippedWeapon;
    private PlayerInputActions playerControls;
    private PlayerStates playerStates = PlayerStates.DEFAULT;    

    private int UseWeaponTrigger = Animator.StringToHash("UseWeapon");
    private int RaiseWeaponTrigger = Animator.StringToHash("RaiseWeapon");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");
    private int ReloadTrigger = Animator.StringToHash("Reload");


    public Weapon EquippedWeapon => equippedWeapon;

    public PlayerStates PlayerStates { 
        get { return playerStates; }
        set { playerStates = value; }
    }       

    public bool LmbPressed { get { return lmbPressed; } }
    public bool RmbPressed { get { return rmbPressed; } }    

    private int MouseScroll;

    private void Awake()
    {
        InitializePlayerControls();             
    }

    private void InitializePlayerControls()
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

        // faça com que Mouse Scrool seja 1 quandi o playerControls.Player.MouseScroll for para cima e -1 quando for para baixo
        playerControls.Player.MouseScrollUp.performed += ctx => { MouseScroll = -1; HandleMouseScroll(); };
        playerControls.Player.MouseScrollDown.performed += ctx => { MouseScroll = 1; HandleMouseScroll(); };        
    }

    private void Start()
    {
        SwitchToWeapon(weaponSelected);        
    }

    private void HandleMouseScroll()
    {
        int weaponCount = Enum.GetValues(typeof(PlayerWeapon)).Length;
        int newWeaponIndex = ((int)weaponSelected + MouseScroll + weaponCount) % weaponCount;
        SwitchToWeapon((PlayerWeapon)newWeaponIndex);
    }

    private void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (playerStates == PlayerStates.FIRING || playerStates == PlayerStates.ATTACKING || (weapon == weaponSelected && weapon != PlayerWeapon.CROWBAR)) return;

        if (equippedWeapon) Destroy(equippedWeapon.gameObject);

        weaponSelected = weapon;
        Weapon weaponToSpawn = weapons[(int)weapon];

        if (weaponSelected == PlayerWeapon.SHOTGUNS) equippedWeapon = SetDualWieldGun(weaponToSpawn);
        else
        {            
            Transform socketToAttach = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(weaponToSpawn.GetSocketToAttach.ToString()));
            equippedWeapon = Instantiate(weaponToSpawn, socketToAttach);
        }

        if (equippedWeapon)
        {
            equippedWeapon.transform.localPosition = Vector3.zero;
            playerAnimator.SetInteger(WeaponIndex, (int)weaponSelected);
            playerAnimator.SetTrigger(RaiseWeaponTrigger);            
        }
    }
    
    private Gun SetDualWieldGun(Weapon weaponToSpawn)
    {
        DualWieldGun guns = new GameObject("DualWieldGun").AddComponent<DualWieldGun>();
        guns.transform.SetParent(transform);

        Transform socketRight = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunR));
        Transform socketLeft = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunL));

        guns.Initialize(
        (Gun)Instantiate(weaponToSpawn, socketRight),
        (Gun)Instantiate(weaponToSpawn, socketLeft)
        );

        return guns;
    }

    private Transform GetSocketTransform(WeaponSocket socket)
    {
        return playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject.CompareTag(socket.ToString()));
    }

    protected void PerformPrimaryAction()
    {
        if (playerStates == PlayerStates.RELOADING || playerStates == PlayerStates.ATTACKING || playerStates == PlayerStates.RAISING) return;

        var equippedGun = equippedWeapon.GetComponent<Gun>();

        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldAction(equippedGuns, WhichGun.GunL);
            return;
        }        
        else if (equippedGun)
        {
            if (!equippedGun.CanFire) return;               
        }

        playerAnimator.SetTrigger(UseWeaponTrigger);
    }    

    protected void PerformSecondaryAction()
    {
        if (playerStates == PlayerStates.RAISING || playerStates ==  PlayerStates.RELOADING) return;

        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldAction(equippedGuns, WhichGun.GunR);
            return;
        }

        equippedWeapon.GetComponent<ISecondaryAction>()?.Perform();        
    }

    private void HandleDualWieldAction(DualWieldGun equippedGuns, WhichGun whichGun)
    {
        if (!equippedGuns.CanFire(whichGun)) return;

        equippedGuns.Fire(whichGun);        
        playerAnimator.SetTrigger(
            Animator.StringToHash(whichGun == WhichGun.GunR ? "ShootR" : "ShootL")
        );        
    }

    protected void Reload()
    {
        if (playerStates == PlayerStates.RELOADING) return;

        if (equippedWeapon is Gun equippedGun)
        {
            playerAnimator.SetTrigger(ReloadTrigger);            
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
