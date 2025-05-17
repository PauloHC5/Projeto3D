using UnityEngine;

public enum WhichPlant
{
    PlantR,
    PlantL
}

public class CarnivorousPlants : Weapon
{
    private CarnivovrousPlant plantR;
    private CarnivovrousPlant plantL;    

    public CarnivorousPlants Initialize(CarnivovrousPlant plantR, CarnivovrousPlant plantL)
    {
        if (plantR == null) throw new System.ArgumentNullException(nameof(plantR));
        if (plantL == null) throw new System.ArgumentNullException(nameof(plantL));

        this.plantR = plantR;
        this.plantL = plantL;

        weaponType = PlayerWeapon.Melee;

        return this;
    }

    public new WeaponSocket GetSocketToAttach(WhichPlant whichPlant)
    {
        return whichPlant == WhichPlant.PlantR ? WeaponSocket.RightHandSocket : WeaponSocket.LeftHandSocket;
    }

    public void Attack()
    {
        plantR.Attack();
        plantL.Attack();        
    }

    public void Attack(WhichPlant whichPlant)
    {
        if (whichPlant == WhichPlant.PlantL)
        {
            plantL.Attack();
        }
        else
        {
            plantR.Attack();
        }        
    }    

    public void EnableCollisions()
    {
       plantL.EnableCollision();
       plantR.EnableCollision();
    }

    private void OnDestroy()
    {
        if (plantR) Destroy(plantR.gameObject);
        if (plantL) Destroy(plantL.gameObject);
    }

    public CarnivovrousPlant GetPlant(WhichPlant whichPlant)
    {
        return whichPlant == WhichPlant.PlantR ? this.plantR : this.plantL;
    }

    private void OnEnable()
    {
        if (plantR) plantR.gameObject.SetActive(true);
        if (plantL) plantL.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (plantR) plantR.gameObject.SetActive(false);
        if (plantL) plantL.gameObject.SetActive(false);
    }
}
