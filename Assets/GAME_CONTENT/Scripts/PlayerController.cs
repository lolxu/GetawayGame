using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace GAME_CONTENT.Scripts
{
    public class PlayerController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {

        public static PlayerController Instance;
        
        [Header("Slow motion settings")] public float m_slowDownFactor = 0.05f;
        public float m_slowDownLength = 0.75f;

        [Header("Power slide settings")] public float m_launchForce = 10.0f;
        [SerializeField] private GameObject m_bulletPrefab;
        [SerializeField] private int m_pelletCount = 3;
        [SerializeField] private int m_maxBulletCount = 10;
        [SerializeField] private float m_bulletAngleOffset = 5.0f;
        [SerializeField] private float m_slideStopVelocity = 8.0f;

        private bool isInSlowMotion = false;
        private bool hasShotProjectile = false;
        private bool coroutineFinished = true;
        private bool isSlamActive = false;
        private int m_bulletCount = 0;

        private Ray m_pressRay;
        private Ray m_releaseRay;

        private GameObject m_player;
        private int m_layerMask;
        
        private Matrix4x4 matrix;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            m_layerMask = ~(1 << 3 | 1 << 6);
            
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

                if (Input.GetMouseButtonUp(0))
                {
                    m_releaseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    isInSlowMotion = false;
                    hasShotProjectile = true;
                }
            }
            
        }

        private void FixedUpdate()
        {
            if (isInSlowMotion)
            {
                DoSlowMotion();
            }
            else
            {
                if (hasShotProjectile && m_player)
                {
                    if (isSlamActive)
                    {
                        m_player.GetComponent<Player>().SetIsInvulnerable(true);
                    }
                    
                    coroutineFinished = false;
                    StartCoroutine(ShootProjectile());
                }
                
                // Slow motion recover
                if (!UIManager.Instance.isGamePaused)
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
                    if (isSlamActive)
                    {
                        m_player.GetComponent<Player>().SetIsInvulnerable(false);
                    }
                    
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
            
            if (Physics.Raycast(m_pressRay, out RaycastHit hitPressInfo, Mathf.Infinity, m_layerMask))
            {
                pressHitPos = hitPressInfo.point;
            }
            
            if (Physics.Raycast(m_releaseRay, out RaycastHit hitReleaseInfo, Mathf.Infinity, m_layerMask))
            {
                releaseHitPos = hitReleaseInfo.point;
            }

            Vector3 forceDirection = (pressHitPos - releaseHitPos).normalized;
            float forceMagnitude = (pressHitPos - releaseHitPos).magnitude;

            if (forceMagnitude > 1.0f)
            {

                if (m_bulletCount > 0)
                {
                    ChangeBulletNums(-1);
                    
                    List<GameObject> bullets = new List<GameObject>();

                    Vector3 bulletDirection = (releaseHitPos - m_player.transform.position).normalized;
                    bulletDirection.y = 0.0f;

                    float currentAngle = 0.0f;
                    Vector3 currentSpawnDirection = bulletDirection;
                    int times = 1;

                    for (int i = 0; i < m_pelletCount; i++)
                    {
                        GameObject bullet = Instantiate(m_bulletPrefab,
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

        /*
         *
         *  Player associated ability section
         * 
         */
        
        public void SetPelletCount(int num)
        {
            m_pelletCount = num;
            ChangeBulletNums(10);
        }

        public void ChangeBulletNums(int num)
        {
            m_bulletCount += num;
            m_bulletCount = Mathf.Clamp(m_bulletCount, 0, m_maxBulletCount);
            UIManager.Instance.ChangeBulletNum(m_bulletCount);
        }

        public void SetBulletType(string type)
        {
            
        }

        public void SetSlamActive()
        {
            isSlamActive = true;
        }
    }
}