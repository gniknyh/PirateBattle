using UnityEngine;
using System.Collections;

public enum CriticalHitType
{
    None,
    Vertical,
    Horizontal,
}

/// <summary>
/// 伤害包数据
/// </summary>
[System.Serializable]
public class DamageData
{
    public string animName = string.Empty;
    public float damage;
    //持续时间
    public float duration;
    public float comboResetTime = 0.5f;
    [HideInInspector]
    public float lastAttackTime;
    public float attackEndTime = 1f;
    //受击特效
    public string hitSFX = string.Empty;
    //是否击倒
    public bool knockDown = false;
    //最佳攻击移动距离
    public float moveDistance;

    public float attackMoveStartTime;
    public float attackMoveEndTime;

    [HideInInspector]
    public float zanshinEndTime;

    //第几套连击
    [HideInInspector]
    public int comboIndex;
    [HideInInspector]
    public int comboStep;

    // hit parameters
    public float HitMomentum;
    //重击类型
    [HideInInspector]
    public CriticalHitType criticalHitType;
    // 是否可以破防御
    public bool breakBlock;
    public bool useImpuls = false;
    //是否播放慢动作特效
    public bool slowMotionEffect = false;
    public bool DefenceOverride = false;
    public bool isGroundAttack = false;
  
    [HideInInspector]
    public bool firstAttackInCombo = true;//第一次攻击 
    [HideInInspector]
    public bool lastAttackInCombo = false; //最后一次攻击
    
    [HideInInspector]
    public bool fullCombo = false;//是否打完一套连击
    [HideInInspector]
    public bool continueAttackCombo = false; // 是否连击 
    public float attackRange = 2f;
    public float hitAngle = 30f;

    [Header("添加动画事件信息")]
    public AnimEventInfo animEventInfo;

    [Header("Hit Collider Settings")]
    public float CollSize;
    public float collDistance;
    public float collHeight;

    [HideInInspector]
    public GameObject inflictor;//攻击者

    public DamageData(string animName, float damage, GameObject inflictor, float moveDistance, 
                    float attackEndTime, CriticalHitType criticalType, bool knockDown)

    {
        this.animName = animName;
        this.damage = damage;
        this.inflictor = inflictor;

        this.moveDistance = moveDistance;

        this.attackEndTime = attackEndTime;

        this.criticalHitType = criticalType;
        this.knockDown = knockDown;

    }

    /// <summary>
    /// 获取伤害值
    /// </summary>
    /// <param name="dmg"></param>
    /// <param name="def"></param>
    /// <param name="atk"></param>
    /// <returns></returns>
    public float getDamage(int dmg, int def, int atk)
    {
        dmg = def ^ 2 * (atk + def);
        return dmg;
    }
}

