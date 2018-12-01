using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTest : MonoBehaviour {

    private Animator anim;

	void Start () {
        anim = GetComponent<Animator>();
        Debug.Log("animlen:" + anim.GetCurrentAnimatorStateInfo(0).length);
        for (int i = 0; i < anim.GetCurrentAnimatorClipInfo(0).Length; i++)
        {
            Debug.Log("clipInfo:" + anim.GetCurrentAnimatorClipInfo(0)[i].clip.length);
        }
        Debug.Log("animlen2:" + anim.GetCurrentAnimatorClipInfo(0)[0].clip.length);

    }

    void Update () {
		
	}
}
