using UnityEngine;

public class Woodsman : Enemy
{    
    [Header("Woodsman Properties")]
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private GameObject weapon;
    private void Awake()
    {
        OnAwake();
        
        // Set the attack rate in the behavior graph blackboard
        if (!behaviorGraph.BlackboardReference.SetVariableValue("AttackRate", attackRate))
        {
            Debug.LogWarning("Woodsman: Blackboard variable 'AttackRate' not found. Please ensure it is set in the Behavior Graph.");
        }
        
        if (_damageColliderEvents != null)
        {
    }
    protected override void Die(PlayerWeaponTypes damageType)
    {
        base.Die(damageType);

        if (weapon)
        {
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon.GetComponent<Collider>().enabled = true;            

            // Desatch weapon from player
            weapon.transform.SetParent(null);            
        }             
    }
}
