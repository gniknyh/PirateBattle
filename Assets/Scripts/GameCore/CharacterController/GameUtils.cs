using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimEventInfo
{
    public string funcName = "CheckForHit";
    public float eventTime = 0.3f;
    public string stringParameter;
    public float floatParameter;
    public int intParameter;
    public Object objectReferenceParameter;
    public AnimationClip clip;
}

public static class GameUtils 
{
    public static void AddAnimatorEvents(Animator animator, List<AnimEventInfo> aInfoList)
    {
        AnimationEvent animEvent = null;
        if (aInfoList.Count > 0)
        {
            for (int i = 0; i < aInfoList.Count; i++)
            {
                if (aInfoList[i].clip.events.Length == 0)
                {
                    animEvent = new AnimationEvent();
                    animEvent.functionName = aInfoList[i].funcName;
                    animEvent.time = aInfoList[i].eventTime;
                    animEvent.stringParameter = aInfoList[i].stringParameter;
                    animEvent.floatParameter = aInfoList[i].floatParameter;
                    animEvent.intParameter = aInfoList[i].intParameter;
                    animEvent.objectReferenceParameter = aInfoList[i].objectReferenceParameter;
                    aInfoList[i].clip.AddEvent(animEvent);
                }
            }
        }

        //重新绑定动画器的所有动画的属性和网格数据。
        animator.Rebind();
    }
}
