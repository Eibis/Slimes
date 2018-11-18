using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using System;
using GameSparks.Core;
using GameSparks.RT;

public class RTSessionInfo
{
    private string hostURL;
    public string GetHostURL() { return this.hostURL; }
    private string acccessToken;
    public string GetAccessToken() { return this.acccessToken; }
    private int portID;
    public int GetPortID() { return this.portID; }
    private string matchID;
    public string GetMatchID() { return this.matchID; }

    private List<RTPlayer> playerList = new List<RTPlayer>();
    public List<RTPlayer> GetPlayerList()
    {
        return playerList;
    }

    /// <summary>
    /// Creates a new RTSession object which is held until a new RT session is created
    /// </summary>
    /// <param name="_message">Message.</param>
    public RTSessionInfo(MatchFoundMessage _message)
    {
        portID = (int)_message.Port;
        hostURL = _message.Host;
        acccessToken = _message.AccessToken;
        matchID = _message.MatchId;
        // we loop through each participant and get their peerId and display name //
        foreach (MatchFoundMessage._Participant p in _message.Participants)
        {
            playerList.Add(new RTPlayer(p.DisplayName, p.Id, (int)p.PeerId));
        }
    }

    public class RTPlayer
    {
        public RTPlayer(string _displayName, string _id, int _peerId)
        {
            this.displayName = _displayName;
            this.id = _id;
            this.peerId = _peerId;
        }

        public string displayName;
        public string id;
        public int peerId;
        public bool isOnline;
    }
}

public class GamesparksManager : MonoBehaviour
{
    public static GamesparksManager Instance { get; private set; }

    GameSparksUnity SlGS;
    GameSparksRTUnity RtGS;
    RTSessionInfo sessionInfo;

    public string UserId { get; private set; }
    public bool IsHost { get; private set; }
    public int PeerId { get; private set; }
    public Action MatchFoundCallback { get; private set; }

    void Awake ()
    {
        Instance = this;

        SlGS = gameObject.AddComponent<GameSparksUnity>();
	}

    private void OnMatchFound(MatchFoundMessage resp)
    {
        Debug.Log(resp.JSONString);

        bool assigned = false;

        foreach (var participant in resp.Participants)
        {
            if (participant.Id == UserId)
            { 
                PeerId = (int)participant.PeerId;

                if (!assigned)
                {
                    IsHost = true;

                    assigned = true;
                }
            }
        }

        sessionInfo = new RTSessionInfo(resp);
        StartNewSession(sessionInfo, resp);
    }

    private void StartNewSession(RTSessionInfo sessionInfo, MatchFoundMessage resp)
    {
        Debug.Log("GSM| Creating New RT Session Instance...");

        RtGS = gameObject.AddComponent<GameSparksRTUnity>();

        RtGS.Configure(resp,
            (peerId) => { OnPlayerConnectedToGame(peerId); },
            (peerId) => { OnPlayerDisconnected(peerId); },
            (ready) => { OnRTReady(ready); },
            (packet) => { OnPacketReceived(packet); }
        );
        RtGS.Connect();
    }

    private void OnPlayerConnectedToGame(int _peerId)
    {
        Debug.Log("GSM| Player Connected, " + _peerId);
    }

    private void OnPlayerDisconnected(int _peerId)
    {
        Debug.Log("GSM| Player Disconnected, " + _peerId);
    }

    private void OnRTReady(bool ready)
    {
        if(ready)
        { 
            if (MatchFoundCallback != null)
                MatchFoundCallback();
        }
    }

    private void OnPacketReceived(RTPacket packet)
    {
        if (packet.OpCode == 1)
        {
            SlimeType slime_type;

            int? type = packet.Data.GetInt(3);

            if (type != null)
                slime_type = (SlimeType)type;
            else
                slime_type = SlimeType.NONE;

            int i = (int)packet.Data.GetInt(1);
            int j = (int)packet.Data.GetInt(2);

            Game.Instance.HandleInput(slime_type, i, j);
        }
        else if (packet.OpCode == 2)
        {
            Game.Instance.PassTurn();
        }
    }

    internal void SendInputData(SlimeType slimeType, int i, int j)
    {
        int other_player = PeerId == 1 ? 2 : 1;
        int[] targets = new int[] { other_player };

        RTData data = new RTData();
        data.SetInt(3, (int)slimeType);
        data.SetInt(1, i);
        data.SetInt(2, j);

        RtGS.SendData(1, GameSparksRT.DeliveryIntent.RELIABLE, data, targets);
    }

    internal void SendPass()
    {
        int other_player = PeerId == 1 ? 2 : 1;
        int[] targets = new int[] { other_player };

        RTData data = new RTData();

        RtGS.SendData(2, GameSparksRT.DeliveryIntent.RELIABLE, data, targets);
    }

    public void Authenticate(Action callback)
    {
        DeviceAuthenticationRequest request = new DeviceAuthenticationRequest();

        request.Send(
            (resp) => 
            {
                Debug.Log(resp.JSONString);

                UserId = resp.UserId;

                if (callback != null)
                    callback();
            },
            (resp) => 
            {
                Debug.LogError(resp.JSONString);

            }
        );
    }

    internal void FindPlayers(Action callback)
    {
        MatchFoundCallback = callback;

        GameSparks.Api.Messages.MatchFoundMessage.Listener += (resp) => 
        {
            OnMatchFound(resp);
        };

        MatchmakingRequest request = new MatchmakingRequest();

        request.SetSkill(0);
        request.SetMatchShortCode("SLIME_1V1");

        request.Send(
                   (resp) =>
                   {
                       Debug.Log(resp.JSONString);
                   },
                   (resp) =>
                   {
                       Debug.LogError(resp.JSONString);

                   }
               );
    }

    public bool IsLocalPlayer(int player)
    {
        return player == (PeerId - 1);
    }
}
