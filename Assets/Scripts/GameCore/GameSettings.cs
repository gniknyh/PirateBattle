using UnityEngine;
using System.Collections;

/// <summary>
/// 保存所有的游戏设置数据
/// </summary>
[System.Serializable]
public class GameSettings : ScriptableObject
{
    public float timeScale = 1f;
    public int framerate = 60;
    public bool showFPSCounter = false;

    public float MusicVolume = .7f;
    public float SFXVolume = .9f;
}
