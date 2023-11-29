using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts.Enemy;
using GAME_CONTENT.Scripts.Other;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace GAME_CONTENT.Scripts.Player
{
    public class PlayerController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {

        public static PlayerController Instance;

        [Header("Core Settings")]
        [SerializeField] private GameObject m_player;
        
        [Header("Slow motion settings")] public float m_slowDownFactor = 0.05f;
        public float m_slowDownLength = 0.75f;

        [Header("Power slide settings")] 
        public float m_launchForce = 10.0f;
        [SerializeField] private GameObject m_normalBulletPrefab;
        [SerializeField] private GameObject m_HEBulletPrefab;
        [SerializeField] private int m_pelletCount = 3;
        [SerializeField] private int m_maxBulletCount = 10;
        [SerializeField] private float m_bulletAngleOffset = 5.0f;
        [SerializeField] private float m_slideStopVelocity = 8.0f;
        [SerializeField] private float m_shootHoldThreshold = 0.1f;
        
        private GameObject currentBulletPrefab;
        
        public bool canShoot { private set; get; } = false;

        [Header("Certain ability settings")] 
        [SerializeField] private float m_sideKickRadius = 100.0f;
        [SerializeField] private float m_sideKickAttackTimeout = 2.0f;
        [SerializeField] private GameObject m_minePrefab;
        [SerializeField] private float m_mineTimeout;
        
        [Header("Sounds")]
        [SerializeField] private List<AudioClip> m_shootSFX;
        [SerializeField] private List<AudioClip> m_deathSFX;
        [SerializeField] private List<AudioClip> m_mineSFX;
        private AudioSource m_audioSource;

        private bool isInSlowMotion = false;
        private bool hasShotProjectile = false;
        private bool coroutineFinished = true;
        private bool isSideKickActive = false;
        private bool isFirstSwipe = true;
        private int m_bulletCount = 0;
        private bool isMineActivated = false;
        
        // Deprecated Ability
        private bool iSlamActive = false;

        private Ray m_pressRay;
        private Ray m_releaseRay;
        private int m_layerMask;
        
        private Matrix4x4 matrix;

        // for shooting mechanics
        private float m_holdTime = 0.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            currentBulletPrefab = m_normalBulletPrefab;
            m_audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            m_layerMask = ~((1 << 3 | 1 << 6) | 1 << 9);
            
            matrix = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 45.0f, 0.0f)); // Stock coordinate system

            m_player.GetComponent<Rigidbody>().isKinematic = true;
        }

        private void Update()
        {
            if (m_player)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_pressRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    m_player.GetComponent<Rigidbody>().isKinematic = false;
                    isInSlowMotion = true;
                }

                if (Input.GetMouseButton(0))
                {
                    m_holdTime += Time.deltaTime;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    m_releaseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    isInSlowMotion = false;
                    hasShotProjectile = true;
                    
                    if (isFirstSwipe)
                    {
                        isFirstSwipe = false;
                        GameManager.Instance.ReleaseStartPanel();
                        EnemyManager.Instance.SetCanSpawn();
                        m_player.GetComponent<PlayerAI>().AICanMove();
                    }
                }
            }
            
            // Debug.Log(m_holdTime);
        }

        private void FixedUpdate()
        {
            Vector3 pressHitPos = Vector3.zero;
            Vector3 releaseHitPos = Vector3.zero;
            
            if (Physics.Raycast(m_pressRay, out RaycastHit hitPressInfo, 500.0f, m_layerMask))
            {
                pressHitPos = hitPressInfo.point;
            }
            if (Physics.Raycast(m_releaseRay, out RaycastHit hitReleaseInfo, 500.0f, m_layerMask))
            {
                releaseHitPos = hitReleaseInfo.point;
            }
            
            float forceMagnitude = (pressHitPos - releaseHitPos).magnitude;
            
            if (isInSlowMotion && forceMagnitude > 2.0f)
            {
                DoSlowMotion();
            }
            else
            {
                if (hasShotProjectile && m_player)
                {
                    /*if (isSlamActive)
                    {
                        m_player.GetComponent<Player>().SetIsInvulnerable(true);
                    }*/
                    
                    coroutineFinished = false;
                    StartCoroutine(ShootProjectile());
                }
                
                // Slow motion recover
                if (!GameManager.Instance.isGamePaused)
                {
                    Time.timeScale += (1.0f / m_slowDownLength) * Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Clamp(Time.timeScale, 0.0f, 1.0f);
                    // Time.fixedDeltaTime = Time.timeScale * 0.02f;
                    if (Time.timeScale > 0.9f)
                    {
                        Time.timeScale = 1.0f;
                        // Time.fixedDeltaTime = 0.02f;
                    }
                }
            }

            if (m_player)
            {
                if (m_player.GetComponent<Rigidbody>().velocity.magnitude < m_slideStopVelocity 
                    && !m_player.GetComponent<NavMeshAgent>().enabled && coroutineFinished)
                {
                    /*if (isSlamActive)
                    {
                        m_player.GetComponent<Player>().SetIsInvulnerable(false);
                    }*/
                    
                    m_player.GetComponent<Rigidbody>().isKinematic = true;
                    m_player.GetComponent<NavMeshAgent>().enabled = true;
                    m_player.GetComponent<NavMeshAgent>().SetDestination(m_player.GetComponent<PlayerAI>().m_goalPosition);
                }
            }
        }

        private IEnumerator ShootProjectile()
        {
            hasShotProjectile = false;
            
            Vector3 pressHitPos = Vector3.zero;
            Vector3 releaseHitPos = Vector3.zero;
            
            if (Physics.Raycast(m_pressRay, out RaycastHit hitPressInfo, 500.0f, m_layerMask))
            {
                pressHitPos = hitPressInfo.point;
            }
            if (Physics.Raycast(m_releaseRay, out RaycastHit hitReleaseInfo, 500.0f, m_layerMask))
            {
                releaseHitPos = hitReleaseInfo.point;
            }

            Vector3 forceDirection = (pressHitPos - releaseHitPos).normalized;
            float forceMagnitude = (pressHitPos - releaseHitPos).magnitude;

            if (forceMagnitude > 2.0f)
            {
                // SHOOOOOTTTTT
                if (m_bulletCount > 0 && m_holdTime > m_shootHoldThreshold)
                {
                    AudioClip toPlay = m_shootSFX[Random.Range(0, m_shootSFX.Count)];
                    m_audioSource.pitch = Random.Range(0.6f, 1.25f);
                    m_audioSource.PlayOneShot(toPlay);
                    
                    AddBullets(-1);
                    List<GameObject> bullets = new List<GameObject>();

                    Vector3 bulletDirection = (releaseHitPos - m_player.transform.position).normalized;
                    bulletDirection.y = 0.0f;

                    float currentAngle = 0.0f;
                    Vector3 currentSpawnDirection = bulletDirection;
                    int times = 1;

                    for (int i = 0; i < m_pelletCount; i++)
                    {
                        GameObject bullet = Instantiate(currentBulletPrefab,
                            m_player.transform.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                        bullet.transform.GetComponent<Rigidbody>()
                            .AddForce(currentSpawnDirection.normalized * 30000.0f);
                        bullets.Add(bullet);

                        if (i % 2 == 0)
                        {
                            currentAngle -= m_bulletAngleOffset * (i + 1);
                            times++;
                        }
                        else
                        {
                            currentAngle += m_bulletAngleOffset * (i + 1);
                            times++;
                        }

                        currentAngle = Mathf.Repeat(currentAngle, 360.0f);
                        // Obtained with rotation matrix
                        currentSpawnDirection = new Vector3(
                            bulletDirection.x * Mathf.Cos(currentAngle * Mathf.Deg2Rad) +
                            bulletDirection.z * Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                            0.0f,
                            -bulletDirection.x * Mathf.Sin(currentAngle * Mathf.Deg2Rad) +
                            bulletDirection.z * Mathf.Cos(currentAngle * Mathf.Deg2Rad));
                    }
                }

                // Disabling nav mesh agent for applying force
                m_player.GetComponent<NavMeshAgent>().enabled = false;
                m_player.GetComponent<Rigidbody>()
                    .AddForce(forceDirection * m_launchForce * forceMagnitude, ForceMode.Impulse);
                m_holdTime = 0.0f;
            }

            yield return null;
            coroutineFinished = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // DoSlowMotion();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // StartCoroutine(EndSlowMotion());
        }

        private void DoSlowMotion()
        {
            Time.timeScale = m_slowDownFactor;
            Time.fixedDeltaTime = m_slowDownFactor * 0.02f;
        }
        
        public void PlayDeathSFX()
        {
            AudioClip toPlay = m_deathSFX[Random.Range(0, m_deathSFX.Count)];
            m_audioSource.pitch = Random.Range(0.6f, 1.25f);
            m_audioSource.PlayOneShot(toPlay);
        }

        public void PlayMineSFX()
        {
            AudioClip toPlay = m_mineSFX[Random.Range(0, m_mineSFX.Count)];
            m_audioSource.pitch = Random.Range(0.6f, 1.25f);
            m_audioSource.PlayOneShot(toPlay);
        }

        /*
         *
         *  Player associated ability section
         * 
         */
        public void SetPelletCount(int num)
        {
            canShoot = true;
            m_pelletCount = num;
            AddBullets(10);
        }

        public void AddBullets(int num)
        {
            m_bulletCount += num;
            m_bulletCount = Mathf.Clamp(m_bulletCount, 0, m_maxBulletCount);
            GameManager.Instance.ChangeBulletNum(m_bulletCount);
        }

        public void AddMaxBulletCount(int num)
        {
            m_maxBulletCount++;
        }

        public void SetBulletType(string type)
        {
            if (type == "Normal")
            {
                currentBulletPrefab = m_normalBulletPrefab;
            }
            else if (type == "HE")
            {
                currentBulletPrefab = m_HEBulletPrefab;
            }
        }

        public void ActivateSideKick()
        {
            if (!isSideKickActive)
            {
                isSideKickActive = true;
                StartCoroutine(SideKick());
            }
        }

        IEnumerator SideKick()
        {
            while (isSideKickActive)
            {
                if (m_player)
                {
                    var playerPos = m_player.transform.position;

                    // Find nearest target
                    Collider[] cols = Physics.OverlapSphere(playerPos, m_sideKickRadius);
                    if (cols.Length > 0)
                    {
                        float minDist = 100000.0f;
                        GameObject target = null;
                        foreach (var col in cols)
                        {
                            if (col.CompareTag("Enemy"))
                            {
                                GameObject enemy = col.gameObject;
                                float enemyDist = Vector3.Distance(m_player.transform.position,
                                    enemy.transform.position);
                                if (enemyDist < minDist)
                                {
                                    minDist = enemyDist;
                                    target = enemy;
                                }
                            }
                        }

                        if (target != null)
                        {
                            // Debug.Log(target);
                            Vector3 currentSpawnDirection = (target.transform.position - playerPos).normalized;
                            // Debug.Log(currentSpawnDirection);
                            GameObject bullet = Instantiate(m_normalBulletPrefab,
                                playerPos + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                            bullet.transform.GetComponent<Rigidbody>()
                                .AddForce(currentSpawnDirection.normalized * 30000.0f);
                        }
                    }

                    yield return new WaitForSeconds(m_sideKickAttackTimeout);
                }
            }
        }

        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_player.transform.position, m_sideKickRadius);
        }*/

        public void SetSlamActive()
        {
            // Nothing here lol too powerful
        }

        public void ActivateMines()
        {
            isMineActivated = true;
            StartCoroutine(SpawnMines());
        }

        public void DisableMines()
        {
            isMineActivated = false;
        }

        IEnumerator SpawnMines()
        {
            while (isMineActivated)
            {
                Instantiate(m_minePrefab, m_player.transform.position, Quaternion.identity);
                yield return new WaitForSeconds(m_mineTimeout);
            }
        }

        public int GetMaxAmmo()
        {
            return m_maxBulletCount;
        }
    }
}