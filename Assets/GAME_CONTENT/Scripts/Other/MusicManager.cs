using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GAME_CONTENT.Scripts.Other
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        [SerializeField] private AudioClip m_menuMusic;
        [SerializeField] private AudioClip m_gameMusic;
        private AudioSource m_audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            m_audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoadedMusic;
        }

        private void OnSceneLoadedMusic(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                if (m_audioSource)
                {
                    m_audioSource.Stop();
                    m_audioSource.clip = m_menuMusic;
                    m_audioSource.Play();
                }
            }
            else
            {
                if (m_audioSource)
                {
                    m_audioSource.Stop();
                    m_audioSource.clip = m_gameMusic;
                    m_audioSource.Play();
                }
            }
        }
    }
}