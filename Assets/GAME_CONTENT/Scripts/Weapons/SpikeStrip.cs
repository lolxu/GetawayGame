using System;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Weapons
{
    public class SpikeStrip : MonoBehaviour
    {
        [SerializeField] private GameObject m_particles;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Quaternion up = Quaternion.LookRotation(Vector3.up);
                GameObject debris = Instantiate(m_particles, transform.position, up);
                other.gameObject.GetComponent<Player.Player>().Damage(1);
                Destroy(gameObject);
            }
        }
    }
}