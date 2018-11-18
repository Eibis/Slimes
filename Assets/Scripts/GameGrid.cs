using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public GameObject Tile;

    public int Width = 8;
    public int Height = 8;

    public float TileWidth = 50.0f;
    public float TileHeight = 50.0f;

    public GameTile[,] Tiles;

    public void Init ()
    {
        Tiles = new GameTile[Width, Height * 2];

        float total_width = Width * TileWidth;
        float total_height = Height * TileHeight;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Vector2 vec = new Vector2(i * TileWidth - total_width / 2.0f, j * TileHeight - total_height);

                CreateTile(i, j, vec, 0);
            }

            for (int j = 0; j < Height; j++)
            {
                Vector2 vec = new Vector2(i * TileWidth - total_width / 2.0f, (j + Height + 1) * TileHeight - total_height);

                CreateTile(i, j + Height, vec, 1);
            }
        }
	}

    private void CreateTile(int i, int j, Vector2 vec, int player)
    {
        GameObject go = Instantiate(Tile, transform);
        go.transform.localPosition = vec;

        GameTile tile = go.GetComponent<GameTile>();
        tile.SetOwner(player);
        tile.SetState(GameTile.TileState.EMPTY);
        tile.SetIndices(i, j);
        Tiles[i, j] = tile;
    }

    internal GameTile GetTile(int i, int j, int owner = -1)
    {
        if (i < 0 || i >= Width)
            return null;

        if (j < 0 || j >= Height * 2)
            return null;

        GameTile tile = Tiles[i, j];

        if (owner != -1)
        {
            if (tile.Owner != owner)
                return null;
        }

        return tile;
    }

    internal List<GameTile> GetMergedTiles(int i, int j, int owner = -1)
    {
        if (i < 0 || i >= Width)
            return null;

        if (j < 0 || j >= Height * 2)
            return null;

        GameTile tile = Tiles[i, j];

        if (owner != -1)
        {
            if (tile.Owner != owner)
                return null;
        }

        if (tile.Slime == null)
            return null;

        return tile.Slime.GetMergedTiles();
    }
}
