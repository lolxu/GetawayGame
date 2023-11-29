using UnityEngine;

namespace GAME_CONTENT.Scripts.Player
{
    public class GoalScript : MonoBehaviour
    {
        // TODO swap the mesh of goal to something cooler
        // TODO (Stretch) do a little dude running out from car and disappearing
        
        [SerializeField] private GameObject m_firstHint;
        [SerializeField] private GameObject m_secondHint;
        [SerializeField] private bool toggleHint;
        private GameObject mainCam;
        private Vector3 m_goalPosition;
        private GameObject m_player;

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (!toggleHint)
            {
                SetFirstHintActive(false);
                SetSecondHintActive(false);
            }
        }

        public void SetFirstHintActive(bool canShow)
        {
            m_firstHint.SetActive(canShow);
        }

        public void SetSecondHintActive(bool canShow)
        {
            m_secondHint.SetActive(canShow);
        }

        // void LateUpdate()
        // {
        //     if (toggleToShowHint)
        //     {
        //         if (!useStaticBillboard)
        //         {
        //             m_hint.transform.LookAt(mainCam.transform);
        //         }
        //         else
        //         {
        //             m_hint.transform.rotation = mainCam.transform.rotation;
        //         }
        //
        //         m_hint.transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
        //         Debug.Log(m_hint.transform.rotation);
        //     }
        // }
    }
}
