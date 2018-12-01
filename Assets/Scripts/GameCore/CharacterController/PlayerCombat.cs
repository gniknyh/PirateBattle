using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using SporeWeaponTrail;

public class PlayerCombat : MonoBehaviour,IDamagable
{
    public ComboListData comboDataList;
    public SwordLevel swordLevel = SwordLevel.One;
    //每个连击只能攻击的次数
    public readonly int COMBOTSTEP_LENGTH = 5;
    private PlayerState playerState = null;
    public bool isInvulnerable = false;
    public bool isAlive = true;

    [Space(5)]
    [Header("Weapon Trail")]
    public WeaponTrail proTrailDistort;//扭曲
    public WeaponTrail proTrailShort;
    public WeaponTrail proTraillong;

    [Space(5)]
    [Header("Linked Components")]
    public Transform weaponBone;
    public Transform attackPoint;
    public GameObject attackTarget;

    public float weaponBonetPositionWeight;
    public float weaponBonetRotationWeight;

    [Space(5)]
    [Header("Attack Data & Combos")]
    public float weaponRange = 2f;
    private int attackNum = -1;

    private DamageData StabOnFloorData;
    private DamageData StabOnChestData;
    private DamageData JuheZhanData;
    private DamageData lastAttack;

    [Header("Settings")]
    public bool blockAttacksFromBehind = false;
    public bool resetComboChainOnChangeCombo;
    public bool invulnerableDuringJump = false;
    public bool invulnerableDuringRoll = false;

    public bool canTurnWhileDefending = false;
    public float hitRecoveryTime = .4f;
    public float hitThreshold = .2f;
    public float hitKnockBackForce = 1.5f;
    public float GroundAttackDistance = 1.5f;
    public int knockdownHitCount = 3;
    public float KnockdownTimeout = 0;
    public float KnockdownUpForce = 5;
    public float KnockbackForce = 4;
    public float KnockdownStandUpTime = .8f;

    [Header("Audio")]
    public string knockdownVoiceSFX = "";
    public string hitVoiceSFX = "";
    public string DeathVoiceSFX = "";
    public string defenceHitSFX = "";

    [Header("Stats")]
    public Vector2 currentDirection;
    public GameObject itemInRange;
    [HideInInspector]
    public bool isBossHeavyAttack = false;//是否是重击致命
    private Vector2 defendDirection;
    private bool continueCombo = false;
    private float lastAttackTime = 0;

    private int hitKnockDownCount = 0;
    private int hitKnockDownResetTime = 2;
    private float LastHitTime = 0;
    private bool isDead = false;
    private int EnemyLayer;
    private int DestroyableObjectLayer;
    private int EnvironmentLayer;
    private LayerMask HitLayerMask;
    private bool isGrounded = false;
    private Vector3 fixedVelocity;
    private bool updateVelocity = false;
    private InputManager inputManager;
    private InputAction lastAttackInput;
    private Vector3 lastAttackDirection;

    private Timer.Handle timer = null;
    private Animator animator;
    private PlayerController pController;
    [HideInInspector]
    public Vector3 rbVelocity = Vector3.zero;
    private DoAttack attackAction;
    private AttackType attackType = AttackType.None;
    //致命攻击
    private bool deadlyAttack = false;
    private bool stabAttack = false;
    [HideInInspector]
    public AnimatorStateInfo baseLayerStateInfo;

    public struct ComboStep
    {
        public AttackType attackType;
        public DamageData dData;
    }

    public struct Combo
    {
        public SwordLevel swordLevel;
        public ComboStep[] comboSteps;
    }

    //目前只设计两个连击，todo  
    public Combo[] comboAttacks = new Combo[1];
    private List<AttackType> comboProgress = new List<AttackType>(5);

    private Transform myTransform;
    private Rigidbody rb;
    private List<GameObject> hitTargets = new List<GameObject>();

    //以下是角色可以攻击的状态
    private List<UnitState> CanAttackStates = new List<UnitState> {
        UnitState.Idle,
        UnitState.Move,
        UnitState.Jumping,
        UnitState.Attack,
        UnitState.Defend,
    };

    //以下是角色可以被受击的状态
    private List<UnitState> HitableStates = new List<UnitState> {
        UnitState.Defend,
        UnitState.Hit,
        UnitState.Idle,
        UnitState.Move,
        UnitState.Attack,
        UnitState.Throw,
        UnitState.GroundAttack,
    };

