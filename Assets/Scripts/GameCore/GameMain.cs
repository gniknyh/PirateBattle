using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{

    private SceneController sceneController = null;


    void Awake()
    {

    }

    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        //sceneController = new SceneController();
        //sceneController.SetScene(new StartScene("",sceneController), "");
        //Game.Instance.Init(this);      
    }


    void Update()
    {
        //if(sceneController != null)
        //    sceneController.OnUpdate();

        //Game.Instance.Update();
    }

    void OnDestroy()
    {
        //Game.Instance.Destroy();
    }

    void OnApplicationQuit()
    {

    }

    void OnApplicationPause(bool pause)
    {

    }
}
