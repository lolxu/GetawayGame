using System.Collections.Generic;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Enemy
{
    public class EnemyTrapper : EnemyBase
    {
        [Header("Trapper Specific Settings")] 
        [SerializeField] private GameObject m_trapPrefab;
        [SerializeField] private int m_deployAmount = 3;
        [SerializeField] private float m_deployTimeout = 1.0f;

        private List<GameObject> m_traps;
        
        private void Start()
        {
            m_hasAbilities = true;
            m_abilityTimeout = m_deployTimeout;
            m_traps = new List<GameObject>();
            for (int i = 0; i < m_deployAmount; i++)
            {
                GameObject trap = Instantiate(m_trapPrefab, transform);
                trap.SetActive(false);
                m_traps.Add(trap);
            }
            
            ActivateCopAbilities();
        }

        protected override void ActivateCopAbilities()
        {
            if (m_traps.Count > 0)
            {
                GameObject currentTrap = m_traps[0];
                currentTrap.transform.SetParent(null, worldPositionStays:true);
                currentTrap.transform.position = new Vector3(transform.position.x, 0.15f, transform.position.z);
                currentTrap.SetActive(true);
                m_traps.RemoveAt(0);
            }
        }
    }
}