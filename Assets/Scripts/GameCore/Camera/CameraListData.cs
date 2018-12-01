using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CameraListData : ScriptableObject
{
    public string Name;
    public List<CameraState> tpCameraStates;

    public CameraListData()
    {
        tpCameraStates = new List<CameraState>();
        tpCameraStates.Add(new CameraState("Default"));
    }

}
