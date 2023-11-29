using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Weapons
{
    public class CopBullet : MonoBehaviour
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
            if (other.CompareTag("Obstacle"))
            {
                Destroy(gameObject);
            }
            else if (other.CompareTag("Player"))
            {
                var player = other.gameObject.GetComponent<Player.Player>();
                player.Damage(1);
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