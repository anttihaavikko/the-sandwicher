using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AnttiStarterKit.Extensions;

public class Flasher : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> sprites;
    [SerializeField] private List<LineRenderer> lines;
    [SerializeField] private List<SpriteRenderer> spritesNeedingShader;
    [SerializeField] private Material whiteMaterial;

    [SerializeField] private float duration = 0.1f;
    
    private List<Color> spriteColors, lineColors;
    private List<Material> defaultMaterials;

    public List<SpriteRenderer> AllSprites => sprites.Concat(spritesNeedingShader).ToList();

    private void Start()
    {
        spriteColors = sprites.Select(s => s.color).ToList();
        lineColors = lines.Select(l => l.startColor).ToList();
        if (spritesNeedingShader.Any())
        {
            defaultMaterials = spritesNeedingShader.Select(s => s.material).ToList();
        }
    }

    public void Flash()
    {
        Colorize(Color.white);
        ChangeMaterials(whiteMaterial);
        this.StartCoroutine(() =>
        {
            Colorize();
            ChangeMaterials();
        }, duration);
    }
    
    private void ChangeMaterials()
    {
        spritesNeedingShader.ForEach(s => s.material = defaultMaterials[spritesNeedingShader.IndexOf(s)]);
    }

    private void ChangeMaterials(Material material)
    {
        spritesNeedingShader.ForEach(s => s.material = material);
    }

    public void Colorize(Color color, bool includeShaderOnes = false)
    {
        sprites.ForEach(s => s.color = color);
        lines.ForEach(l => l.startColor = l.endColor = color);

        if (includeShaderOnes)
        {
            spritesNeedingShader.ForEach(s => s.color = color);
        }
    }

    private void Colorize()
    {
        sprites.ForEach(s => s.color = spriteColors[sprites.IndexOf(s)]);
        lines.ForEach(l => l.startColor = l.endColor = lineColors[lines.IndexOf(l)]);
    }

    public void ChangeMaterialForAll(Material material)
    {
        spritesNeedingShader.ForEach(s => s.material = material);
        sprites.ForEach(s => s.material = material);
        lines.ForEach(s => s.material = material);
    }
}