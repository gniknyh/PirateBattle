using UnityEngine;
using System.Collections;

/// <summary>
/// 专门对攻击动作做处理
/// </summary>
public class DoAttack
{
    public enum MyState
    {
        none,
        preparing,//攻击前准备阶段
        attacking,//攻击阶段
        finish,//攻击完成阶段
    }
    public MyState aState = MyState.none;

    // 攻击的目标
    public GameObject attackTarget;
    public DamageData dData;
    public AttackType attackType;
    public Vector3 attackDir;
    public bool isFinish = false;

    //武器攻击范围
    public float weaponRange = 1f;

    public float lastCombatTime = 0;

    private Quaternion finalRotation;
    private Quaternion startRotation;

    private Vector3 startPosition;
    private Vector3 finalPosition;

    private float currentRotationTime;
    private float rotationTime;

    private float moveTime;
    private float currentMoveTime;
    public float attackJumpTime;
    public float jumpHeight = 0.5f;

    //private float endOfStateTime;

    private bool rotationOK = false;
    private bool positionOK = false;

    //重击，致命一击
    private bool critical = false;

    private Animator animator;
    private Rigidbody rigidbody;
    private Transform myTransform;
    AnimatorStateInfo baseLayerStateInfo;
    private PlayerCombat playerCombat;
    private AIController ai;

    public DoAttack(Animator anim, Rigidbody rb, Transform t, PlayerCombat pc) {
        this.animator = anim;
        this.rigidbody = rb;
        this.myTransform = t;
        this.playerCombat = pc;
    }

    public DoAttack(Animator anim, Rigidbody rb, Transform t, AIController ai)
    {
        this.animator = anim;
        this.rigidbody = rb;
        this.myTransform = t;
        this.ai = ai;
    }

    public void OnInit(DamageData data)
    {
        isFinish = false;
        aState = MyState.preparing;
        dData = data;

        if (dData == null)
            Debug.LogError("dData == null");

        startRotation = myTransform.rotation;
        startPosition = myTransform.position;

        float angle = 0;
        //背刺杀
        bool backstab = false;

        float distance = 0;
        Vector3 dir = Vector3.zero;

        if (attackTarget)
        {
            dir = attackTarget.transform.position - myTransform.position;
            distance = dir.magnitude;

            if (distance > 0.1f)
            {
                dir.Normalize();
                angle = Vector3.Angle(myTransform.forward, dir);

                if (angle < 40 && Vector3.Angle(myTransform.forward, attackTarget.transform.forward) < 80)
                {
                    backstab = true;
                }
            }
            else
            {
                dir = myTransform.forward;
            }

            //Quaternion newRot = Quaternion.LookRotation(dir);
            //myTransform.rotation = newRot;
            finalRotation.SetLookRotation(dir);

            if (distance < weaponRange)
                finalPosition = startPosition;
            else
                finalPosition = attackTarget.transform.position - dir * weaponRange;

            //初始化
            moveTime = (finalPosition - startPosition).magnitude / 20.0f;
            rotationTime = angle / 720.0f;
        }
        else
        {
            //无攻击目标时
            dir = myTransform.forward;
            Quaternion newRot = Quaternion.LookRotation(dir);
            myTransform.rotation = newRot;
            //finalRotation.SetLookRotation(attackDir);

            rotationTime = Vector3.Angle(myTransform.forward, dir) / 720.0f;
            moveTime = 0;
        }
        attackDir = dir;

        rotationOK = rotationTime == 0;
        positionOK = moveTime == 0;

        currentRotationTime = 0;
        currentMoveTime = 0;

        if (dData.criticalHitType != CriticalHitType.None && attackTarget)
        {
            if (backstab)
                critical = true;
            else
            {
                critical = Random.Range(0, 100) < 20;
            }
        }
        else
        {
            critical = false;
        }

    }

    public void OnUpdate()
    {
        baseLayerStateInfo = playerCombat.baseLayerStateInfo;
        switch (aState)
        {
            case MyState.preparing:
                Prepare();
                break;
            case MyState.attacking:
                Attack();
                break;
            case MyState.finish:
                //Debug.Log(Time.timeSinceLevelLoad + " attack finished!!!");
                isFinish = true;
                break;
        }

    }