    //以下是角色可以防御的状态 
    private List<UnitState> CanDefendStates = new List<UnitState> {
        UnitState.Idle,
        UnitState.Defend,
        UnitState.Move,
    };

    void OnEnable()
    {
        InputManager.onCombatInputEvent += CombatInputEvent;
        InputManager.onInputEvent += MovementInputEvent;
        //Messenger.AddListener<int>("death", Death);

    }

    void OnDisable()
    {
        InputManager.onCombatInputEvent -= CombatInputEvent;
        InputManager.onInputEvent -= MovementInputEvent;
        //Messenger.RemoveListener<int>("death", Death);

    }

    void Start()
    {
        myTransform = transform;
        animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerState>();
        pController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        EnemyLayer = LayerMask.NameToLayer("Enemy");
        DestroyableObjectLayer = LayerMask.NameToLayer("DestroyableObject");
        EnvironmentLayer = LayerMask.NameToLayer("Environment");
        HitLayerMask = (1 << EnemyLayer) | (1 << DestroyableObjectLayer);

        if (!invulnerableDuringJump)
        {
            HitableStates.Add(UnitState.Jumping);
        }
        InitComboList();
        Timer.Handle timer = new Timer.Handle();
        attackAction = new DoAttack(animator, rb, myTransform, this);
        isGrounded = pController.onGround;

        proTrailDistort.Init();
        proTrailShort.Init();
        proTraillong.Init();
    }

    private void Update()
    {
        isGrounded = pController.onGround;
        if (updateVelocity)
        {
            rb.velocity = rbVelocity;
            updateVelocity = false;
        }

        baseLayerStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string animString = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        //Debug.Log("animString:" + animString);
        if (attackAction != null && playerState.currentState == UnitState.Attack)
        {
            attackAction.OnUpdate();
        }
        if (attackAction.isFinish || playerState.currentState != UnitState.Attack)
        {
            proTrailDistort.Deactivate();
            proTrailShort.Deactivate();
            proTraillong.Deactivate();
        }
    }

