using System;
using DG.Tweening;
using UnityEngine;

namespace GAME_CONTENT.Scripts.Player
{
    public class PlayerMenu : MonoBehaviour
    {
        private void Start()
        {
            transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), 2.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutCirc).SetLoops(-1);
        }
    }
}