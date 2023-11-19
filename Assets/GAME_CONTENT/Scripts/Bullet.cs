using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace GAME_CONTENT.Scripts
{
    public class Bullet : MonoBehaviour
    {
        public float m_lifeTime = 1.0f;
        private Rigidbody m_rb;
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            StartCoroutine(StartDecay());
        }

        private void OnTriggerEnter(Collider other)
        {
//            Debug.Log(other.gameObject);
            if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Wheel"))
            {
                var enemy = other.gameObject.transform.parent.GetComponent<Enemy>();
                enemy.BulletCollision(other.ClosestPointOnBounds(transform.position), m_rb.velocity);
                Destroy(gameObject);
            }
            
            /*if (!other.gameObject.CompareTag("Player") || !other.gameObject.CompareTag("Bullet"))
            {
                Destroy(gameObject);
            }*/
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