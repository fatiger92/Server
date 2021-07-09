using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Linq;
using UnityEngine;

public class PlayfabFriendController : MonoBehaviour
{
    public static Action<List<FriendInfo>> OnFriendListUpdated = delegate { };

    private List<FriendInfo> friends;
    void Awake()
    {
        friends = new List<FriendInfo>();

        PhotonConnector.GetPhotonFriends += HandleGetFriends;
        UIAddFriend.OnAddFriend += HandleAddPlayfabFriend;
        UIFriend.OnRemoveFriend += HandleRemoveFriend;
    }
    void OnDestroy()
    {
        PhotonConnector.GetPhotonFriends -= HandleGetFriends;
        UIAddFriend.OnAddFriend -= HandleAddPlayfabFriend;
        UIFriend.OnRemoveFriend -= HandleRemoveFriend;
    }
    private void HandleAddPlayfabFriend(string name)
    {
        var requset = new AddFriendRequest { FriendTitleDisplayName = name };
        PlayFabClientAPI.AddFriend(requset, OnFriendAddedSuccess, OnFailure);
    }

    private void HandleRemoveFriend(string name)
    {
        string id = friends.FirstOrDefault(f => f.TitleDisplayName == name).FriendPlayFabId;
        var request = new RemoveFriendRequest { FriendPlayFabId = id };
        PlayFabClientAPI.RemoveFriend(request, OnFriendsRemoveSuccess, OnFailure);
    }
    private void HandleGetFriends()
    {
        GetPlayfabFriends();
    }

    private void GetPlayfabFriends()
    {
        var request = new GetFriendsListRequest { IncludeSteamFriends = false, IncludeFacebookFriends = false, XboxToken = null };
        PlayFabClientAPI.GetFriendsList(request, OnFriendsListSuccess, OnFailure);
    }
    private void OnFriendAddedSuccess(AddFriendResult result)
    {
        GetPlayfabFriends();
    }
    private void OnFriendsListSuccess(GetFriendsListResult result)
    {
        friends = result.Friends;
        OnFriendListUpdated?.Invoke(result.Friends);
    }

    private void OnFriendsRemoveSuccess(RemoveFriendResult result)
    {
        Debug.Log("You successed remove friend");
        GetPlayfabFriends();
    }
    private void OnFailure(PlayFabError error)
    {
        Debug.Log($"Playfab Friend Error occured : {error.GenerateErrorReport()}");
    }

}
