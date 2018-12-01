
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using System;

public class GameCamera : MonoBehaviour
{
    private static GameCamera _instance;
    public static GameCamera Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameCamera>();
                //DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    #region displayed properties    
    public Transform target;
    public float xMouseSensitivity = 3f;
    public float yMouseSensitivity = 3f;
    public float scrollSpeed = 10f;

    //平滑状态切换
    public float smoothBetweenState = 3f;
    public float smoothCameraRotation = 12f;

    //被裁切的层
    public LayerMask cullingLayer = 1 << 0;
    public float clipPlaneMargin;

    public bool showGizmos = false;
    // 用于调试
    public bool lockCamera = false;

    #endregion

    #region hide properties    

    [HideInInspector]
    public int indexList, indexLookPoint;
    //偏移
    //[HideInInspector]
    public float offsetPlayerPivot;
    [HideInInspector]
    public string currentStateName;
    //[HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public CameraState currentState;

    [HideInInspector]
    public CameraListData CameraStateList;
    //[HideInInspector]
    public Transform lockTarget;

    private CameraState lerpState;
    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 lookPoint;
    private Vector3 cPos;
    private Vector3 lastTargetPos;

    private Camera _camera;
    private float distance = 5f;

    private float mouseY = 0f;
    private float mouseX = 0f;

    private float targetHeight;
    private float currentZoom;
    private float desiredDistance;
    private float lastDistance;
    private bool useSmooth;
    private Vector3 lookTargetOffset;

    #endregion

    void Start()
    {
        Init();
    }

    public void Init()
    {
        //Cursor.visible = false;
        if (target == null)
            return;

        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x,
            currentTarget.position.y + offsetPlayerPivot, currentTarget.position.z);
        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;

