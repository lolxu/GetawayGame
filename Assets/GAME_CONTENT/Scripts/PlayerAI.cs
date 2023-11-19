using UnityEngine;
using UnityEngine.AI;

namespace GAME_CONTENT.Scripts
{
    public class PlayerAI : MonoBehaviour
    {
        [SerializeField] private GameObject m_goal;
        
        public float m_goalGenerateRadius = 150.0f;
        public float m_goalFinishRadius = 5.0f;
        public Vector3 m_goalPosition;
        
        private NavMeshAgent agent;
        private int m_goalsReached = 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            m_goalPosition = m_goal.transform.position;
            agent.SetDestination(m_goalPosition);
        }
        
        private void Update()
        {
            if (Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, m_goalPosition) < m_goalFinishRadius)
            {
                m_goalsReached++;
                
                UIManager.Instance.ChangeCheckpointNum(1);
                
                if (m_goalsReached % 3 == 0)
                {
                    UIManager.Instance.ShowAbilities();
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
            Vector3 finalPosition = Vector3.zero;
            do
            {
                float angle = Random.Range(-120.0f, 120.0f);
                var quaternion = Quaternion.Euler(0.0f, angle, 0.0f);
                var randomDirection = quaternion * GameObject.FindGameObjectWithTag("Player").transform.forward * m_goalGenerateRadius;
                NavMeshHit hit;
                NavMesh.SamplePosition(transform.position + randomDirection, out hit, m_goalGenerateRadius, 1);
                finalPosition = hit.position;
            } while (Vector3.Distance(m_goalPosition, finalPosition) < m_goalGenerateRadius);
            
            if (agent.enabled)
            {
                agent.SetDestination(finalPosition);
            }
            
            return finalPosition;
        }
    }
}