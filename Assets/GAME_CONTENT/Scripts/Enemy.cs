using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GAME_CONTENT.Scripts
{
    public class Enemy : MonoBehaviour
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

        private GameObject m_player;
        private Rigidbody m_rb;
        private Vector3 m_direction;
        private bool canMove = true;
        private bool isDead = false;
        
        private CinemachineShake camShake;
        
        public bool isGrounded { get; set; } = false;

        // Start is called before the first frame update
        void Awake()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            m_rb = GetComponent<Rigidbody>();
            camShake = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineShake>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_player)
            {
                m_direction = (m_player.transform.position - transform.position).normalized;
            }
        }

        private void FixedUpdate()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 2.5f);
            if (isGrounded && canMove && !isDead)
            {
                FaceTarget();
                Move();
            }
            else if (!isGrounded)
            {
                // DeathSeqeuence(transform.position, Vector3.zero);
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

        public void BulletCollision(Vector3 contactPos, Vector3 bulletDirection)
        {
            contactPos.y = 0.0f;
            if (!isDead)
            {
                camShake.StartCoroutine(camShake.CamShake(2.5f, 0.2f));
                StartCoroutine(HurtSequence(0.2f));
                m_hp--;
                if (m_hp <= 0)
                {
                    DeathSeqeuence(contactPos, bulletDirection);
                }
            }
        }

        private void DeathSeqeuence(Vector3 hitPos, Vector3 direction)
        {
            m_bodyCollider.enabled = false;
            canMove = false;
            isDead = true;
            
            Quaternion up = Quaternion.LookRotation(Vector3.up);
            Instantiate(m_explosion, hitPos, up);
            
            EnemyManager.Instance.DestroyedCop();
            Destroy(gameObject);

            /*var mats = m_renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = m_explodeMaterial;
            }
            m_renderer.materials = mats;*/
                
            /*m_rb.AddExplosionForce(1500.0f, hitPos, 0.5f);
            m_rb.AddForce(direction * 1.5f, ForceMode.Impulse);
            m_rb.AddForce(Vector3.up * 100.0f, ForceMode.Impulse);*/
            
            /*yield return new WaitForSeconds(0.75f);
            
            transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    EnemyManager.Instance.DestroyedCop();
                    Destroy(gameObject);
                });*/
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.relativeVelocity.magnitude > 20.0f)
            {
                if (other.gameObject.CompareTag("Obstacle") && !isDead)
                {
                    m_hp--;
                    StartCoroutine(HurtSequence(0.2f));
                    if (m_hp <= 0)
                    {
                        DeathSeqeuence(other.contacts[0].point, other.relativeVelocity);
                    }
                }
                else if (other.gameObject.CompareTag("Player") && !isDead)
                {
                    other.gameObject.GetComponent<Player>().Damage(1);
                    StartCoroutine(HurtSequence(0.2f));
                    m_hp--;
                    if (m_hp <= 0)
                    {
                        DeathSeqeuence(other.contacts[0].point, other.relativeVelocity);
                    }
                }
            }
        }

        IEnumerator HurtSequence(float duration)
        {
            Material[] orgMaterials = m_renderer.materials;
            var mats = m_renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = m_hurtMaterial;
            }
            m_renderer.materials = mats;
            
            yield return new WaitForSeconds(duration);
            
            m_renderer.materials = orgMaterials;
        }

        // TODO damage code move to here
        private void Damage()
        {
            
        }
    }
}
