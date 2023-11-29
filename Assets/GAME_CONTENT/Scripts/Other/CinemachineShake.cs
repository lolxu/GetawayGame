using System.Collections;
using Cinemachine;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Other
{
    public class CinemachineShake : MonoBehaviour
    {
        // public static CinemachineShake Instance { get; private set; }
        private CinemachineVirtualCamera cv;
        private void Awake()
        {
            cv = GetComponent<CinemachineVirtualCamera>();
        }

        public IEnumerator CamShake(float intensity, float time)
        {
            CinemachineBasicMultiChannelPerlin cbp = cv.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cbp.m_AmplitudeGain = intensity;
            yield return new WaitForSecondsRealtime(time);
            // Debug.Log("Finish Cam Shake");
            cbp.m_AmplitudeGain = 0f;
        }
    }
}