    /// <summary>
    /// 攻击前准备,对角色位置和旋转做调整
    /// </summary>
    public void Prepare()
    {
        bool dontMove = false;
        if (!rotationOK)
        {
            currentRotationTime += Time.deltaTime;

            if (currentRotationTime >= rotationTime)
            {
                currentRotationTime = rotationTime;
                rotationOK = true;
            }

            float progress = currentRotationTime / rotationTime;
            Quaternion newRot = Quaternion.Lerp(startRotation, finalRotation, progress);
            myTransform.rotation = newRot;

            if (Quaternion.Angle(newRot, finalRotation) > 20.0f)
            {
                dontMove = true;
            }

        }

        if (!dontMove && !positionOK)
        {
            currentMoveTime += Time.deltaTime;
            if (currentMoveTime >= moveTime)
            {
                currentMoveTime = moveTime;
                positionOK = true;
            }

            if (currentMoveTime > 0)
            {
                float progress = currentMoveTime / moveTime;
                Vector3 finalPos = Mathfs.Hermite(startPosition, finalPosition, progress);
                rigidbody.MovePosition(finalPos);

                if (Vector3.Distance(finalPos, finalPosition) <= 1.0f)
                {
                    //Debug.Log("PositionOK!!!");
                    positionOK = true;
                }
            }
        }

        if (rotationOK && positionOK)
        {
            aState = MyState.attacking;
            PlayAnim();
        }

    }

    public void Attack()
    {
        currentMoveTime += Time.deltaTime;

        if (baseLayerStateInfo.IsName("Attack." + dData.animName))
        {
            //Debug.Log("enter baseLayerStateInfo");

            //是否最后一次攻击
            if (dData.lastAttackInCombo)
            {
                if (baseLayerStateInfo.normalizedTime >= 0.95f)
                {
                    //Debug.Log(Time.timeSinceLevelLoad + " attack phase done");
                    aState = MyState.finish;
                }

            }
            else
            {
                if (baseLayerStateInfo.normalizedTime >= 0.9f)
                {
                    //Debug.Log(Time.timeSinceLevelLoad + " attack phase done");
                    aState = MyState.finish;
                }
            }
        }

        if (currentMoveTime >= moveTime)
            currentMoveTime = moveTime;

        if (currentMoveTime > 0 && currentMoveTime <= moveTime)
        {
            float progress = Mathf.Min(1.0f, currentMoveTime / moveTime);
            Vector3 finalPos = Mathfs.Hermite(startPosition, finalPosition, progress);

            rigidbody.MovePosition(finalPos);

            if (Vector3.Distance(finalPos, finalPosition) > 1.0f)
            {
                currentMoveTime = moveTime;
            }

        }

    }

    private void PlayAnim()
    {
        Debug.Log("dData.animName:" + dData.animName);
        animator.SetTrigger(dData.animName);
        //Debug.Log("state info:" + baseLayerStateInfo.IsName("Attack." + dData.animName));

        if (playerCombat.proTrailDistort == null || playerCombat.proTrailShort == null || playerCombat.proTraillong == null)
        {
            Debug.LogError("playerCombat.proTrailDistort == null || playerCombat.proTrailShort == null || playerCombat.proTraillong == null");
        }

        playerCombat.proTrailDistort.Activate();
        playerCombat.proTrailShort.Activate();
        playerCombat.proTraillong.Activate();

        startPosition = myTransform.position;
        finalPosition = startPosition + myTransform.forward * dData.moveDistance;
        moveTime = dData.attackMoveEndTime - dData.attackMoveStartTime;
        // move a little bit later
        currentMoveTime = - dData.attackMoveStartTime;

        //添加镜头效果
        //if (attackTarget)
        //{
        //    if (critical)
        //    {
        //        CameraBehaviour.Instance.InterpolateTimeScale(0.25f, 0.5f);
        //        CameraBehaviour.Instance.InterpolateFov(25, 0.5f);
        //        CameraBehaviour.Instance.Invoke("InterpolateScaleFovBack", 0.7f);
        //    }
        //    else if (attackType == AttackType.Fatality)
        //    {
        //        CameraBehaviour.Instance.InterpolateTimeScale(0.25f, 0.7f);
        //        CameraBehaviour.Instance.InterpolateFov(25, 0.65f);
        //        CameraBehaviour.Instance.Invoke("InterpolateScaleFovBack", 0.8f);
        //    }
        //}

    }
}
