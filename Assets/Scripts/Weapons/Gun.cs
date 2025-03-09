using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{

    [Header("Gun Properties")]
    [SerializeField] private float fireRate = 0.5f;
    public float FireRate { get { return fireRate; } }

    private bool canFire = true;
    public bool CanFire { get { return canFire; } }

    private Animator gunAnimator;

    private void Awake()
    {
        gunAnimator = GetComponent<Animator>();
    }    

    public virtual void Fire()
    {
        StartCoroutine(ShootDelay());        
        gunAnimator.SetTrigger("Fire");
    }

    public virtual void Reload()
    {
        gunAnimator.SetTrigger("Reload");
    }

    private IEnumerator ShootDelay()
    {
        canFire = false;
        yield return new WaitForSeconds(FireRate);
        canFire = true;
    }

}

public interface ISecondaryAction
{
    bool Perform();
}
