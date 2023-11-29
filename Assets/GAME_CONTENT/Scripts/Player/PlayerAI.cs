using System.Collections.Generic;
using GAME_CONTENT.Scripts.Other;
using UnityEngine;
using UnityEngine.AI;

namespace GAME_CONTENT.Scripts.Player
{
    public class PlayerAI : MonoBehaviour
    {
        [SerializeField] private GameObject m_goal;
        [SerializeField] private float m_goalGenerateRadius = 150.0f;
        [SerializeField] private float m_goalFinishRadius = 5.0f;
        [SerializeField] private Material m_goalSpecialMaterial;
        [SerializeField] private GameObject m_goalParticle;
        
        [Header("Sounds")]
        [SerializeField] private List<AudioClip> m_pickupSFX;
        private AudioSource m_audioSource;

        public Vector3 m_goalPosition { set; get; } = Vector3.zero;
        
        private NavMeshAgent agent;
        private int m_goalsReached = 0;
        private int m_abilityCheckpointNum;
        private Renderer m_renderer;
        private Material m_orgMaterial;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updatePosition = false;
            
            if (m_goal)
            {
                m_goalPosition = m_goal.transform.position;
                m_renderer = m_goal.GetComponent<Renderer>();
                m_orgMaterial = m_renderer.material;
            }
            m_abilityCheckpointNum = Random.Range(3, 5);

            m_audioSource = GetComponent<AudioSource>();
        }
        
        private void Update()
        {
            if (Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, m_goalPosition) < m_goalFinishRadius)
            {
                m_goalsReached++;
                m_goal.GetComponent<GoalScript>().SetFirstHintActive(false);
                GameManager.Instance.ChangeCheckpointNum(1);
                GameManager.Instance.AddToRemainingTime(Random.Range(5.0f, 10.0f));

                Quaternion up = Quaternion.LookRotation(Vector3.up);
                Instantiate(m_goalParticle, m_goal.transform.position, up);
                
                AudioClip toPlay = m_pickupSFX[Random.Range(0, m_pickupSFX.Count)];
                m_audioSource.pitch = Random.Range(0.45f, 0.75f);
                m_audioSource.PlayOneShot(toPlay);
                
                if (m_goalsReached == m_abilityCheckpointNum && m_goalsReached != GameManager.Instance.GetTotalCheckpoints())
                {
                    m_goal.GetComponent<GoalScript>().SetSecondHintActive(false);
                    m_abilityCheckpointNum += Random.Range(3, 6);
                    GameManager.Instance.ShowAbilities();
                    
                    m_renderer.material = m_orgMaterial;
                }
                else if (m_goalsReached == m_abilityCheckpointNum - 1)
                {
                    m_renderer.material = m_goalSpecialMaterial;
                    m_goal.GetComponent<GoalScript>().SetSecondHintActive(true);
                }
                else
                {
                    m_renderer.material = m_orgMaterial;
                }
                
                GenerateGoal();
            }
        }
        
        private void GenerateGoal()
        {
            m_goalPosition = RandomNavmeshLocation();
            m_goal.transform.position = m_goalPosition + new Vector3(0.0f, 3.0f, 0.0f);
        }

        private Vector3 RandomNavmeshLocation()
        {
            Vector3 finalPosition = new Vector3(-450.0f, 3.0f, 500.0f);
            int tries = 0;
            do
            {
                float angle = Random.Range(-150.0f, 150.0f);
                var quaternion = Quaternion.Euler(0.0f, angle, 0.0f);
                var randomDirection = quaternion * GameObject.FindGameObjectWithTag("Player").transform.forward * m_goalGenerateRadius;
                NavMeshHit hit;
                NavMesh.SamplePosition(transform.position + randomDirection, out hit, m_goalGenerateRadius, 1);
                finalPosition = hit.position;
                tries++;
            } while (Vector3.Distance(m_goalPosition, finalPosition) < m_goalGenerateRadius && tries < 10);

            if (agent.enabled)
            {
                agent.SetDestination(finalPosition);
            }
            
            return finalPosition;
        }

        public void AICanMove()
        {
            agent.updatePosition = true;
            agent.SetDestination(m_goalPosition);
        }
    }
}