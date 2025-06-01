using UnityEngine;

public class CarnivorousPlantTriggerEvents : MonoBehaviour
{
    public void EnableCollision()
    {
        GetComponentInParent<CarnivovrousPlant>().EnableCollision();
    }

    public void DisableCollision()
    {
        GetComponentInParent<CarnivovrousPlant>().DisableCollision();
    }
}
