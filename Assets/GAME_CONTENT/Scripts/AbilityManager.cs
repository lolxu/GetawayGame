using System;
using System.Collections;
using System.Collections.Generic;
using GAME_CONTENT.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    private int abilityActivated = 0;
    
    
    public enum AbilityTier
    {
        Basic,
        Intermediate,
        OP
    }
    
    public enum AbilityTypes
    {
        Projectile,
        Smash,
        Passive,
        Consumable
    }

    [Serializable]
    public class Ability
    {
        public string m_abilityName;
        public string m_abilityDescription;
        public AbilityTypes m_abilityType;
        public bool activated = false;
    }

    [Serializable]
    public class AbilityPool
    {
        public AbilityTier m_tier;
        public Ability m_ability;
    }
    
    [SerializeField] private List<AbilityPool> m_abilityPool;
    public Dictionary<Ability, bool> AbilityActivated;

    [SerializeField] private Player m_player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        AbilityActivated = new Dictionary<Ability, bool>();
        foreach (var ability in m_abilityPool)
        {
            AbilityActivated.Add(ability.m_ability, false);
        }
        
    }
    
    // If the same ability is rolled and chosen, just add points...

    public List<Ability> OnRequestAbilities()
    {
        abilityActivated++;
        AbilityTier m_currentTier;

        List<Ability> m_availableAbilities = new List<Ability>();

        if (abilityActivated <= 3)
        {
            m_currentTier = AbilityTier.Basic;
        }
        else if (abilityActivated <= 6)
        {
            m_currentTier = AbilityTier.Intermediate;
        }
        else
        {
            m_currentTier = AbilityTier.OP;
        }
        
        foreach (var abilityPair in m_abilityPool)
        {
            if (abilityPair.m_tier <= m_currentTier)
            {
                m_availableAbilities.Add(abilityPair.m_ability);
            }
        }

        List<Ability> chosenAbilities = new List<Ability>();
        List<int> pickedIndex = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, m_availableAbilities.Count);

            while (pickedIndex.Contains(index) || m_availableAbilities[index].activated)
            {
                index = Random.Range(0, m_availableAbilities.Count);
            }
            
            pickedIndex.Add(index);
            Ability chosenAbility = m_availableAbilities[index];
            // Debug.LogError(chosenAbility.m_abilityName + ", " + index + ", " + m_availableAbilities.Count);

            chosenAbilities.Add(chosenAbility);
        }

        return chosenAbilities;
    }

    public void ActivateAbility(Ability active)
    {
        // Check type
        AbilityTypes activeType = active.m_abilityType;
        string abilityName = active.m_abilityName;

        if (activeType == AbilityTypes.Projectile)
        {
            switch (abilityName)
            {
                case "Hand Gun":
                    PlayerController.Instance.SetPelletCount(1);
                    break;
                case "Shotgun":
                    PlayerController.Instance.SetPelletCount(3);
                    break;
                case "Shotgun+":
                    PlayerController.Instance.SetPelletCount(5);
                    break;
                case "HE Round":
                    PlayerController.Instance.SetPelletCount(1);
                    PlayerController.Instance.SetBulletType("HE");
                    break;
            }
        }
        else if (activeType == AbilityTypes.Smash)
        {
            switch (abilityName)
            {
                case "Slam":
                    PlayerController.Instance.SetSlamActive();
                    break;
                case "Explosive Slam":
                    PlayerController.Instance.SetSlamActive();
                    break;
            }
        }
        else if (activeType == AbilityTypes.Passive)
        {
            switch (abilityName)
            {
                case "Side Kick":
                    break;
                case "Mines":
                    break;
            }
        }
        else if (activeType == AbilityTypes.Consumable)
        {
            switch (abilityName)
            {
                case "Heal":
                    m_player.Heal(1);
                    break;
                case "Heal+":
                    m_player.Heal(2);
                    break;
                case "Heal++":
                    m_player.Heal(100);
                    break;
                case "HP+":
                    m_player.AddMaxHP(1);
                    break;
                case "Speed+":
                    m_player.AddSpeed(Random.Range(1.0f, 5.0f));
                    break;
            }
        }
    }
}