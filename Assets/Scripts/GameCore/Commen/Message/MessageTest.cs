using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageTest : MonoBehaviour {

    //void OnEnable()
    //{
    //    Messenger.AddListener("start game", StartGame);
    //}

    //void OnDisable()
    //{
    //    Debug.Log("OnDisable");
    //    Messenger.RemoveListener("start game", StartGame);
    //}

    void Start()
    {
        Messenger.Broadcast("start game");
    }

}
