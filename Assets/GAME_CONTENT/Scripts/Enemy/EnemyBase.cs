using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts.Other;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] private float m_acceleration = 8.0f;
        [SerializeField] private float m_deacceleration = 5.0f;
        [SerializeField] private float m_turnSpeed = 5.0f;
        [SerializeField] private float m_maxSpeed = 80.0f;
        [SerializeField] private GameObject m_explosion;
        [SerializeField] private Renderer m_renderer;
        [SerializeField] private Material m_hurtMaterial;
        [SerializeField] private Collider m_bodyCollider;
        [SerializeField] private int m_hp = 1;
        [SerializeField] private float m_waitToDestroyTimer = 3.0f;
        [SerializeField] private float rayCastDistance = 0.5f;
        [SerializeField] private float rayCastOffset = 0.15f;
        [SerializeField] private int m_enemyType = 0;

        private GameObject m_player;
        private Rigidbody m_rb;
        private Vector3 m_direction;
        private bool canMove = true;
        private bool isDead = false;
        private bool isHurtDone = true;
        private float flippedTimer = 0.0f;
        private Material[] orgMaterials;
        private CinemachineShake camShake;
        private int m_orgHP;

        protected bool m_hasAbilities = false;
        protected float m_abilityTimeout = 0.0f;
        private float m_timeout = 0.0f;
        
        public bool isGrounded { get; set; } = false;

        // Start is called before the first frame update
        private void Awake()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            m_rb = GetComponent<Rigidbody>();
            camShake = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineShake>();
            orgMaterials = m_renderer.materials;
            m_orgHP = m_hp;
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_player)
            {
                m_direction = (m_player.transform.position - transform.position).normalized;
            }
        }

        private void FixedUpdate()
        {
            isGrounded = Physics.Raycast(transform.position + Vector3.up * rayCastOffset, Vector3.down, rayCastDistance);
            if (isGrounded && canMove && !isDead)
            {
                flippedTimer = 0.0f;
                FaceTarget();
                Move();
            }
            else if (!isGrounded)
            {
                // Debug.Log("Not Grounded: " + gameObject.name + transform.position+rayCastDistance);
                flippedTimer += Time.deltaTime;
            }

            if (flippedTimer >= m_waitToDestroyTimer)
            {
                if (!isDead)
                {
                    DeathSeqeuence(transform.position, Vector3.zero);
                }
            }
            
            // Abilities Section
            if (m_hasAbilities)
            {
                if (m_timeout < m_abilityTimeout)
                {
                    m_timeout += Time.fixedDeltaTime;
                }
                else
                {
                    ActivateCopAbilities();
                    m_timeout = 0.0f;
                }
            }
        }

        private void Move()
        {
            if (m_player)
            {
                m_rb.AddForce(m_direction * m_acceleration);
                m_rb.velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, m_maxSpeed);
            }
        }

        private void FaceTarget()
        {
            if (m_player)
            {
                Vector3 direction = (m_player.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * m_turnSpeed);
            }
        }

        public void BulletCollision(Vector3 contactPos, Vector3 bulletDirection, int damage)
        {
            contactPos.y = 0.0f;
            if (!isDead)
            {
                GameManager.Instance.AddToRemainingTime(1.0f);
                camShake.StartCoroutine(camShake.CamShake(3.5f, 0.2f));
                Damage(damage);
            }
        }

        private void DeathSeqeuence(Vector3 hitPos, Vector3 direction)
        {
            // Reset key values
            m_hp = m_orgHP;
            flippedTimer = 0.0f;
            isHurtDone = true;
            m_rb.velocity = Vector3.zero;
            
            EnemyManager.Instance.PlayDeathSFX(transform.position);
            
            Quaternion up = Quaternion.LookRotation(Vector3.up);
            Instantiate(m_explosion, hitPos, up);
            // StopAllCoroutines();
            m_renderer.materials = orgMaterials;
            EnemyManager.Instance.DestroyedCop(m_enemyType, gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.relativeVelocity.magnitude > 20.0f)
            {
                // Obstacle collision a bit too jarring
                // if (other.gameObject.CompareTag("Obstacle") && !isDead && other.relativeVelocity.magnitude > 50.0f)
                // {
                //     Damage();
                // }
                if (other.gameObject.CompareTag("Player") && !isDead)
                {
                    GameManager.Instance.AddToRemainingTime(1.0f);
                    other.gameObject.GetComponent<Player.Player>().Damage(1);
                    Damage(1);
                }
            }
        }

        IEnumerator HurtSequence(float duration)
        {
            // Debug.Log("Start Hurt Sequence");
            
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

        // Damage cop
        public void Damage(int amount)
        {
            m_hp -= amount;
            if (isHurtDone)
            {
                isHurtDone = false;
                if (gameObject)
                {
                    StartCoroutine(HurtSequence(0.1f));
                }
            }
            if (m_hp <= 0)
            {
                DeathSeqeuence(transform.position, Vector3.zero);
            }
        }

        protected virtual void ActivateCopAbilities()
        {
            
        }
    }
}
