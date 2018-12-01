using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 游戏管理器，包括脚本执行顺序
/// </summary>
public class Game
{
    private static Game _instance = new Game();

    public static Game Instance
    {
        get { return _instance; }
    }

    private List<Action> frameActionList = new List<Action>();

    public void Init(GameMain main)
    {
        //Manager
        //NotificationManager.Instance.Init();
        //TimerManager.Instance.Init();
        //NetProcessManager.Instance.Init();
    }

    public void Update()
    {
        //NotificationManager.Instance.Update();
        //TimerManager.Instance.Update();
        //DelayManager.Instance.Update();
        //NetProcessManager.Instance.Update();
        //主循环
        for (int i = 0; i < frameActionList.Count; i++)
        {
            Action action = frameActionList[i];
            if (action != null)
            {
                action();
            }
        }
    }

    public void Destroy()
    {
        //NotificationManager.Instance.Destroy();
        //TimerManager.Instance.Destroy();
    }

    public void AddFrameAction(Action action)
    {
        if (!frameActionList.Contains(action))
        {
            frameActionList.Add(action);
        }
    }

    public void RemoveFrameAction(Action action)
    {
        if (frameActionList.Contains(action))
        {
            frameActionList.Remove(action);
        }
    }


}
