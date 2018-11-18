using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchMakingPanel : MonoBehaviour
{
    public TextMeshProUGUI FeedbackText;

	void Start ()
    {
        FeedbackText.text = "Authenticating...";

        StartCoroutine(Co_Authenticate());
	}

    private IEnumerator Co_Authenticate()
    {
        yield return new WaitForSeconds(0.5f);
        GamesparksManager.Instance.Authenticate(OnAuthenticate);
    }

    private void OnAuthenticate()
    {
        FeedbackText.text = "Authenticated! User Id: " + GamesparksManager.Instance.UserId + "\n Searching Players...";

        GamesparksManager.Instance.FindPlayers(OnPlayersFound);
    }

    private void OnPlayersFound()
    {
        FeedbackText.text = "Found players! Game will start in 3 seconds.";

        //TODO rtsession and set host and set players

        StartCoroutine(Co_StartGame());
    }

    private IEnumerator Co_StartGame()
    {
        yield return new WaitForSeconds(3.0f);

        gameObject.SetActive(false);
        Game.Instance.Init();
    }
}
