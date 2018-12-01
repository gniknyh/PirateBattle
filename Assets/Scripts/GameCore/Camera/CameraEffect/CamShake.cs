using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour {
    public AnimationCurve camShakeY;
    public AnimationCurve camShakeX;
    public AnimationCurve camShakeZ;
    public float multiplier = 1f;
    public bool randomize;
    public float time = 0.5f;

    float CurrentFovTime;
    float FovTime;
    float FovStart;
    float FovCameraEnd;
    float CurrentCameraFov = 0;
    bool FovCameraOk;

    public void Shake(float intensity)
    {
        StartCoroutine(DoShake(intensity));
    }

    private IEnumerator DoShake(float scale)
    {

        Vector3 rand = new Vector3(getRandomValue(), getRandomValue(), getRandomValue());
        scale *= multiplier;

        float t = 0;
        while (t < time)
        {
            if (randomize)
            {
                transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale * rand.x, camShakeY.Evaluate(t) * scale * rand.y, camShakeZ.Evaluate(t) * scale * rand.z);
            }
            else
            {
                transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale, camShakeY.Evaluate(t) * scale, camShakeZ.Evaluate(t) * scale);
            }

            t += Time.deltaTime / time;
            yield return null;
        }
        transform.localPosition = Vector3.zero;
    }

    private int getRandomValue()
    {
        int[] i = { -1, 1 };
        return i[Random.Range(0, 2)];
    }

    public void InterpolateFov(float newFov, float inTime)
    {
        CurrentFovTime = 0;
        FovTime = inTime;
        FovStart = CurrentCameraFov;
        FovCameraEnd = newFov;
        FovCameraOk = false;
    }

    void UpdateFov()
    {
        if (!FovCameraOk)
        {
            CurrentFovTime += Time.deltaTime;

            if (CurrentFovTime > FovTime)
            {
                CurrentFovTime = FovTime;
                FovCameraOk = true;
            }

        }
        GetComponent<Camera>().fov = CurrentCameraFov;
    }
}
