using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GAME_CONTENT.Scripts.Enemy
{
    // TODO make use of object pooling for spawning enemies!!!!
    // TODO implement more enemies
    
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        [Header("Spawner Settings")] 
        public List<GameObject> m_enemyTypes;
        public List<int> m_enemyTypeMaxNum;
        public float m_spawnRadius = 150.0f;
        public int m_maxOnScreen = 50;
        public int m_maxSpawnNum = 5;
        public int m_currEnemyNum = 0;

        private Dictionary<int, List<GameObject>> m_enemyPool;
        
        [Header("Sounds")] 
        [SerializeField] private List<AudioClip> m_deathSFX;
        private AudioSource m_audioSource;

        private bool canSpawnEnemies = false;
        private float m_spawnTimeOut;
        private bool timeoutFinished = true;
        private bool enemyCountConstraint = true;
        private HashSet<GameObject> m_enemies;
        private GameObject m_player;
        private float m_spawnTimer = 0.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            m_audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            m_enemies = new HashSet<GameObject>();
            m_player = GameObject.FindGameObjectWithTag("Player");

            m_enemyPool = new Dictionary<int, List<GameObject>>();

            // Initializing pool of enemies
            for (int i = 0; i < m_enemyTypes.Count; i++)
            {
                m_enemyPool[i] = new List<GameObject>();
                for (int k = 0; k < m_enemyTypeMaxNum[i]; k++)
                {
                    GameObject enemy = Instantiate(m_enemyTypes[i], gameObject.transform, true);
                    enemy.SetActive(false);
                    m_enemyPool[i].Add(enemy);
                }
            }
        }

        private void Update()
        {
            m_spawnTimer += Time.deltaTime;

            if (timeoutFinished && m_player && canSpawnEnemies)
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
                m_spawnTimeOut = Random.Range(0.5f, 2.0f);
            }
            else
            {
                m_spawnTimeOut = Random.Range(5.0f, 10.0f);
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
                Vector3 spawnPos = new Vector3(randomDirection.x + playerPos.x, 1.5f, randomDirection.y + playerPos.z);
                Collider[] results = Physics.OverlapSphere(spawnPos, 6.0f);
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
                    // TODO make this scalable with more enemy types...

                    // int roll = Random.Range(0, 10);
                    // GameObject enemyPrefab;
                    // if (roll > 6)
                    // {
                    //     if (m_enemyTypes.Count > 1)
                    //     {
                    //         enemyPrefab = m_enemyTypes[1]; // SUV type
                    //     }
                    //     else
                    //     {
                    //         enemyPrefab = m_enemyTypes[0];
                    //     }
                    // }
                    // else
                    // {
                    //     enemyPrefab = m_enemyTypes[0]; // Regular
                    // }
                    //
                    // GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    // m_enemies.Add(enemy);
                    // m_currEnemyNum++;

                    int roll = Random.Range(0, 10);
                    
                    // Logic for choosing cop variant
                    if (roll > 8)
                    {
                        m_currEnemyNum += SpawnCop(2, spawnPos);
                    }
                    else if (roll > 6)
                    {
                        m_currEnemyNum += SpawnCop(3, spawnPos);
                    }
                    else if (roll > 4)
                    {
                        m_currEnemyNum += SpawnCop(1, spawnPos);
                    }
                    else
                    {
                        m_currEnemyNum += SpawnCop(0, spawnPos);
                    }
                }
            }
        }
        
        public void DestroyedCop(int enemyType, GameObject enemyObj)
        {
            m_currEnemyNum--;
            // Debug.LogError("Cop died, now the number is: " + m_currEnemyNum);
            if (m_currEnemyNum == 0)
            {
                StopCoroutine(SpawnSequence());
                timeoutFinished = true;
            }
            enemyObj.SetActive(false);
            enemyObj.transform.SetParent(gameObject.transform);
        }

        public void SetCanSpawn()
        {
            canSpawnEnemies = true;
        }

        private int SpawnCop(int enemyType, Vector3 spawnPos)
        {
            List<GameObject> enemyList = m_enemyPool[enemyType];
            while (enemyList.Count <= 0 && enemyType >= 0)
            {
                enemyType--;
                enemyList = m_enemyPool[enemyType];
            }
            
            foreach (var enemy in enemyList)
            {
                if (!enemy.activeInHierarchy)
                {
                    enemy.transform.SetParent(null);
                    enemy.transform.position = spawnPos;

                    Vector3 orgPos = new Vector3(spawnPos.x, 0.0f, spawnPos.z);
                    Vector3 targetPos = new Vector3(m_player.transform.position.x, 0.0f,
                        m_player.transform.position.z);
                        
                    Quaternion faceDirection = Quaternion.LookRotation((targetPos - orgPos).normalized);
                    enemy.transform.rotation = faceDirection;
                    enemy.SetActive(true);
                    return 1;
                }
            }

            if (enemyType < 0)
            {
                Debug.LogError("Enemy Type is not assigned correctly for this level");
            }
            
            Debug.Log("Ran out of enemies to spawn");
            return 0;
        }

        public void PlayDeathSFX(Vector3 position)
        {
            float dist = 0.0f;
            if (m_player)
            {
                dist = Vector3.Distance(m_player.transform.position, position);
            }
            float audioVolume = 1.0f + (-0.005f * dist);

            AudioClip toPlay = m_deathSFX[Random.Range(0, m_deathSFX.Count)];
            m_audioSource.pitch = Random.Range(0.6f, 1.25f);
            m_audioSource.volume = Mathf.Clamp(audioVolume, 0.0f, 1.0f);
            m_audioSource.PlayOneShot(toPlay);
        }
    }
}