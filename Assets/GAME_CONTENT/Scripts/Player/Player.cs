using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts.Other;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GAME_CONTENT.Scripts.Player
{
    public class Player : MonoBehaviour
    {
        // TODO use feel package for particles
        
        [SerializeField] private float m_turnSpeed = 5.0f;
        [SerializeField] private int m_HP = 3;
        private int m_maxHP = 3;
        [SerializeField] private GameObject m_explosion;
        [SerializeField] private Renderer m_renderer;
        [SerializeField] private Material m_explodeMaterial;
        [SerializeField] private Material m_hurtMaterial;
        [SerializeField] private GameObject m_engineSmoke;

        [Header("Sounds")] 
        [SerializeField] private List<AudioClip> m_hurtSFX;
        private AudioSource m_audioSource;

        private Vector3 m_input;
        private Rigidbody m_rb;
        private NavMeshAgent m_agent;
        private Matrix4x4 matrix;

        private float currentSpeed = 0.0f;
        private float m_randomInputTimeout = 0.0f;
        private bool canStartMoving = false;
        private bool isInvulnerable = false;
        private bool isDead = false;
        private CinemachineShake camShake;
        private bool isHurtDone = true;
        private Material[] orgMaterials;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            matrix = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 45.0f, 0.0f)); // Stock coordinate system
            orgMaterials = m_renderer.materials;
            m_audioSource = GetComponent<AudioSource>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.1f);
            canStartMoving = true;
            m_agent = GetComponent<NavMeshAgent>();
            camShake = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineShake>();
        }

        private void FixedUpdate()
        {
            if (canStartMoving)
            {
                FaceTarget();
            }
        }

        private void FaceTarget()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                var turnTowardNavSteeringTarget = GameObject.FindGameObjectWithTag("Finish").transform.position;
                if (m_agent.enabled)
                {
                    turnTowardNavSteeringTarget = GetComponent<NavMeshAgent>().steeringTarget;
                }

                Vector3 direction = (turnTowardNavSteeringTarget - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.z));
                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, lookRotation, Time.fixedDeltaTime * m_turnSpeed);
            }
        }

        private IEnumerator DeathSequence(Vector3 hitPos)
        {
            Debug.Log("Start player death");

            if (m_agent.enabled)
            {
                m_agent.ResetPath();
            }

            m_agent.enabled = false;
            m_rb.isKinematic = false;
            isDead = true;
            
            PlayerController.Instance.PlayDeathSFX();

            yield return null;
            
            Quaternion up = Quaternion.LookRotation(Vector3.up);
            Instantiate(m_explosion, hitPos, up);
            Destroy(gameObject);
        }
        
        /*
         *
         * Health Section
         * 
         */
        public void Damage(int amount)
        {
            if (!isInvulnerable && isHurtDone)
            {
                isHurtDone = false;
                camShake.StartCoroutine(camShake.CamShake(5.5f, 0.2f));
                StartCoroutine(HurtSequence(0.2f));
                
                AudioClip toPlay = m_hurtSFX[Random.Range(0, m_hurtSFX.Count)];
                m_audioSource.pitch = Random.Range(0.6f, 1.25f);
                m_audioSource.PlayOneShot(toPlay);
                
                m_HP -= amount;
                if (m_HP == 1)
                {
                    m_engineSmoke.GetComponent<ParticleSystem>().Play();
                }
                else if (m_HP <= 0 && !isDead)
                {
                    m_HP = 0;
                    GameManager.Instance.SetDeathMessage("KIA");
                    StartCoroutine(DeathSequence(transform.position));
                }
            
                GameManager.Instance.ChangeHP(m_HP);
            }
        }

        public void Heal(int amount)
        {
            m_HP += amount;
            m_HP = Mathf.Clamp(m_HP, 0, m_maxHP);
            if (m_HP > 1 && m_engineSmoke.GetComponent<ParticleSystem>().isPlaying)
            {
                m_engineSmoke.GetComponent<ParticleSystem>().Stop();
            }
            GameManager.Instance.ChangeHP(m_HP);
        }

        public void AddMaxHP(int amount)
        {
            m_maxHP += amount;
            Heal(m_maxHP);
        }

        public void InstantKill()
        {
            m_HP = 0;
            GameManager.Instance.ChangeHP(m_HP);
            StartCoroutine(DeathSequence(transform.position));
        }
        
        IEnumerator HurtSequence(float duration)
        {
            Debug.Log("Start Hurt Sequence");
            
            var mats = m_renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = m_hurtMaterial;
            }
            m_renderer.materials = mats;
            
            yield return new WaitForSeconds(duration);
            
            m_renderer.materials = orgMaterials;
            isHurtDone = true;
        }
        
        /*
         *
         *  Abilities Section
         * 
         */
        public void AddSpeed(float amount)
        {
            m_agent.speed += amount;
        }

        public void SetIsInvulnerable(bool status)
        {
            isInvulnerable = status;
        }
    }
}
