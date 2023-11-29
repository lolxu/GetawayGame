using System;
using UnityEngine;
using UnityEngine.UI;

namespace GAME_CONTENT.Scripts.Other
{
    public class CheckpointPointer : MonoBehaviour
    {
        [SerializeField] private GameObject m_goal;
        private Vector3 targetPosition;
        private RectTransform pointerRectTransform;
        private GameObject m_player;
        private Image m_imageComponent;
        private float imageAlpha;

        private void Awake()
        {
            targetPosition = m_goal.transform.position;
            pointerRectTransform = transform.Find("Pointer").GetComponent<RectTransform>();
            m_imageComponent = pointerRectTransform.gameObject.GetComponent<Image>();
            m_player = GameObject.FindGameObjectWithTag("Player");
            imageAlpha = m_imageComponent.color.a;
        }

        private void Update()
        {
            if (m_player)
            {
                targetPosition = m_goal.transform.position;
                Vector3 toPosition = Camera.main.WorldToScreenPoint(targetPosition);
                Vector3 fromPosition = Camera.main.WorldToScreenPoint(m_player.transform.position);

                // Debug.Log(fromPosition);

                fromPosition.z = 0.0f;
                Vector3 dir = (toPosition - fromPosition).normalized;

                // Getting angle from vector3
                float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) % 360.0f;
                // Debug.Log(angle);
                pointerRectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, angle);

                float borderSize = 100.0f;

                Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint(targetPosition);
                bool isOffScreen = targetPositionScreenPoint.x <= borderSize ||
                                   targetPositionScreenPoint.x >= Screen.width - borderSize ||
                                   targetPositionScreenPoint.y <= borderSize ||
                                   targetPositionScreenPoint.y >= Screen.height - borderSize;

                // Debug.Log(isOffScreen);

                if (isOffScreen)
                {
                    // pointerRectTransform.gameObject.SetActive(true);
                    Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
                    cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderSize,
                        Screen.width - borderSize);
                    cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderSize,
                        Screen.height - borderSize);

                    pointerRectTransform.position = cappedTargetScreenPosition;

                    pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x,
                        pointerRectTransform.localPosition.y, 0.0f);
                }
                else
                {
                    // pointerRectTransform.gameObject.SetActive(false);
                    pointerRectTransform.position = Camera.main.WorldToScreenPoint(targetPosition);
                    pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x,
                        pointerRectTransform.localPosition.y, 0.0f);
                }

                float dist = Vector3.Distance(targetPosition, m_player.transform.position) - 55.0f;
                // Debug.Log(dist);
                imageAlpha = Mathf.Clamp(dist / 125.0f, 0.0f, 1.0f);
                Color fadeColor = new Color(m_imageComponent.color.r, m_imageComponent.color.g,
                    m_imageComponent.color.b,
                    imageAlpha);
                m_imageComponent.color = fadeColor;
            }
        }
    }
}