    private void InitComboList()
    {
        if (comboDataList == null)
        {
            Debug.LogError("comboDataList不能为空");
            return;
        }

        comboAttacks[0] = new Combo() 
        {
            swordLevel = SwordLevel.One,

            comboSteps = new ComboStep[]{new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[0]},
                                            new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[1]},
                                            new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[2]},
                                            new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[3]},
                                            new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[4]},
            }
        };
        StabOnFloorData = comboDataList.attackData[5];
        JuheZhanData = comboDataList.attackData[6];
        StabOnChestData = comboDataList.attackData[7];
        List<AnimEventInfo> animEventInfoList = new List<AnimEventInfo>();
        animEventInfoList.Add(comboDataList.attackData[0].animEventInfo);
        GameUtils.AddAnimatorEvents(animator, animEventInfoList);
        //comboAttacks[1] = new Combo()
        //{
        //    swordLevel = SwordLevel.One,
        //    comboSteps = new ComboStep[]{new ComboStep(){attackType = AttackType.B, dData = comboDataList.attackData[5]},
        //                                    new ComboStep(){attackType = AttackType.B, dData = comboDataList.attackData[6]},
        //                                    new ComboStep(){attackType = AttackType.B, dData = comboDataList.attackData[7]},
        //                                    new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[8]},
        //                                    new ComboStep(){attackType = AttackType.A, dData = comboDataList.attackData[9]},
        //    }
        //};

    }

    private void MovementInputEvent(Vector2 inputVector)
    {
        Vector3 dir = new Vector3(inputVector.x, 0, inputVector.y);
        currentDirection = dir.normalized;
    }

    private void CombatInputEvent(InputAction action)
    {
        if (CanAttackStates.Contains(playerState.currentState) && !isDead)
        {
            //Debug.Log(" comboProgress.Count:" + comboProgress.Count);
            deadlyAttack = false;
            if ((action == InputAction.ATK_A || action == InputAction.ATK_B) && (playerState.currentState != UnitState.Attack && deadlyAttack && isGrounded))
            {
                AttackType atkType = AttackType.None;
                if (action == InputAction.ATK_A)
                {
                    atkType = AttackType.A;
                }
                else if (action == InputAction.ATK_B)
                {
                    atkType = AttackType.B;
                }

                //if (StabOnFloorData.animName.Length > 0)
                //{
                //    if (atkType == AttackType.A)
                //        AttackHandle(StabOnFloorData, UnitState.Attack, InputAction.ATK_A);
                //    else if (atkType == AttackType.A)
                //        AttackHandle(StabOnFloorData, UnitState.Attack, InputAction.ATK_B);
                //}

                if (JuheZhanData.animName.Length > 0)
                {
                    if (atkType == AttackType.A)
                        AttackHandle(JuheZhanData, UnitState.Attack, InputAction.ATK_A);
                    else if (atkType == AttackType.A)
                        AttackHandle(JuheZhanData, UnitState.Attack, InputAction.ATK_B);
                }
                return;
            }
            stabAttack = false;
            if ((action == InputAction.ATK_A || action == InputAction.ATK_B) && (playerState.currentState != UnitState.Attack && stabAttack && isGrounded))
            {
                AttackType atkType = AttackType.None;
                if (action == InputAction.ATK_A)
                {
                    atkType = AttackType.A;
                }
                else if (action == InputAction.ATK_B)
                {
                    atkType = AttackType.B;
                }

                if (StabOnChestData.animName.Length > 0)
                {
                    if (atkType == AttackType.A)
                        AttackHandle(StabOnChestData, UnitState.Attack, InputAction.ATK_A);
                    else if (atkType == AttackType.A)
                        AttackHandle(StabOnChestData, UnitState.Attack, InputAction.ATK_B);
                }
                return;
            }

            //首次攻击
            if ((action == InputAction.ATK_A || action == InputAction.ATK_B) && playerState.currentState != UnitState.Attack && isGrounded)
            {
                Debug.Log("01 --- attacking!!!!");
                AttackType atkType = AttackType.None;
                if (action == InputAction.ATK_A)
                {
                    atkType = AttackType.A;
                }else if(action == InputAction.ATK_B)
                {
                    atkType = AttackType.B;
                }

                bool insideComboWindow = (lastAttack != null && (Time.time < (lastAttackTime + lastAttack.duration + lastAttack.comboResetTime)));
                if (insideComboWindow && !continueCombo && (attackNum < COMBOTSTEP_LENGTH))
                {
                    attackNum += 1;
                }
                else
                {
                    attackNum = 0;
                    comboProgress.Clear();
                }

                DamageData data = ProcessCombo(atkType);

                if (data != null && data.animName.Length > 0)
                {
                    if(atkType == AttackType.A)
                        AttackHandle(data, UnitState.Attack, InputAction.ATK_A);
                    else if(atkType == AttackType.A)
                        AttackHandle(data, UnitState.Attack, InputAction.ATK_B);
                }
                return;
            }
                
            //处理连击
            if ((action == InputAction.ATK_A || action == InputAction.ATK_B) && (playerState.currentState == UnitState.Attack)
                && !continueCombo && isGrounded)
            {
                Debug.Log("02 --- continue attacking!!!!");

                if (attackNum < COMBOTSTEP_LENGTH - 1)
                {
                    if (action == InputAction.ATK_A)
                    {
                        attackType = AttackType.A;
                    }
                    else if (action == InputAction.ATK_B)
                    {
                        attackType = AttackType.B;
                    }
                    continueCombo = true;
                    return;
                }

            }
        }

    }

    public void Ready()
    {
        if (continueCombo)
        {
            continueCombo = false;

            if (attackNum < COMBOTSTEP_LENGTH - 1)
            {
                attackNum += 1;
            }
            else
            {
                attackNum = 0;
                comboProgress.Clear();
            }

            DamageData data = ProcessCombo(attackType);

            if (data != null && data.animName.Length > 0)
            {
                if (attackType == AttackType.A)
                    AttackHandle(data, UnitState.Attack, InputAction.ATK_A);
                else if (attackType == AttackType.A)
                    AttackHandle(data, UnitState.Attack, InputAction.ATK_B);
            }

            return;
        }

        playerState.SetState(UnitState.Idle);
    }

    private void AttackHandle(DamageData data, UnitState state, InputAction inputAction)
    {
        attackAction.attackTarget = attackTarget;
        // 播放攻击动作
        attackAction.OnInit(data);
        playerState.SetState(state);

        lastAttack = data;
        lastAttack.inflictor = gameObject;
        lastAttackTime = Time.time;
        lastAttackInput = inputAction;
        lastAttackDirection = attackAction.attackDir;

        SetVelocity(Vector3.zero);
        Invoke("Ready", data.duration);
        
    }

    public DamageData ProcessCombo(AttackType atkType)
    {
        if (atkType != AttackType.B && atkType != AttackType.A)
            return null;

        comboProgress.Add(atkType);
        if (comboAttacks.Length == 0)
        {
            Debug.LogError("comboAttacks.Length == 0");
            return null;
        }

        bool valid = false;
        for (int i = 0; i < comboAttacks.Length; i++)
        {
            Combo combo = comboAttacks[i];

            if (combo.swordLevel > swordLevel)
                continue;

            if (combo.comboSteps != null)
            {
                //设置默认值，一般为true
                valid = comboProgress.Count <= combo.comboSteps.Length;

                for (int j = 0; j < comboProgress.Count && j < combo.comboSteps.Length; j++)
                {
                    if (comboProgress[j] != combo.comboSteps[j].attackType)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            //尾部插入
            if (valid)
            {
                //最后一次连击
                combo.comboSteps[comboProgress.Count - 1].dData.lastAttackInCombo = !NextAttackIsAvailable(AttackType.A) && !NextAttackIsAvailable(AttackType.B);
                //是否首次攻击
                combo.comboSteps[comboProgress.Count - 1].dData.firstAttackInCombo = (1 == comboProgress.Count);
                combo.comboSteps[comboProgress.Count - 1].dData.comboIndex = i;
                combo.comboSteps[comboProgress.Count - 1].dData.fullCombo = (comboProgress.Count == combo.comboSteps.Length);
                combo.comboSteps[comboProgress.Count - 1].dData.comboStep = comboProgress.Count;
                //返回最新的连击动作数据
                return combo.comboSteps[comboProgress.Count - 1].dData;
            }
        }

        //输入并非连击的顺序
        comboProgress.Clear();
        comboProgress.Add(atkType);
        for (int i = 0; i < comboAttacks.Length; i++)
        {
            if (comboAttacks[i].comboSteps[0].attackType == atkType)
            {
                Debug.Log(Time.timeSinceLevelLoad + " New combo " + i + " step " + comboProgress.Count);
                comboAttacks[i].comboSteps[0].dData.firstAttackInCombo = true;
                comboAttacks[i].comboSteps[0].dData.lastAttackInCombo = false;
                comboAttacks[i].comboSteps[0].dData.comboIndex = i;
                comboAttacks[i].comboSteps[0].dData.fullCombo = false;
                comboAttacks[i].comboSteps[0].dData.comboStep = 0;
                return comboAttacks[i].comboSteps[0].dData;

            }
        }
        

        Debug.LogError("没找到任何连击 !!!");
        return null;
    }
    //是否可以进行下一次攻击
    private bool NextAttackIsAvailable(AttackType atkType)
    {
        if (atkType != AttackType.B && atkType != AttackType.A)
            return false;

        // COMBOTSTEP_LENGTH 默认为 5
        if (comboProgress.Count == COMBOTSTEP_LENGTH)
            return false;

        List<AttackType> progress = new List<AttackType>(comboProgress);
        progress.Add(atkType);

        if (comboAttacks.Length == 0 || progress.Count == 0)
        {
            Debug.LogError("comboAttacks.Length == 0 || progress.Count == 0");
            return false;
        }

        Combo combo;
        for (int i = 0; i < comboAttacks.Length; i++)
        {
            combo = comboAttacks[i];

            if (combo.swordLevel > swordLevel)
                continue;

            bool valid = true;
            for (int j = 0; j < progress.Count; j++)
            {
                //Debug.Log("combo.comboSteps[j].attackType:" + combo.comboSteps[j].attackType);
                //检查最后一个
                if (progress[j] != combo.comboSteps[j].attackType)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return true;
        }
        return false;
    }

    private void Defend(bool defend)
    {
        animator.SetBool("Defend", defend);
        if (defend)
        {
            TurnToTargetDir(currentDirection);
            SetVelocity(Vector3.zero);
            playerState.SetState(UnitState.Defend);
        }
        else
        {
            playerState.SetState(UnitState.Idle);
        }
    }

    public void TurnToTargetDir(Vector2 dir)
    {
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetVelocity(Vector3 velocity)
    {
        rbVelocity = velocity;
        updateVelocity = true;
    }

    public void CheckForHit()
    {
        Debug.Log("CheckForHit");
        EnemyManager.getActiveEnemies();
        if (EnemyManager.activeEnemies.Count > 0)
        {
            for (int i = 0; i < EnemyManager.activeEnemies.Count; i++)
            {
                if (Vector3.Distance(EnemyManager.activeEnemies[i].transform.position, transform.position) <= lastAttack.attackRange)
                {
                    if(!hitTargets.Contains(EnemyManager.activeEnemies[i]))
                        hitTargets.Add(EnemyManager.activeEnemies[i]);
                }
            }

            if (hitTargets.Count > 0)
            {
                for (int i = 0; i < hitTargets.Count; i++)
                {
                    var angle = Vector3.Angle(lastAttackDirection, hitTargets[i].transform.forward);
                    Debug.Log("angle:" + angle);
                    lastAttack.hitAngle = 365f;
                    if (Vector3.Angle(lastAttackDirection, hitTargets[i].transform.forward) <= lastAttack.hitAngle)
                    {
                        IDamagable damagableObject = hitTargets[i].GetComponent<IDamagable>();
                        if (damagableObject != null)
                        {
                            damagableObject.Hit(lastAttack);
                        }
                    }
                }
            }
        }

    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration)
        {
            Gizmos.color = Color.red;
            //Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + lastAttackDirection * lastAttack.collDistance;
            //Vector3 boxSize = new Vector3(lastAttack.CollSize, lastAttack.CollSize, hitZRange);
            //Gizmos.DrawWireCube(boxPosition, boxSize);
            //Gizmos.DrawWireMesh()
        }
    }

#endif

    public void Hit(DamageData data)
    {

        if (Time.time < LastHitTime + hitThreshold) return;

        if (HitableStates.Contains(playerState.currentState))
        {
            CancelInvoke();

            //CamShake camShake = Camera.main.GetComponent<CamShake>();
            //if (camShake != null) camShake.Shake(.1f);

            if (playerState.currentState == UnitState.Defend && !data.DefenceOverride && (IsFacingTarget(data.inflictor) || blockAttacksFromBehind))
            {
                Defend(data);
                return;
            }
            else
            {
                animator.SetBool("Defend", false);
            }

            UpdateHitCounter();
            LastHitTime = Time.time;
            //animator.ShowHitEffect();

            //HealthSystem hs = GetComponent<HealthSystem>();
            //if (hs != null)
            //{
            //    hs.SubstractHealth(d.damage);
            //    if (hs.CurrentHp == 0)
            //        return;
            //}
            //blackboard.SubstractHealth(d.damage);
            //if (blackboard.CurrentHp <= 0) return;

            //!IsGrounded()
            if ((hitKnockDownCount >= knockdownHitCount || data.knockDown) && playerState.currentState != UnitState.Knockdown)
            {
                hitKnockDownCount = 0;
                StopCoroutine("KnockDownSequence");
                StartCoroutine("KnockDownSequence", data.inflictor);
                //GlobalAudioPlayer.PlaySFXAtPosition(data.hitSFX, transform.position + Vector3.up);
                //GlobalAudioPlayer.PlaySFXAtPosition(knockdownVoiceSFX, transform.position + Vector3.up);
                return;
            }

            int i = Random.Range(1, 3);
            animator.SetTrigger("Hit" + i);
            //SetVelocity(Vector3.zero);
            playerState.SetState(UnitState.Hit);

            if (IsFacingTarget(data.inflictor))
            {
                AddForce(-1.5f);
            }
            else
            {
                AddForce(1.5f);
            }

            //GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position + Vector3.up);
            //GlobalAudioPlayer.PlaySFXAtPosition(hitVoiceSFX, transform.position + Vector3.up);

            Invoke("Ready", hitRecoveryTime);
        }
    }

    public void AddForce(float force)
    {
        StartCoroutine(AddForceCoroutine(force));
    }

    private IEnumerator AddForceCoroutine(float force)
    {
        Vector3 startDir = currentDirection;
        float speed = 8f;
        float t = 0;

        while (t < 1)
        {
            yield return Yielders.FixedUpdate;
            rb.velocity = startDir * Mathf.Lerp(force, rb.velocity.y, Mathfs.Sinerp(0, 1, t));
            t += Time.fixedDeltaTime * speed;
            yield return null;
        }

    }

    //todo
    public bool IsFacingTarget(GameObject go)
    {
        return true;
    }

    private void UpdateHitCounter()
    {
        if (Time.time - LastHitTime < hitKnockDownResetTime)
        {
            hitKnockDownCount += 1;
        }
        else
        {
            hitKnockDownCount = 1;
        }
        LastHitTime = Time.time;
    }

    private void Defend(DamageData d)
    {
        //animator.ShowDefendEffect();
        //GlobalAudioPlayer.PlaySFXAtPosition(defenceHitSFX, transform.position + Vector3.up);

        //if (isFacingTarget(d.inflictor))
        //{
        //    animator.AddForce(-hitKnockBackForce);
        //}
        //else
        //{
        //    animator.AddForce(hitKnockBackForce);
        //}
    }

    //public GameObject GetBestTarget(bool hasToBeKnockdown)
    //{

    //    float[] EnemyCoeficient = new float[enemies.Count];
    //    DamageData enemy;
    //    Vector3 dirToEnemy;

    //    for (int i = 0; i < enemies.Count; i++)
    //    {
    //        EnemyCoeficient[i] = 0;
    //        enemy = enemies[i];

    //        if (hasToBeKnockdown)
    //            continue;

    //        if (enemy.BlackBoard.Invulnerable)
    //            continue;

    //        dirToEnemy = (enemy.Position - Owner.Position);

    //        float distance = dirToEnemy.magnitude;

    //        if (distance > 5.0f)
    //            continue;

    //        dirToEnemy.Normalize();

    //        float angle = Vector3.Angle(dirToEnemy, Owner.Forward);

    //        if (enemy == LastAttacketTarget)
    //            EnemyCoeficient[i] += 0.1f;

    //        //Debug.Log("LastTarget " + Mission.Instance.CurrentGameZone.GetEnemy(i).name + " : " + EnemyCoeficient[i]); 

    //        EnemyCoeficient[i] += 0.2f - ((angle / 180.0f) * 0.2f);

    //        //  Debug.Log("angle " + Mission.Instance.CurrentGameZone.GetEnemy(i).name + " : " + EnemyCoeficient[i]);

    //        if (Controls.Joystick.Direction != Vector3.zero || dir != Vector3.zero)
    //        {
    //            angle = Vector3.Angle(dirToEnemy, dir);
    //            //angle = Vector3.Angle(dirToEnemy, Controls.Joystick.Direction);
    //            EnemyCoeficient[i] += 0.5f - ((angle / 180.0f) * 0.5f);
    //        }
    //        //    Debug.Log(" joy " + Mission.Instance.CurrentGameZone.GetEnemy(i).name + " : " + EnemyCoeficient[i]); 

    //        EnemyCoeficient[i] += 0.2f - ((distance / 5) * 0.2f);

    //        //      Debug.Log(" dist " + Mission.Instance.CurrentGameZone.GetEnemy(i).name + " : " + EnemyCoeficient[i]); 
    //    }

    //    float bestValue = 0;
    //    int best = -1;
    //    for (int i = 0; i < enemies.Count; i++)
    //    {
    //        //     Debug.Log(Mission.Instance.CurrentGameZone.GetEnemy(i).name + " : " + EnemyCoeficient[i]); 
    //        if (EnemyCoeficient[i] <= bestValue)
    //            continue;

    //        best = i;
    //        bestValue = EnemyCoeficient[i];
    //    }

    //    if (best >= 0)
    //        return enemies[best];

    //    return null;
    //}

    private void Death(int id)
    {
        if (!id.Equals(gameObject.GetInstanceID())) return;
        if (!isDead)
        {
            isDead = true;
            StopAllCoroutines();
            CancelInvoke();
            //SetVelocity(Vector3.zero);
            //GlobalAudioPlayer.PlaySFXAtPosition(DeathVoiceSFX, transform.position + Vector3.up);
            isBossHeavyAttack = true;
            animator.SetBool("Death", true);
            //if (isBossHeavyAttack)
            //{
            //    anim.SetBool("Death3", true);
            //}
            //else
            //{
            //    int rdx = Random.Range(0, 100);
            //    Debug.Log("rdx:" + rdx);
            //    if (rdx <= 60)
            //        anim.SetBool("Death", true);
            //    else
            //        anim.SetBool("Death2", true);
            //}
            //EnemyManager.PlayerHasDied();

            //ReStartLevel();
            Timer.InvokeMe(1, () =>
            {
                if (timer != null)
                    timer.Cancel();
            });

        }


    }

    void OnAnimatorIK(int layerIndex)
    {
        if (attackPoint != null && playerState.currentState == UnitState.Attack)
        {
            //animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weaponBonetPositionWeight);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weaponBonetRotationWeight);
            //animator.SetIKPosition(AvatarIKGoal.RightHand, attackPoint.position);
            //animator.SetIKRotation(AvatarIKGoal.RightHand, attackPoint.rotation);
        }
    }

}