using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GAME_CONTENT.Scripts
{
    public class GoalScript : MonoBehaviour
    {
        private Vector3 m_goalPosition;
        private GameObject m_player;

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }
    }
}
