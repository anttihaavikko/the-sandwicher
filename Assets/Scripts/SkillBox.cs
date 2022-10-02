using System;
using TMPro;
using UnityEngine;

public class SkillBox : MonoBehaviour
{
    [SerializeField] private TMP_Text title, description;
    [SerializeField] private int skill;

    private int level = 1;

    public const int BaseHeal = 3;
    public const int HealPerAlchemyLevel = 3;
    public const int SlowAmountPerLevel = 20;

    private void Start()
    {
        title.text = GetTitle();
        description.text = GetDescription();
    }

    public void Level()
    {
        level++;
        title.text = GetTitle();
        description.text = GetDescription();
    }

    public string GetTitle()
    {
        return skill switch
        {
            0 => $"Alchemy LVL {level}",
            1 => $"Aarg LVL {level}",
            2 => $"Ignu LVL {level}",
            3 => $"Azii LVL {level}",
            4 => $"Guen LVL {level}",
            5 => $"Urden LVL {level}",
            _ => "Unknown"
        };
    }
    
    public string GetDescription()
    {
        return skill switch
        {
            0 => $"Potions heal for {BaseHeal + level * HealPerAlchemyLevel} HP.\nQuaffed after landing.",
            1 => level > 1 ? 
                "Even stronger push on enemies when low on HP." : 
                "Push enemies away when low on HP.",
            2 => level > 1 ? 
                $"Burn {level} closest enemies when landing from a jump." : 
                "Burn closest enemy when landing from a jump.",
            3 => $"Enemy wave meter fills {GetSlow()}% more slowly.",
            4 => level > 1 ? 
                $"Cast a shield that protects you from a {level} killing blows." :
                "Cast a shield that protects you from a killing blow.",
            5 => level > 1 ? 
                $"Charm {level} closest enemies on kill. They will not retaliate." : 
                "Charm closest enemy on kill. They will not retaliate.",
            _ => "Unknown"
        };
    }

    private int GetSlow()
    {
        return Mathf.RoundToInt(100 * (1 - Mathf.Pow(0.8f, level)));
    }
}