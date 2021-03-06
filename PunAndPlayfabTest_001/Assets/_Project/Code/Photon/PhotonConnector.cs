using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonConnector : MonoBehaviourPunCallbacks
{
    public static Action GetPhotonFriends = delegate { };

    #region Unity Methods
    void Start()
    {
        //string randomName = $"Tester{Guid.NewGuid().ToString()}";
        string nickname = PlayerPrefs.GetString("USERNAME");
        ConnectToPhoton(nickname);
    }
    #endregion

    #region Private Methods
    void ConnectToPhoton(string nickName)
    {
        Debug.Log($"Connect to Photon as {nickName}");
        PhotonNetwork.AuthValues = new AuthenticationValues(nickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();
    }

    void CreatePhotonRoom(string roomName)
    {
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }
    #endregion

    #region Public Methods
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log($"You have connected to the Photon Master Server");

        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("You have connected to a Photon Lobby");

        //CreatePhotonRoom("TestRoom");
        GetPhotonFriends?.Invoke();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"You have created a Photon Room named {PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log($"You have joined the Photon Room named {PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnLeftRoom()
    {
        Debug.Log("You have left a Photon Room");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"You failed to join a Photon room {message}");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Another player has joined the room {newPlayer.UserId}");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player has left the room {otherPlayer.UserId}");
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client is {newMasterClient.UserId}");
    }
    #endregion
}
