using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class CharacterData : MonoBehaviour
{
    [Header("----- 生命值和体力值---------")]
    public float maxHealth = 100f;

    public float currentHealth;
    //恢复的生命值
    public float healthRecovery = 0f;
    public float healthRecoveryDelay = 0f;
    //最大的体力值
    public float maxStamina = 200f;
    public float currentStamina;
    //恢复体力值
    public float staminaRecovery = 1.2f;
    //是否无敌
    public bool invulnerable = false;

    protected float recoveryDelay;
    protected bool recoveringStamina;

    protected bool canRecovery;
    protected float currentHealthRecoveryDelay;

    protected bool isDead;


    //死亡形式
    public enum DeathBy
    { 
        //动画的形式死亡
        Animation,
       //动画或者Ragdoll形式死亡
        AnimationWithRagdoll,
        //Ragdoll形式死亡
        Ragdoll
    }

    public DeathBy deathBy = DeathBy.Animation;
    [HideInInspector] public Animator animator;
    [HideInInspector] public bool ragdolled { get; set; }

    public Transform GetTransform
    {
        get { return transform; }
    }

    #region 规定一些接口
    public virtual void ResetRagdoll()
    {

    }

    public virtual void RagdollGettingUp()
    {

    }

    public virtual void EnableRagdoll()
    {

    }

    #endregion
}
