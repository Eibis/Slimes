using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeType
{
    NONE,
    RED,
    GREEN,
    BLUE
}

public class Slime
{
    public SlimeType Type { get; private set; }
    public int Health = 0;
    public int Power = 0;

    private List<GameTile> MergedTiles { get; set; }

    public Slime(SlimeType type)
    {
        Type = type;

        MergedTiles = new List<GameTile>();
    }

    internal void AddTile(GameTile tile)
    {
        if (!MergedTiles.Contains(tile))
        {
            MergedTiles.Add(tile);

            Health++;
            Power++;
        }

        foreach (var m_tile in MergedTiles)
        {
            m_tile.UpdateDamageText();
        }
    }

    public List<GameTile> GetMergedTiles()
    {
        return MergedTiles;
    }

    internal void Damage(int value)
    {
        Health -= value;

        if (Health <= 0)
        {
            foreach (var tile in MergedTiles)
            {
                tile.Reset();
            }
        }
    }
}
