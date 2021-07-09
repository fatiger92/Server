using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDisplayFriends : MonoBehaviour
{
    [SerializeField] private Transform friendContainer;
    [SerializeField] private UIFriend uiFriendPrefab;

    void Awake()
     {
        PhotonFriendController.OnDisplayFriends += HandleDisplayFriends;   
    }


    private void OnDestroy()
    {
        PhotonFriendController.OnDisplayFriends -= HandleDisplayFriends;
    }

    private void HandleDisplayFriends(List<FriendInfo> friends)
    {
        foreach (Transform child in friendContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (FriendInfo friend in friends)
        {
            UIFriend uifriend = Instantiate(uiFriendPrefab, friendContainer);
            uifriend.Initialize(friend);
        }
    }
}
