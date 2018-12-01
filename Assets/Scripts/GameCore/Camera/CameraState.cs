using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraMode
{
    FixedAngle,//固定视角,top down 
    FreeDirectional,//自由视角,这个预留的
    FixedPoint, //固定点
    Moba, // moba类型的移动
}

[System.Serializable]
public class LookPoint
{
    public string pointName;
    public Vector3 position;
    public Vector3 eulerAngle;
    public bool freeRotation;
}

[System.Serializable]
public class CameraState
{
    public string Name = "Default";
    public bool useZoom = false;

    public float forward = -1f;
    public float right = 0.35f;

    public float defaultDistance = 1.5f;
    public float maxDistance = 3f;
    public float minDistance = 0.5f ;

    public float height = 1.5f;
    public float smoothFollow = 10f;

    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;
    public float xMinLimit = -360f;
    public float xMaxLimit = 360f;

    public Vector3 rotationOffset = Vector3.zero;

    public float cullingHeight = 1f;
    public float cullingMinDist = 0.1f;

    public Vector2 fixedAngle = Vector2.zero;
    public List<LookPoint> lookPoints;
    public CameraMode cameraMode = CameraMode.FixedAngle;

    public CameraState(string name)
    {
        Name = name;
        lookPoints = new List<LookPoint>();
    }
}




