using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 开始场景，负责游戏片头，加载游戏数据，出现游戏主画面，玩家登陆游戏等。
/// </summary>
public class StartScene:IScene{
    public StartScene(string sceneName, SceneController controller) : base(sceneName, controller)
    {

    }
}