        //HideFlags:控件物体的位蒙版，在检视面板销毁和可见。
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;

        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        ChangeState("Default", false);
        currentZoom = currentState.defaultDistance;
        distance = currentState.defaultDistance;
        targetHeight = currentState.height;
        useSmooth = true;
    }

    void FixedUpdate()
    {
        if (target == null || targetLookAt == null || currentState == null || lerpState == null)
            return;

        switch (currentState.cameraMode)
        {
            case CameraMode.FixedAngle:
                FixedAngleMovement();
                break;
            case CameraMode.FixedPoint:
                FixedPointCameraMovement();
                break;
            case CameraMode.FreeDirectional:
                FreeDirectionalMovement();
                break;
        }

    }

    private void FixedAngleMovement()
    {
        if(currentTarget == null)
            return;

        if (useSmooth)
            currentState.Slerp(lerpState, smoothBetweenState * Time.fixedDeltaTime);
        else
            currentState.CopyState(lerpState);

        if (currentState.useZoom)
        {
            currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
            distance = useSmooth ? Mathf.Lerp(distance, currentZoom, 2f * Time.fixedDeltaTime) :
                currentZoom;
        }
        else
        {
            distance = useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance,
                2f * Time.fixedDeltaTime) : currentState.defaultDistance;
            currentZoom = distance;
        }

        desiredDistance = distance;
        var camDir = (currentState.forward * targetLookAt.forward) +
            (currentState.right * targetLookAt.right);
        camDir = camDir.normalized;

        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y
            + offsetPlayerPivot, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.Lerp(currentTargetPos,
            targetPos, lerpState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        cPos = currentTargetPos + new Vector3(0, targetHeight, 0);

        lastDistance = useSmooth ? Mathf.Lerp(lastDistance, distance,
            2f * Time.fixedDeltaTime) : distance;
        targetHeight = useSmooth ? Mathf.Lerp(targetHeight,
            currentState.height, 2f * Time.fixedDeltaTime) : currentState.height;

        var lookPoint = cPos;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = useSmooth ? Quaternion.Slerp(targetLookAt.rotation,
            newRot, smoothCameraRotation * Time.fixedDeltaTime) : newRot;
        transform.position = cPos + (camDir * (distance));

        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);

        lookTargetOffset = Vector3.Lerp(lookTargetOffset, Vector3.zero, 1 * Time.fixedDeltaTime);       
        rotation.eulerAngles += currentState.rotationOffset + lookTargetOffset;
        transform.rotation = rotation;
    }

    /// <summary>
    /// fixed angle 摄像机行为
    /// </summary>
    private void FreeDirectionalMovement()
    {
        if (currentTarget == null)
            return;

        if (useSmooth)
            currentState.Slerp(lerpState, smoothBetweenState * Time.fixedDeltaTime);
        else
            currentState.CopyState(lerpState);

        if (currentState.useZoom)
        {
            currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
            distance = useSmooth ? Mathf.Lerp(distance, currentZoom, 2f * Time.fixedDeltaTime) : currentZoom;

        }
        else
        {
            distance = useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance,
                2f * Time.fixedDeltaTime) : currentState.defaultDistance;
            currentZoom = distance;
        }

        desiredDistance = distance;
        Vector3 camDir = (currentState.forward * targetLookAt.forward) + (currentState.right * targetLookAt.right);
        camDir = camDir.normalized;

        Vector3 targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y
            + offsetPlayerPivot, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.Lerp(currentTargetPos,
            targetPos, lerpState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        cPos = currentTargetPos + new Vector3(0, targetHeight, 0);
        lastTargetPos = targetPos + new Vector3(0, currentState.height, 0);

        RaycastHit hitInfo;
        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(cPos + (camDir * (distance)),
            clipPlaneMargin);
        ClipPlanePoints lastPoints = _camera.NearClipPlanePoints(lastTargetPos + (camDir * lastDistance),
            clipPlaneMargin);

        if (CullingRayCast(cPos, planePoints, out hitInfo, distance + 0.2f, cullingLayer))
            distance = desiredDistance;

        if (CullingRayCast(lastTargetPos, lastPoints, out hitInfo, lastDistance + 0.2f, cullingLayer))
        {
            float t = distance - 0.2f;
            t -= currentState.cullingMinDist;
            t /= (distance - currentState.cullingMinDist);
            targetHeight = Mathf.Lerp(currentState.cullingHeight, currentState.height,
                Mathf.Clamp(t, 0.0f, 1.0f));
            cPos = currentTargetPos + new Vector3(0, targetHeight, 0);
        }
        else
        {
            lastDistance = useSmooth ? Mathf.Lerp(lastDistance, distance,
                2f * Time.fixedDeltaTime) : distance;
            targetHeight = useSmooth ? Mathf.Lerp(targetHeight,
                currentState.height, 2f * Time.fixedDeltaTime) : currentState.height;
        }

        Vector3 lookPoint = cPos;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        //third person controller
        targetLookAt.rotation = useSmooth ? Quaternion.Slerp(targetLookAt.rotation,
            newRot, smoothCameraRotation * Time.fixedDeltaTime) : newRot;
        transform.position = cPos + (camDir * (distance));

        Quaternion rotation = Quaternion.LookRotation((lookPoint) - transform.position);


        if (lockTarget)
        {
            if (!(currentState.cameraMode.Equals(CameraMode.FixedAngle)))
            {
                var collider = lockTarget.GetComponent<Collider>();
                if (collider != null)
                {
                    var point = collider.bounds.center - transform.position;
                    var euler = Quaternion.LookRotation(point).eulerAngles - rotation.eulerAngles;
                    lookTargetOffset = euler;
                }
            }
        }
        else
        {
            lookTargetOffset = Vector3.Lerp(lookTargetOffset, Vector3.zero, 1 * Time.fixedDeltaTime);
        }

        rotation.eulerAngles += currentState.rotationOffset + lookTargetOffset;
        transform.rotation = rotation;
    }

    /// <summary>
    /// Custom Raycast using NearClipPlanesPoints
    /// 如果被墙挡道视觉了，就进行近平面裁减
    /// </summary>
    /// <param name="_to"></param>
    /// <param name="from"></param>
    /// <param name="hitInfo"></param>
    /// <param name="distance"></param>
    /// <param name="cullingLayer"></param>
    /// <returns></returns>
    private bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo,
        float distance, LayerMask cullingLayer)
    {
        bool value = false;
        if (showGizmos)
        {
            Debug.DrawRay(from, _to.LowerLeft - from);
            Debug.DrawLine(_to.LowerLeft, _to.LowerRight);
            Debug.DrawLine(_to.UpperLeft, _to.UpperRight);
            Debug.DrawLine(_to.UpperLeft, _to.LowerLeft);
            Debug.DrawLine(_to.UpperRight, _to.LowerRight);
            Debug.DrawRay(from, _to.LowerRight - from);
            Debug.DrawRay(from, _to.UpperLeft - from);
            Debug.DrawRay(from, _to.UpperRight - from);
        }

        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance)
                desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance)
                desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance)
                desiredDistance = hitInfo.distance;
        }

        return value;

    }

    private void FixedPointCameraMovement()
    {
        if (useSmooth)
        {
            currentState.Slerp(lerpState, smoothBetweenState * Time.fixedDeltaTime);
        }
        else
        {
            currentState.CopyState(lerpState);
        }

        Vector3 targetPos = new Vector3(currentTarget.position.x,
            currentTarget.position.y + offsetPlayerPivot + currentState.height, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.MoveTowards(currentTargetPos,
            targetPos, currentState.smoothFollow * Time.fixedDeltaTime) : targetPos;

        cPos = currentTargetPos;
        Vector3 pos = isValidFixedPoint ? currentState.lookPoints[indexLookPoint].position : transform.position;
        transform.position = useSmooth ? Vector3.Lerp(transform.position, pos, currentState.smoothFollow * Time.fixedDeltaTime) : pos;
        targetLookAt.position = cPos;

        if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
        {
            var rot = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation,
                rot, (currentState.smoothFollow * 0.5f) * Time.fixedDeltaTime) : rot;
        }
        else if (isValidFixedPoint)
        {
            var rot = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation, rot,
                (currentState.smoothFollow) * Time.fixedDeltaTime) : rot;
        }

    }

    /// <summary>
    /// Check if current state is a valid FixedPoint
    /// </summary>
    private bool isValidFixedPoint
    {
        get
        {
            return (currentState.lookPoints != null && currentState.cameraMode.Equals(CameraMode.FixedPoint)
                && (indexLookPoint < currentState.lookPoints.Count || currentState.lookPoints.Count > 0));
        }
    }

    /// <summary>
    /// Set the cursorObject for TopDown only
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    public void ChangeState(string stateName, bool hasSmooth)
    {

        if (currentState != null && currentState.Name.Equals(stateName)) return;
        //查找CameraState by stateName

        CameraState state = CameraStateList.tpCameraStates.Find((obj) =>
        { return obj.Name.Equals(stateName); });

        //Debug.Log("state:" + state);
        if (state != null)
        {
            currentStateName = stateName;
            lerpState = state;

            if (currentState != null && !hasSmooth)
                currentState.CopyState(state);
        }
        else
        {

            if (CameraStateList.tpCameraStates.Count > 0)
            {
                // 设置默认值
                state = CameraStateList.tpCameraStates[0];
                currentStateName = state.Name;

                lerpState = state;
                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
        }
        //不存在CameraState, 创建一个
        if (currentState == null)
        {
            currentState = new CameraState("Null");
            currentStateName = currentState.Name;
        }

        indexList = CameraStateList.tpCameraStates.IndexOf(state);
        currentZoom = state.defaultDistance;
        currentState.fixedAngle = new Vector3(mouseX, mouseY);
        useSmooth = hasSmooth;
        indexLookPoint = 0;
    }

    /// <summary>
    /// Change State using look at point if the cameraMode is FixedPoint  
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="pointName"></param>
    /// <param name="hasSmooth"></param>
    public void ChangeState(string stateName, string pointName, bool hasSmooth)
    {
        useSmooth = hasSmooth;
        if (!currentState.Name.Equals(stateName))
        {
            var state = CameraStateList.tpCameraStates.Find(delegate (CameraState obj)
            {
                return obj.Name.Equals(stateName);
            });

            if (state != null)
            {
                currentStateName = stateName;
                currentState.cameraMode = state.cameraMode;
                lerpState = state;

                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
            else
            {
                if (CameraStateList.tpCameraStates.Count > 0)
                {
                    state = CameraStateList.tpCameraStates[0];
                    currentStateName = state.Name;
                    currentState.cameraMode = state.cameraMode;
                    lerpState = state;
                    if (currentState != null && !hasSmooth)
                        currentState.CopyState(state);
                }
            }
            // in case a list of states does not exist, a default state will be created
            if (currentState == null)
            {
                currentState = new CameraState("Null");
                currentStateName = currentState.Name;
            }

            indexList = CameraStateList.tpCameraStates.IndexOf(state);
            currentZoom = state.defaultDistance;
            currentState.fixedAngle = new Vector3(mouseX, mouseY);
            indexLookPoint = 0;
        }

        if (currentState.cameraMode == CameraMode.FixedPoint)
        {
            var point = currentState.lookPoints.Find(delegate (LookPoint obj)
            {
                return obj.pointName.Equals(pointName);
            });
            if (point != null)
            {
                indexLookPoint = currentState.lookPoints.IndexOf(point);
            }
            else
            {
                indexLookPoint = 0;
            }
        }
    }

    /// <summary>    
    /// 变焦行为,通过滚动鼠标
    /// </summary>
    /// <param name="scrollValue"></param>
    /// <param name="zoomSpeed"></param>
    public void Zoom(float scrollValue)
    {
        currentZoom -= scrollValue * scrollSpeed;
    }

    public void RotateCamera(float x, float y)
    {
        //固定点，fixedAngle = Vector2.zero;
        if (currentState.cameraMode.Equals(CameraMode.FixedPoint)) return;

        if (!currentState.cameraMode.Equals(CameraMode.FixedAngle))
        {
            if (lockTarget)
            {
                CalculateLockOnPoint();
            }
            else
            {
                //free rotation
                mouseX += x * xMouseSensitivity;
                mouseY -= y * yMouseSensitivity;
                if (!lockCamera)
                {
                    mouseX = Extension.ClampAngle(mouseX, currentState.xMinLimit, currentState.xMaxLimit);
                    mouseY = Extension.ClampAngle(mouseY, currentState.yMinLimit, currentState.yMaxLimit);
                }
                else
                {
                    mouseY = currentTarget.root.localEulerAngles.x;
                    mouseX = currentTarget.root.localEulerAngles.y;
                }
            }
        }
        else
        {
            //fixed angle
            //Debug.Log("currentState.fixedAngle:" + currentState.fixedAngle);
            mouseX = currentState.fixedAngle.x;
            mouseY = currentState.fixedAngle.y;
        }


    }

    private void CalculateLockOnPoint()
    {
        if (currentState.cameraMode.Equals(CameraMode.FixedAngle) && lockTarget) return;
        Collider collider = lockTarget.GetComponent<Collider>();

        if (collider)
        {
            Vector3 point = collider.bounds.center;
            Vector3 relativePos = point - cPos;
            Quaternion rotation = Quaternion.LookRotation(relativePos);

            //clamp  eulerAngleX
            if (rotation.eulerAngles.x < -180)
            {
                mouseY = rotation.eulerAngles.x + 360;
            }
            else if (rotation.eulerAngles.x > 180)
            {
                mouseY = rotation.eulerAngles.x - 360;
            }
            else
            {
                mouseY = rotation.eulerAngles.x;
            }
            mouseX = rotation.eulerAngles.y;

        }
    }
}


