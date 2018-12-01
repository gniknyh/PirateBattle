using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SAISensor;

[ExecuteInEditMode]
public class RangeSensor : MonoBehaviour
{
    public enum UpdateMode
    {
        FixedInterval,
        Manual
    }

    public UpdateMode SensorUpdateMode;
    public float SensorRange = 10f;
    public LayerMask DetectsOnLayers;
    // 轮询时间间隔
    public float CheckInterval = 0.21f;

    [HideInInspector]
    public Dictionary<int, Collider> detectedObjects = new Dictionary<int, Collider>();
    // 在这个列表中的任何游戏对象都不会被这个传感器检测到
    public GameObject[] IgnoreList;
    // 物体的能见度
    public Dictionary<int, Collider> objectVisibility = new Dictionary<int, Collider>();

    Vector3 leftRayPoint;
    Vector3 rightRayPoint;
    public float viewDistance = 5f;
    public float fov = 45f;
    List<Collider> canSeeObjects = new List<Collider>();

    protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
    protected static readonly Color GizmoBlockedColor = Color.red;

    private float lastTime = 0f;

    protected void Start()
    {
        lastTime = Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            if (SensorUpdateMode == UpdateMode.Manual)
            {
                testSensor();
            }
            else if (SensorUpdateMode == UpdateMode.FixedInterval)
            {
                if (Time.fixedDeltaTime - CheckInterval > lastTime)
                {
                    testSensor();
                    lastTime = Time.fixedDeltaTime;
                }

            }
        }
    }

    private void testSensor()
    {
        var sensedColliders = Physics.OverlapSphere(transform.position, SensorRange, DetectsOnLayers);
        int key;
        bool ignore;
        Vector3 rayDir;
        for (int i = 0; i < sensedColliders.Length; i++)
        {
            rayDir = (sensedColliders[i].transform.position - transform.position).normalized;
            rayDir.y = 0;
            if (Vector3.Angle(sensedColliders[i].transform.position, transform.forward) <= fov)
            {
                canSeeObjects.Add(sensedColliders[i]);
            }
            else
                continue;

        }

        for (int i = 0; i < canSeeObjects.Count; i++)
        {
            key = canSeeObjects[i].gameObject.GetInstanceID();
            ignore = false;
            if (IgnoreList.Length > 0)
            {
                for (int j = 0; j < IgnoreList.Length; j++)
                {
                    if (IgnoreList[j].GetInstanceID() == key)
                    {
                        ignore = true;
                        break;
                    }
                }
                if (!ignore)
                {
                    if (!detectedObjects.ContainsKey(key))
                    {
                        detectedObjects.Add(key, sensedColliders[i]);
                    }
                    else
                    {
                        //更新值
                        detectedObjects[key] = sensedColliders[i];
                    }
                }
            }
            else
            {
                if (!detectedObjects.ContainsKey(key))
                {
                    detectedObjects.Add(key, sensedColliders[i]);
                }
                else
                {
                    //更新值
                    detectedObjects[key] = sensedColliders[i];
                }
            }

        }

    }

    void reset()
    {
        detectedObjects.Clear();
    }

    public void OnDrawGizmosSelected()
    {
        if (!isActiveAndEnabled) return;

        Gizmos.color = GizmoColor;
        Gizmos.DrawWireSphere(transform.position, SensorRange);

        //弧长
        float fovRadius = fov * Mathf.PI / 180.0f;
        leftRayPoint = transform.TransformPoint(new Vector3(viewDistance * Mathf.Sin(fovRadius), 0, viewDistance * Mathf.Cos(fovRadius)));
        rightRayPoint = transform.TransformPoint(new Vector3(-viewDistance * Mathf.Sin(fovRadius), 0, viewDistance * Mathf.Cos(fovRadius)));

        Gizmos.color = new Color(250 / 255f, 0, 0);
        var drawPos = transform.position + Vector3.up * 1.8f;
        Gizmos.DrawLine(drawPos, drawPos + transform.forward * viewDistance);
        Gizmos.DrawLine(drawPos, leftRayPoint + Vector3.up * 1.8f);
        Gizmos.DrawLine(drawPos, rightRayPoint + Vector3.up * 1.8f);

    }
}