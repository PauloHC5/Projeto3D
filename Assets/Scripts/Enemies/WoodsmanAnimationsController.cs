using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class WoodsmanAnimationsController : EnemyAnimationsControlller
    {
        private readonly int Attack = Animator.StringToHash("Attack");

        public WoodsmanAnimationsController(Animator enemyAnimator, NavMeshAgent enemyAgent) : base(enemyAnimator, enemyAgent) 
        {
        }
    }
}