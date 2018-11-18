using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    public enum TileState
    {
        EMPTY,
        COVERED,
        OCCUPIED
    }

    public TileState CurrentState { get; private set; }

    public SlimeType CurrentSlimeType
    {
        get
        {
            if (Slime == null)
                return SlimeType.NONE;

            return Slime.Type;
        }
    }

    public SlimeType CurrentSlimeMenaceType;

    public Slime Slime { get; private set; }
    public int Owner { get; private set; }

    public int I { get; private set; }
    public int J { get; private set; }

    public SpriteRenderer Sprite;
    public SpriteRenderer MenaceSprite;
    public TextMeshPro DamageText;

    internal void SetState(TileState state)
    {
        CurrentState = state;

        if (GamesparksManager.Instance.IsLocalPlayer(Owner) || CurrentState == TileState.OCCUPIED)
        {
            switch (CurrentSlimeType)
            {
                case SlimeType.NONE:

                    Sprite.color = Color.white;

                    break;
                case SlimeType.RED:

                    Sprite.color = Color.red;

                    break;
                case SlimeType.GREEN:

                    Sprite.color = Color.green;

                    break;
                case SlimeType.BLUE:

                    Sprite.color = Color.blue;

                    break;
                default:
                    break;
            }
        }
        else
        {
            Sprite.color = Color.white;
        }

        switch (CurrentState)
        {
            case TileState.EMPTY:

                DamageText.enabled = false;

                break;
            case TileState.COVERED:
                { 
                    Color c = Sprite.color;
                    c.a = 0.35f;
                    Sprite.color = c;
                }
                break;
            case TileState.OCCUPIED:
                { 
                    Color c = Sprite.color;
                    c.a = 1.0f;
                    Sprite.color = c;
                }
                break;
            default:
                break;
        }
    }

    internal void SetMenaceState(SlimeType state)
    {
        CurrentSlimeMenaceType = state;

        MenaceSprite.gameObject.SetActive(true);

        Color c;

        switch (CurrentSlimeMenaceType)
        {
            case SlimeType.NONE:

                MenaceSprite.gameObject.SetActive(false);

                c = Color.black;

                break;
            case SlimeType.RED:

                c = Color.red;

                break;
            case SlimeType.GREEN:

                c = Color.green;

                break;
            case SlimeType.BLUE:

                c = Color.blue;

                break;
            default:

                c = Color.black;

                break;
        }

        c.a = MenaceSprite.color.a;
        MenaceSprite.color = c;
    }

    internal void Reset()
    {
        Slime = null;
        SetState(TileState.EMPTY);

        GameTile opposite = GetOppositeTile();
        opposite.SetMenaceState(SlimeType.NONE);
    }

    internal void SetOwner(int player)
    {
        Owner = player;

        MenaceSprite.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (Game.Instance.CurrentPlayer != Owner)
            return;

        if (!GamesparksManager.Instance.IsLocalPlayer(Owner))
            return;

        SlimeType slimeType = Game.Instance.GetNextSlime(Owner);
        HandleInput(slimeType);

        GamesparksManager.Instance.SendInputData(slimeType, I, J);
    }

    public void HandleInput(SlimeType slimeType)
    {
        switch (CurrentState)
        {
            case TileState.EMPTY:

                if (!Game.Instance.HasStillMoves(TileState.EMPTY, Owner))
                {
                    Debug.Log("No moves left: " + TileState.EMPTY);
                    return;
                }

                Slime new_slime = new Slime(slimeType);
                SetSlime(new_slime);
                SetState(TileState.COVERED);
                Game.Instance.RegisterMove(TileState.EMPTY, Owner);

                break;
            case TileState.COVERED:

                if (!Game.Instance.HasStillMoves(TileState.COVERED, Owner))
                {
                    Debug.Log("No moves left: " + TileState.COVERED);
                    return;
                }

                Game.Instance.Uncover(I, J);
                SetState(TileState.OCCUPIED);

                GameTile opposite_tile = GetOppositeTile();
                opposite_tile.SetMenaceState(Slime.Type);

                Game.Instance.RegisterMove(TileState.COVERED, Owner);

                break;
            case TileState.OCCUPIED:

                Debug.LogWarning("Already Occupied, nothing happens");

                break;
            default:
                break;
        }
    }

    internal void SetIndices(int i, int j)
    {
        I = i;
        J = j;

        name = "Tile" + i + "_" + j;
    }

    internal void Damage(int value)
    {
        Debug.Log("Damaged " + value);

        Slime.Damage(value);

        if(Slime != null)
            DamageText.text = Slime.Health.ToString();
    }

    internal void SetSlime(Slime slime, TileState newState = TileState.EMPTY)
    {
        Slime = slime;

        DamageText.enabled = true;

        Slime.AddTile(this);

        if (newState != TileState.EMPTY)
            CurrentState = newState;

        if (CurrentState == TileState.OCCUPIED)
        {
            GameTile opposite_tile = GetOppositeTile();
            opposite_tile.SetMenaceState(Slime.Type);
        }

        if (newState != TileState.EMPTY)
            SetState(CurrentState);
    }

    public void UpdateDamageText()
    {
        DamageText.text = Slime.Health.ToString();
    }

    internal GameTile GetOppositeTile()
    {
        int height_offset = Owner == 1 ? -Game.Instance.Grid.Height : Game.Instance.Grid.Height;

        int owner = Owner == 1 ? 0 : 1;

        return Game.Instance.Grid.GetTile(I, J + height_offset, owner);
    }
}
