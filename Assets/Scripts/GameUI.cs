using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image Choice_0;
    public Image Choice_1;
    public Text RoundText;

    internal void SetNextSlime()
    {
        SlimeType p;
        Image choice = null;

        if (GamesparksManager.Instance.IsLocalPlayer(0))
        {
            p = Game.Instance.GetNextSlime(0);
            choice = Choice_0;
        }
        else
        {
            p = Game.Instance.GetNextSlime(1);
            choice = Choice_1;
        }

        switch (p)
        {
            case SlimeType.NONE:
                break;
            case SlimeType.RED:

                choice.color = Color.red;

                break;
            case SlimeType.GREEN:

                choice.color = Color.green;

                break;
            case SlimeType.BLUE:

                choice.color = Color.blue;

                break;
            default:
                break;
        }
    }

    internal void SetTurn(int currentPlayer)
    {
        RoundText.text = "Player " + (currentPlayer + 1) + "'s turn";
    }
}
