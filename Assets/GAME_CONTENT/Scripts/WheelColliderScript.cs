using System;
using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts;
using Unity.VisualScripting;
using UnityEngine;

public class WheelColliderScript : MonoBehaviour
{
    [SerializeField] private Enemy m_owner;
    private Collider m_collider;
    void Start()
    {
        m_collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 7)
        {
            m_owner.isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == 7)
        {
            m_owner.isGrounded = false;
        }
    }
}
