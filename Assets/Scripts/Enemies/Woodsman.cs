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
    private void Update()
    {
        OnUpdate();

        // Check if the weapon is not null and has a Rigidbody component
        if (weapon && weapon.GetComponent<Rigidbody>() != null)
        {
            // Ensure the weapon is kinematic while the enemy is alive
            weapon.GetComponent<Rigidbody>().isKinematic = !isDead;
            weapon.GetComponent<Collider>().enabled = !isDead;
        }
    }
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
