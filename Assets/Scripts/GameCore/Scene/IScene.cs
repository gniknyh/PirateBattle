using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IScene:System.Object
{
    private string _sceneName = string.Empty;

    public string SceneName
    {
        get { return _sceneName; }
        set { _sceneName = value; }
    }

    protected SceneController _sceneController = null;
    public SceneController SController
    {
        get { return _sceneController; }
        set { _sceneController = value; }
    }

    public IScene(string sceneName,SceneController controller)
    {
        this._sceneName = sceneName;
        this._sceneController = controller;
    }

    public virtual void InitScene()
    {
        
    }

    public virtual void UpdateScene()
    {
        
    }

    public override string ToString()
    {
        return string.Format("SceneName --> {0}", _sceneName);
    }

    public virtual void ExitScene()
    {
        
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
