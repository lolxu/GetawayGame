using System;
using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts;
using GAME_CONTENT.Scripts.Enemy;
using GAME_CONTENT.Scripts.Other;
using GAME_CONTENT.Scripts.Player;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private float m_explosionRadius;
    [SerializeField] private GameObject m_explosion;

    private bool isTriggered = false;
    private CinemachineShake camShake;

    private void Awake()
    {
        camShake = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineShake>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !isTriggered)
        {
            camShake.StartCoroutine(camShake.CamShake(10.5f, 0.25f));
            
            other.gameObject.transform.parent.gameObject.GetComponent<EnemyBase>().Damage(3);
            PlayerController.Instance.PlayMineSFX();
            
            isTriggered = true;
            Quaternion up = Quaternion.LookRotation(Vector3.up);
            GameObject explosion = Instantiate(m_explosion, transform.position, up);
            explosion.transform.localScale *= m_explosionRadius / 4.0f;
            
            Collider[] cols = Physics.OverlapSphere(transform.position, m_explosionRadius);
            if (cols.Length > 0)
            {
                foreach (var col in cols)
                {
                    if (col.gameObject.activeInHierarchy)
                    {
                        if (col.CompareTag("Enemy"))
                        {
                            col.gameObject.transform.parent.gameObject.GetComponent<EnemyBase>().Damage(1);
                        }
                    }
                }
            }
            
            Destroy(gameObject);
        }
    }
}
