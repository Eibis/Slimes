using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public GameGrid Grid;
    public GameUI UI;
    public int CurrentPlayer { get; private set; }

    SlimeType[] NextSlimeType;

    Dictionary<GameTile.TileState, int> MoveAllowance = new Dictionary<GameTile.TileState, int>()
    {
        { GameTile.TileState.EMPTY, 1 },
        { GameTile.TileState.COVERED, 2 }
    };

    Dictionary<GameTile.TileState, int>[] CurrentMoveAllowance;

    void Awake()
    {
        Instance = this;
        NextSlimeType = new SlimeType[2];
        CurrentMoveAllowance = new []
        {
            new Dictionary<GameTile.TileState, int>(),
            new Dictionary<GameTile.TileState, int>()
        };
    }

    public void Init()
    {
        Grid.Init();
        UI.SetTurn(CurrentPlayer);
        StartRound();
    }

    private void GenerateSlimeTypes()
    {
        NextSlimeType[0] = (SlimeType)(UnityEngine.Random.Range(0, 3) + 1);
        NextSlimeType[1] = (SlimeType)(UnityEngine.Random.Range(0, 3) + 1);

        UI.SetNextSlime();
    }

    internal SlimeType GetNextSlime(int owner)
    {
        return NextSlimeType[owner];
    }

    internal void Uncover(int I, int J)
    {
        List<GameTile> tiles = new List<GameTile>();

        GameTile uncovered_tile = Grid.GetTile(I, J);
        tiles.Add(uncovered_tile);

        int min_i, min_j, max_i, max_j;
        min_i = uncovered_tile.I;
        max_i = min_i;
        min_j = uncovered_tile.J;
        max_j = min_j;

        for (int i = I - 1; i <= I + 1; i++)
        {
            for (int j = J - 1; j <= J + 1; j++)
            {
                List<GameTile> merged_tiles = Grid.GetMergedTiles(i, j, uncovered_tile.Owner);

                if (merged_tiles == null)
                    continue;

                foreach (GameTile tile in merged_tiles)
                { 
                    switch (tile.CurrentState)
                    {
                        case GameTile.TileState.EMPTY:
                            break;
                        case GameTile.TileState.COVERED:
                        case GameTile.TileState.OCCUPIED:

                            if (tile.CurrentSlimeType == uncovered_tile.CurrentSlimeType)
                            {
                                if (tile.I < min_i)
                                    min_i = tile.I;
                                if (tile.J < min_j)
                                    min_j = tile.J;
                                if (tile.I > max_i)
                                    max_i = tile.I;
                                if (tile.J > max_j)
                                    max_j = tile.J;

                                tiles.Add(tile);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        for (int i = min_i; i <= max_i; i++)
        {
            for (int j = min_j; j <= max_j; j++)
            {
                GameTile tile = Grid.GetTile(i, j, uncovered_tile.Owner);

                if (tile == null)
                    continue;

                switch (tile.CurrentState)
                {
                    case GameTile.TileState.EMPTY:

                        tile.SetSlime(uncovered_tile.Slime, GameTile.TileState.OCCUPIED);

                        break;
                    case GameTile.TileState.COVERED:

                        if (tile.CurrentSlimeType == uncovered_tile.CurrentSlimeType)
                            tile.SetSlime(uncovered_tile.Slime, GameTile.TileState.OCCUPIED);

                        break;
                    case GameTile.TileState.OCCUPIED:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    internal void HandleInput(SlimeType slime_type, int i, int j)
    {
        GameTile tile = Grid.GetTile(i, j);

        if (tile == null)
            return;

        tile.HandleInput(slime_type);
    }

    public void PassTurn()
    {
        if (CurrentPlayer == 0)
        {
            CurrentPlayer = 1;
        }
        else
        {
            CurrentPlayer = 0;
            EndRound();
        }

        UI.SetTurn(CurrentPlayer);

        if (!GamesparksManager.Instance.IsLocalPlayer(CurrentPlayer))
            GamesparksManager.Instance.SendPass();
    }

    void EndRound()
    {
        HandleAttacks();

        //TODO check if game over
        //else
        StartRound();
    }

    void StartRound()
    {
        GenerateSlimeTypes();

        CurrentMoveAllowance[0] = new Dictionary<GameTile.TileState, int>(MoveAllowance);
        CurrentMoveAllowance[1] = new Dictionary<GameTile.TileState, int>(MoveAllowance);
    }

    void HandleAttacks()
    {
        Dictionary<GameTile, int> pending_damage = new Dictionary<GameTile, int>();

        for (int i = 0; i < Grid.Width; i++)
        {
            for (int j = 0; j < Grid.Height; j++)
            {
                GameTile tile = Grid.GetTile(i, j, 0);

                if (tile == null)
                    return;

                GameTile opposite_tile = tile.GetOppositeTile();

                bool success = Damage(tile, opposite_tile);

                if (success)
                {
                    if (!pending_damage.ContainsKey(opposite_tile))
                        pending_damage.Add(opposite_tile, 0);

                    pending_damage[opposite_tile] += tile.Slime.Power;
                }

                success = Damage(opposite_tile, tile);

                if (success)
                {
                    if (!pending_damage.ContainsKey(tile))
                        pending_damage.Add(tile, 0);

                    pending_damage[tile] += opposite_tile.Slime.Power;
                }
            }
        }

        foreach (var damage in pending_damage)
        {
            damage.Key.Damage(damage.Value);
        }
    }

    private bool Damage(GameTile tile, GameTile opposite_tile)
    {
        if (tile == null)
            return false;

        if (opposite_tile == null)
            return false;

        if (tile.CurrentState != GameTile.TileState.OCCUPIED)
            return false;

        if (opposite_tile.CurrentState != GameTile.TileState.OCCUPIED)
            return false;

        if (opposite_tile.CurrentSlimeType == tile.CurrentSlimeType)
            return false;

        if (opposite_tile.Slime == null)
            return false;

        return true;
    }

    internal bool HasStillMoves(GameTile.TileState moveType, int owner)
    {
        return CurrentMoveAllowance[owner][moveType] > 0;
    }

    internal void RegisterMove(GameTile.TileState moveType, int owner)
    {
        CurrentMoveAllowance[owner][moveType]--;
    }
}
