using System;
using UnityEditor;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Enemy
{
    public class EnemyShooter : EnemyBase
    {
        [Header("Shooter Specific Settings")] 
        [SerializeField] private float m_attackRadius;
        [SerializeField] private GameObject m_bulletPrefab;
        [SerializeField] private float m_shootTimeout = 2.0f;
        
        private void Start()
        {
            m_hasAbilities = true;
            m_abilityTimeout = m_shootTimeout;
        }

        protected override void ActivateCopAbilities()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, m_attackRadius);
            GameObject target = null;
            foreach (var col in cols)
            {
                if (col.CompareTag("Player"))
                {
                    target = col.gameObject;
                    break;
                }
            }
            
            if (target != null)
            {
                // Debug.Log(target);
                Vector3 currentSpawnDirection = (target.transform.position - transform.position).normalized;
                // Debug.Log(currentSpawnDirection);
                GameObject bullet = Instantiate(m_bulletPrefab,
                    transform.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                bullet.transform.GetComponent<Rigidbody>()
                    .AddForce(currentSpawnDirection.normalized * 7500.0f);
            }
        }
    }
}