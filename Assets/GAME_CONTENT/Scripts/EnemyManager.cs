using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GAME_CONTENT.Scripts
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        [Header("Spawner Settings")] 
        public List<GameObject> m_enemyTypes;
        /*public GameObject m_enemy_reg;
        public GameObject m_enemy_suv;*/
        public float m_spawnRadius = 150.0f;
        public int m_maxOnScreen = 50;
        
        public int m_maxSpawnNum = 5;
        public int m_currEnemyNum = 0;
        

        private float m_spawnTimeOut;
        private bool timeoutFinished = true;
        private bool enemyCountConstraint = true;
        private HashSet<GameObject> m_enemies;
        private GameObject m_player;

        private float m_spawnTimer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            m_spawnTimer = 0.0f;
        }

        private void Start()
        {
            m_enemies = new HashSet<GameObject>();
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            m_spawnTimer += Time.deltaTime;

            if (timeoutFinished && m_player)
            {
                m_spawnTimer = 0.0f;
                if (m_currEnemyNum != 0)
                {
                    m_maxSpawnNum += Random.Range(1, 3);
                }
                m_maxSpawnNum = Mathf.Clamp(m_maxSpawnNum, 1, m_maxOnScreen);
                StartCoroutine(SpawnSequence());
            }
        }

        IEnumerator SpawnSequence()
        {
            SpawnEnemies();
            if (m_currEnemyNum == 0)
            {
                m_spawnTimeOut = Random.Range(0.5f, 1.0f);
            }
            else if (m_currEnemyNum <= 5)
            {
                m_spawnTimeOut = Random.Range(2.5f, 5.0f);
            }
            else
            {
                m_spawnTimeOut = Random.Range(25.0f, 45.0f);
            }
            timeoutFinished = false;
            yield return new WaitForSeconds(m_spawnTimeOut);
            timeoutFinished = true;
        }

        private void SpawnEnemies()
        {
            Vector3 playerPos = m_player.transform.position;

            int spawnNum = Random.Range(1, m_maxSpawnNum + 1);
            for (int i = 0; i < spawnNum; i++)
            {
                // Find a valid location to spawn 
                bool validPosition = true;
                Vector2 randomDirection = Random.insideUnitCircle.normalized * m_spawnRadius;
                Vector3 spawnPos = new Vector3(randomDirection.x + playerPos.x, 1.0f, randomDirection.y + playerPos.z);
                Collider[] results = Physics.OverlapSphere(spawnPos, 5.0f);
                // Debug.LogError(results.Length);
                if (results.Length > 1)
                {
                    validPosition = false;
                }
                else if (!Physics.Raycast(spawnPos, Vector3.down, 10.0f, 1 << 7))
                {
                    validPosition = false;
                }
                
                if (validPosition)
                {
                    // Randomly choose enemy type
                    int roll = Random.Range(0, 10);
                    GameObject enemyPrefab;
                    if (roll > 6)
                    {
                        enemyPrefab = m_enemyTypes[1];
                    }
                    else
                    {
                        enemyPrefab = m_enemyTypes[0];
                    }
                    
                    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    m_enemies.Add(enemy);
                    m_currEnemyNum++;
                }
            }
        }
        
        public void DestroyedCop()
        {
            m_currEnemyNum--;
            // Debug.LogError("Cop died, now the number is: " + m_currEnemyNum);
            if (m_currEnemyNum == 0)
            {
                StopCoroutine(SpawnSequence());
                timeoutFinished = true;
            }
        }
    }
}