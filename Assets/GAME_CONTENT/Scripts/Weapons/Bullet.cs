using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Weapons
{
    public enum BulletType
    {
        Normal,
        HE
    }
    
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private BulletType myType;
        [SerializeField] private GameObject explosion;
        public float m_lifeTime = 1.0f;
        
        private Rigidbody m_rb;
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            StartCoroutine(StartDecay());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Enemy") 
                || other.gameObject.CompareTag("Wheel") 
                || other.gameObject.CompareTag("Obstacle"))
            {
                if (myType == BulletType.Normal)
                {
                    var enemy = other.gameObject.transform.parent.GetComponent<Enemy.EnemyBase>();
                    if (enemy)
                    {
                        enemy.BulletCollision(other.ClosestPointOnBounds(transform.position), m_rb.velocity, 1);
                    }
                }

                if (myType == BulletType.HE)
                {
                    Quaternion up = Quaternion.LookRotation(Vector3.up);
                    Instantiate(explosion, transform.position, up);
                    
                    var enemy = other.gameObject.transform.parent.GetComponent<Enemy.EnemyBase>();
                    if (enemy)
                    {
                        enemy.BulletCollision(other.ClosestPointOnBounds(transform.position), m_rb.velocity, 2);
                    }

                    Collider[] cols = Physics.OverlapSphere(transform.position, 10.0f);
                    foreach (var col in cols)
                    {
                        if (col.gameObject.activeInHierarchy)
                        {
                            if (col.CompareTag("Enemy"))
                            {
                                Enemy.EnemyBase otherEnemyBase = col.gameObject.transform.parent.gameObject.GetComponent<Enemy.EnemyBase>();
                                if (otherEnemyBase)
                                {
                                    otherEnemyBase.Damage(1);
                                }
                            }
                        }
                    }
                }
                
                Destroy(gameObject);
            }
        }

        IEnumerator StartDecay()
        {
            yield return new WaitForSeconds(m_lifeTime);
            
            transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => Destroy(gameObject));
        }
        
    }
}