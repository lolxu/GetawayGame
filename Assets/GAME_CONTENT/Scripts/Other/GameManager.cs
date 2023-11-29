using System.Collections.Generic;
using DG.Tweening;
using GAME_CONTENT.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GAME_CONTENT.Scripts.Other
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        public GameObject m_abilityPanel;
        public GameObject m_deathPanel;
        public GameObject m_timeScore;
        public GameObject m_hpUI;
        public GameObject m_ammoUI;
        public GameObject m_checkpointUI;
        public GameObject m_winPanel;
        public GameObject m_HUD;
        public GameObject m_startPanel;
        public GameObject m_deathMessage;
        
        public bool isGamePaused = false;

        private float m_orgTimeScale;
        private GameObject m_player;

        private bool isGameOver = false;
        private bool isPlayerWin = false;

        private List<AbilityManager.Ability> chosenAbilities = new List<AbilityManager.Ability>();

        [SerializeField] private float m_remainingTime = 120.0f;
        [SerializeField] private int m_totalCheckPoints = 30;
        private int m_currentCheckPoints = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                m_player = GameObject.FindGameObjectWithTag("Player");
                // Initialize UI
                ChangeCheckpointNum(0);
                ChangeBulletNum(0);
            }
            
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
            Application.targetFrameRate = 90;
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (!m_player)
                {
                    if (!isGameOver)
                    {
                        isGameOver = true;
                        ShowDeathPanel();
                    }
                }
                else if (!isGameOver && !m_startPanel.activeInHierarchy)
                {
                    m_remainingTime -= Time.deltaTime;
                    m_timeScore.GetComponent<Text>().text = ((int)m_remainingTime).ToString();
                }

                if (m_remainingTime <= 0.0f)
                {
                    isGameOver = true;
                    if (m_player)
                    {
                        m_player.GetComponent<Player.Player>().InstantKill();
                    }
                    SetDeathMessage("YOU RAN OUT OF TIME...");
                    ShowDeathPanel();
                }
            }
        }

        public void ShowDeathPanel()
        {
            m_deathPanel.SetActive(true);
            m_deathPanel.transform.DOScale(Vector3.one, 0.25f);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ShowAbilities()
        {
            chosenAbilities = AbilityManager.Instance.OnRequestAbilities();
            m_abilityPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                GameObject title = m_abilityPanel.transform.GetChild(i).gameObject;
                GameObject button = m_abilityPanel.transform.GetChild(i + 3).gameObject;
                title.GetComponent<Text>().text = chosenAbilities[i].m_abilityName;
                button.transform.GetChild(0).GetComponent<Text>().text = chosenAbilities[i].m_abilityDescription;
            }
            
            m_abilityPanel.transform.DOScale(Vector3.one, 0.25f)
                .OnComplete(() =>
                {
                    isGamePaused = true;
                    m_orgTimeScale = Time.timeScale;
                    Time.timeScale = 0.0f;
                });
        }

        public void HideAbilities(int buttonNum)
        {
            if (chosenAbilities.Count > 0)
            {
                if (chosenAbilities[buttonNum].activated)
                {
                    Debug.Log("Adding extra points");
                }
                else
                {
                    if (chosenAbilities[buttonNum].m_abilityType != AbilityManager.AbilityTypes.Consumable)
                    {
                        chosenAbilities[buttonNum].activated = true;
                    }
                    AbilityManager.Instance.ActivateAbility(chosenAbilities[buttonNum]);
                }
            }
            
            Time.timeScale = m_orgTimeScale;
            m_abilityPanel.transform.DOScale(Vector3.zero, 0.25f)
                .OnComplete(() =>
                {
                    isGamePaused = false;
                    m_abilityPanel.SetActive(false);
                });
        }

        public void ChangeHP(int amount)
        {
            m_hpUI.GetComponent<Text>().text = "HP: " + amount;
        }

        public void ChangeCheckpointNum(int amount)
        {
            m_currentCheckPoints += amount;
            
            m_checkpointUI.GetComponent<Text>().text = "Stops: " + m_currentCheckPoints + "/" + m_totalCheckPoints;

            if (m_currentCheckPoints == m_totalCheckPoints && !isGameOver)
            {
                m_winPanel.SetActive(true);
                isGameOver = true;
                isPlayerWin = true;
                m_winPanel.transform.DOScale(Vector3.one, 0.25f).OnComplete(() =>
                {
                    Time.timeScale = 0.0f;
                });
            }
        }

        public void ChangeBulletNum(int amount)
        {
            m_ammoUI.GetComponent<Text>().text = "Ammo: " + amount + "/" + PlayerController.Instance.GetMaxAmmo();
        }

        public void AddToRemainingTime(float amount)
        {
            m_remainingTime += amount;
        }

        public void ReleaseStartPanel()
        {
            m_startPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m_startPanel.SetActive(false);
                m_HUD.SetActive(true);
            });
        }

        public void LoadNextLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void LoadThisLevel(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;            
#endif
            Application.Quit();
        }

        public void BackToMenu()
        {
            LoadThisLevel(0);
        }

        public int GetTotalCheckpoints()
        {
            return m_totalCheckPoints;
        }

        public void SetDeathMessage(string message)
        {
            m_deathMessage.GetComponent<Text>().text = message;
        }
    }
}