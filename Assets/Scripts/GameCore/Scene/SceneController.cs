#define Debug

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController
{

    private IScene _scene = null;

    public SceneController() { }

    public void OnInit()
    {
        
    }

    public void SetScene(IScene scene,string sceneName)
    {
#if (Debug)
        Debug.Log(scene.ToString());
#endif
        SceneManager.LoadScene(sceneName);
        //通知上一个Scene结束
        if(scene != null)
            scene.ExitScene();

        this._scene = scene;

    }

    public void OnUpdate()
    {
        _scene.UpdateScene();
    }
}
