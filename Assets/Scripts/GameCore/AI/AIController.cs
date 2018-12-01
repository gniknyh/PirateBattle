#define DEBUG
#define ANNOTATE_NAVMESH
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using NPBehave;
using Random = UnityEngine.Random;

public partial class AIController : MonoBehaviour, IDamagable
{
    public bool isInvulnerable = false;
    public bool isAlive = true;
    public float fov = 60f;

    public GameObject attackTarget;
    protected Animator animator;
    public GameObject GFX;
    protected Rigidbody rb;
    protected CapsuleCollider capsule;
    protected CamShake camShake;
    protected CamSlowMotionDelay camSlowMotionDelay = null;

    [Space(10)]
    [Header("Attack Data")]
    public DamageData[] AttackList;
    public bool PickRandomAttack;
    public float hitZRange = 2;
    public float defendChance = 0;
    public float hitRecoveryTime = .4f;
    public float standUpTime = 1.1f;
    public bool canDefendDuringAttack;
    public bool AttackPlayerAirborne;
    private DamageData lastAttack;
    private int AttackCounter = 0;
    private Vector3 lastAttackDirection;
    public bool canHitEnemies;
    public bool canHitDestroyableObjects;
    [HideInInspector]
    public float lastAttackTime;

    [Header("Settings")]
    public float weaponRange = 1.4f;
    public float closeRangeDistance = 2f;
    public float midRangeDistance = 3f;
    public float farRangeDistance = 4.5f;
    public float patrolArrivalDistance = 0.5f;

    public float walkSpeed = 1.95f;
    public float runSpeed = 5f;
    public float sprintSpeed = 6f;

    public float sightDistance = 7f;

    public float attackInterval = 1.2f;
    public float rotationSpeed = 15f;
    public float lookaheadDistance;
    public bool ignoreCliffs = false;

    public float KnockdownTimeout = 0f;
    public float KnockdownUpForce = 5f;
    public float KnockbackForce = 4;

    private LayerMask HitLayerMask;
    public LayerMask CollisionLayer;
    public bool randomizeValues = true;

    [Space(10)]
    [Header("Combat Setting")]
  
    public float RageMin = 0; //0 = no attack
    public float RageMax = 0;// 100 % chance is do do attack
    public float RageModificator = 0;//每秒自动增加系数
   
    public float DodgeMin = 0;
    public float DodgeMax = 0;
    public float DodgeModificator = 0; //每秒自动增加系数

    public float FearMin = 0;
    public float FearMax = 0;
    public float FearModificator = 0; //每秒自动增加系数
    public float BerserkMin = 0; //0 = no attack
    public float BerserkMax = 0;// 100 % chance is do do attack
    public float BerserkModificator = 0;//每秒自动增加系数

    public float RageInjuryModificator = 0; // each injury increase rage
    public float DodgeInjuryModificator = 0;  // each injury increase dodge
    public float FearInjuryModificator = 0;
    public float BerserkInjuryModificator = 0;

    public float RageBlockModificator = 0; // each block increase rage
    public float FearBlockModificator = 0; // each block increase rage
    public float BerserkBlockModificator = 0; // each block increase rage

    public float DodgeAttackModificator = 0; // each attack increase rage
    public float FearAttackModificator = 0; // each attack increase rage
    public float BerserkAttackModificator = 0; // each attack increase rage

    //main AI parameters
    [System.NonSerialized]
    public float Rage = 0;//愤怒值，决定是否进行攻击
    [System.NonSerialized]
    public float Fear = 0;  //恐惧值
    [System.NonSerialized]
    public float Dodge = 0; //格挡值，决定是否格挡
    [System.NonSerialized]
    public float Berserk = 0;//狂暴值

    [Space(10)]
    [Header("Status")]
    public Vector3 currentDirection;
    public bool targetSpotted;
    public bool cliffSpotted;
    public bool wallspotted;
    public bool isGrounded;
    private Vector3 moveDirection;
    public float distance;
    private Vector3 fixedVelocity;
    private bool updateVelocity;
    public Vector3 distanceToTarget;

    [SerializeField] private float _avoidanceForceFactor = 0.75f;

    [SerializeField] private float _minTimeToCollision = 2;

    [SerializeField] private bool _offMeshCheckingEnabled = true;

    [SerializeField] private Vector3 _probePositionOffset = new Vector3(0, 0.2f, 0);

    [SerializeField] private float _probeRadius = 0.1f;
    public float maxForce = 10;
    private int _navMeshLayerMask;

    private PlayerState AIState;
    private DoAttack attackAction;
    private Transform myTransform;
 
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    [HideInInspector]
    public AnimatorStateInfo baseLayerStateInfo;

    private RangeSensor sensor;

    private Vector3 targetPos;

    public int m_PlayerNumber = 1;     
    public int m_Behaviour = 0;       

    private List<GameObject> m_Targets; 
    private Root tree;                 
    private Blackboard blackboard;
    private WaypointPath wayPoints;
   
    //以下状态Agent不能移动
    private List<UnitState> NoMovementStates = new List<UnitState> {
        UnitState.Death,
        UnitState.Attack,
        UnitState.Defend,
        UnitState.GroundAttack,
        UnitState.Hit,
        UnitState.Idle,
        UnitState.Knockdown,
        UnitState.StandUp,
    };

    private List<UnitState> HitableStates = new List<UnitState> {
        UnitState.Attack,
        UnitState.Defend,
        UnitState.Hit,
        UnitState.Idle,
        UnitState.StandUp,
        UnitState.Move,
        UnitState.KnockDownGrounded,
    };

    private List<UnitState> defendableStates = new List<UnitState>
    {
        UnitState.Idle,
        UnitState.Move,
        UnitState.Defend,
    };

    void OnEnable()
    {
        Messenger.AddListener("NotifyToNeighbors", NotifyToNeighbors);
    }

    void OnDisable()
    {
        Messenger.RemoveListener("NotifyToNeighbors", NotifyToNeighbors);

    }

    public void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        EnemyManager.enemyList.Add(gameObject);
    }

    public void Start()
    {
        myTransform = transform;
        //camShake = Camera.main.GetComponent<CamShake>();
        //camSlowMotionDelay = Camera.main.GetComponent<CamSlowMotionDelay>();

        EnemyManager.getActiveEnemies();
        if (GameObject.Find("WayPoints"))
        {
            wayPoints = GameObject.Find("WayPoints").GetComponent<WaypointPath>();
        }

        HitLayerMask = 1 << LayerMask.NameToLayer("Player");
        if (canHitEnemies)
            HitLayerMask |= (1 << LayerMask.NameToLayer("Enemy"));
        if (canHitDestroyableObjects)
            HitLayerMask |= (1 << LayerMask.NameToLayer("DestroyableObject"));

        AIState = GetComponent<PlayerState>();
        attackAction = new DoAttack(animator, rb, myTransform, this);
        navMeshAgent = GetComponent<NavMeshAgent>();
        baseLayerStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        sensor = GetComponent<RangeSensor>();
        _navMeshLayerMask = 1 << NavMesh.GetAreaFromName("Default");

        //InitTree();
    }

    void InitTree()
    {
        tree = CreateBehaviourTree();
        blackboard = tree.Blackboard;
#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = tree;
#endif

        tree.Start();
    }

    //怪物被杀死或你只是摧毁你的GameObject，应该停止树
    public void StopBehaviorTree()
    {
        if (tree != null && tree.CurrentState == Node.State.ACTIVE)
        {
            tree.Stop();
        }
    }

    #region Update

    public void Update()
    {
        IsGrounded();
        baseLayerStateInfo = animator.GetCurrentAnimatorStateInfo(0);
    }

    public void FixedUpdate()
    {
        if (updateVelocity)
        {
            rb.velocity = fixedVelocity;
            updateVelocity = false;
        }
    }

    void SetVelocity(Vector3 velocity)
    {
        fixedVelocity = velocity;
        updateVelocity = true;
    }

    #endregion

    #region Attack
        
    public bool HasEnemy()
    {
        return attackTarget != null;
    }

    public bool IsVisible_Enemy()
    {
        return false;
    }

    public void SetDestination()
    {
        targetPos = attackTarget.transform.position + UnityEngine.Random.insideUnitSphere;
    }

    public void Attack()
    {
        //if(!isGrounded && )
        if (PickRandomAttack)
            AttackCounter = UnityEngine.Random.Range(0, AttackList.Length);

        if (!PickRandomAttack)
        {
            AttackCounter += 1;
            if (AttackCounter >= AttackList.Length) AttackCounter = 0;
        }

        AttackHandle(AttackList[AttackCounter], UnitState.Attack);
        
    }

    private void AttackHandle(DamageData data, UnitState state)
    {
        attackAction.attackTarget = attackTarget;
        // 播放攻击动作
        attackAction.OnInit(data);
        AIState.SetState(state);

        lastAttack = data;
        lastAttack.inflictor = gameObject;
        lastAttackTime = Time.time;
        lastAttackDirection = currentDirection;

        SetVelocity(Vector3.zero);
        Invoke("Ready", data.duration);

    }

    #endregion

    #region We are Hit

    public void Hit(DamageData damageObject)
    {
        Debug.Log("I am hitting!!!!");
        if (HitableStates.Contains(AIState.currentState))
        {
            if (AIState.currentState == UnitState.KnockDownGrounded && !damageObject.isGroundAttack)
                return;

            CancelInvoke(); //取消该脚本上的所有延时方法
            StopAllCoroutines();//停止所有在该脚本上的协程
            //Move(Vector3.zero);

            //设置攻击时间，这样敌人就不会在受到攻击后立即攻击
            lastAttackTime = Time.time;

            if ((AIState.currentState == UnitState.KnockDownGrounded || AIState.currentState == UnitState.GroundHit) && !damageObject.isGroundAttack)
                return;

            if (!damageObject.DefenceOverride && defendableStates.Contains(AIState.currentState))
            {
                int rand = Random.Range(0, 100);
                if (rand < defendChance)
                {
                    Defend();
                    return;
                }
            }

            //GlobalAudioPlayer.PlaySFXAtPosition(damageObject.hitSFX, transform.position);
            //ShowHitEffectAtPosition(new Vector3(transform.position.x, damageObject.inflictor.transform.position.y + damageObject.collHeight, transform.position.z));

            //if (camShake)
            //    camShake.Shake(.1f);
            //else
            //{
            //    camShake = Camera.main.GetComponent<CamShake>();
            //    camShake.Shake(0.1f);
            //}

            if (damageObject.slowMotionEffect)
            {
                if (camSlowMotionDelay)
                    camSlowMotionDelay.StartSlowMotionDelay(.2f);
                else
                {
                    camSlowMotionDelay = Camera.main.GetComponent<CamSlowMotionDelay>();
                    camSlowMotionDelay.StartSlowMotionDelay(.2f);
                }

            }

            //HealthSystem hs = GetComponent<HealthSystem>();
            //if (hs)
            //{
            //    hs.SubstractHealth(damageObject.damage);
            //    if (hs.CurrentHp == 0)
            //        return;
            //}

            if (AIState.currentState == UnitState.KnockDownGrounded)
            {
                StopAllCoroutines();
                AIState.SetState(UnitState.GroundHit);
                //StartCoroutine(GroundHit());
                return;
            }

            //int dir = damageObject.inflictor.transform.position.x > transform.position.x ? 1 : -1;
            //TurnToDir((DIRECTION)dir);

            if (damageObject.knockDown)
            {
                StartCoroutine(KnockDownSequence(damageObject.inflictor));
                return;

            }
            else
            {
                //int rand = Random.Range(1, 3);
                //animator.SetTrigger("Hit" + rand) ;
                animator.SetTrigger("Hit");
                //int hitID = Random.Range(1, 3);
                //animator.SetAnimatorTrigger("Hit");
                //animator.animator.SetInteger(HashIDsAnimator.HitIDID,hitID);

                AIState.SetState(UnitState.Hit);
                LookAtTarget(damageObject.inflictor.transform);
                AddForce(-KnockbackForce);

                //当攻击时，将敌人状态从被动状态切换到攻击状态
                //if (enemyTactic != ENEMYTACTIC.ENGAGE)
                //{
                //    EnemyManager.setAgressive(gameObject);
                //}

                Invoke("Ready", hitRecoveryTime);
                return;
            }
        }
    }

    //Defend
    void Defend()
    {
        AIState.SetState(UnitState.Defend);
        //animator.ShowDefendEffect();
        animator.SetTrigger("Defend");
        //GlobalAudioPlayer.PlaySFX("DefendHit");
        //animator.SetDirection(currentDirection);
    }

    public void AddForce(float force)
    {
        StartCoroutine(AddForceCoroutine(force));
    }

    IEnumerator AddForceCoroutine(float force)
    {
        Vector3 startDir = currentDirection;
        float speed = 8f;
        float t = 0;

        while (t < 1)
        {
            yield return new WaitForFixedUpdate();
            rb.velocity = startDir * Mathf.Lerp(force, rb.velocity.y, Mathfs.Sinerp(0, 1, t));
            t += Time.fixedDeltaTime * speed;
            yield return null;
        }

    }

    #endregion

    #region Check for hit

    //伤害判定
    //public void CheckForHit()
    //{
    //    //通过相交盒，Physics.OverlapBox是通过射线检测实现的
    //    Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight)
    //                        + currentDirection * lastAttack.collDistance;
    //    Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
    //    Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

    //    int i = 0;
    //    if (hitColliders.Length > 0)
    //    {
    //        while (i < hitColliders.Length)
    //        {
    //            IDamagable<DamageData> damagableObject =
    //                hitColliders[i].GetComponent(typeof(IDamagable<DamageData>)) as IDamagable<DamageData>;
    //            if (damagableObject != null && damagableObject != (IDamagable<DamageData>)this)  //排除自身
    //            {
    //                damagableObject.Hit(lastAttack);
    //            }
    //            i++;
    //        }
    //    }

        
    //}
#if DEBUG

    void OnDrawGizmos(){

		//可视化伤害判定
		if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration) {
			Gizmos.color = Color.red;
			Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + currentDirection * lastAttack.collDistance;
			Vector3 boxSize = new Vector3 (lastAttack.CollSize, lastAttack.CollSize, hitZRange);
			Gizmos.DrawWireCube (boxPosition, boxSize);
		}

		//visualize lookahead sphere
		Gizmos.color = Color.yellow;
		Vector3 offset = -moveDirection.normalized * lookaheadDistance;
		//Gizmos.DrawWireSphere (transform.position + capsule.center - offset, capsule.radius); 
	}

#endif

    #endregion

    #region KnockDown Sequence

    IEnumerator KnockDownSequence(GameObject inflictor)
    {
        yield return Yielders.FixedUpdate;//等待一次物理引擎刷新后再执行后面的
        //TurnToDir(currentDirection);

        //add knockback force
        animator.SetTrigger("KnockDown_Up");
        while (IsGrounded())
        {
            SetVelocity(new Vector3(KnockbackForce * -currentDirection.x, KnockdownUpForce, 0));
            yield return Yielders.FixedUpdate;
        }

        //going up...
        while (rb.velocity.y >= 0)
            yield return Yielders.FixedUpdate;

        //going down
        animator.SetTrigger("KnockDown_Down");
        while (!IsGrounded())
            yield return Yielders.FixedUpdate;

        //hit ground
        animator.SetTrigger("KnockDown_End");
        //GlobalAudioPlayer.PlaySFXAtPosition("Drop", transform.position);
        animator.SetFloat("MovementSpeed", 0f);
        //animator.ShowDustEffectLand();
        //Move(Vector3.zero);

        CamShake camShake = Camera.main.GetComponent<CamShake>();
        if (camShake)
            camShake.Shake(.3f);

        //倒地时的特效
        //animator.ShowDustEffectLand();

        //防止滑步
        float t = 0;
        float speed = 2;
        Vector3 fromVelocity = rb.velocity;
        while (t < 1)
        {
            SetVelocity(Vector3.Lerp(new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z),
                new Vector3(0, rb.velocity.y, 0), t));
            t += Time.deltaTime * speed;
            yield return Yielders.FixedUpdate;
        }

        //Move(Vector3.zero);
        yield return Yielders.GetWaitForSeconds(KnockdownTimeout);

        //倒地后站立
        //enemyState = UnitState.STANDUP;
        animator.SetTrigger("StandUp");
        Invoke("Ready", standUpTime);

    }

    //ground hit
    //public IEnumerator GroundHit()
    //{
    //    CancelInvoke();
    //    GlobalAudioPlayer.PlaySFXAtPosition("EnemyGroundPunchHit", transform.position);
    //    animator.SetAnimatorTrigger("GroundHit");
    //    yield return Yielders.GetWaitForSeconds(KnockdownTimeout);

    //    if (!isDead)
    //        animator.SetAnimatorTrigger("StandUp");
    //    Invoke("Ready", standUpTime);
    //}

    #endregion

    public void MoveTo()
    {
        Vector3 dir = attackTarget.transform.position - transform.position;
        float speed = 0f;
        //自动根据目标进行调整移动速度
        if(dir.magnitude >= 10f)
        {
            speed = 1.5f;
        }else if(dir.magnitude < 10f && dir.magnitude >= 4f)
        {
            speed = 1.0f;
        }
        else
        {
            speed = 0.5f;
        }

        dir.Normalize();
        TurnToDir(dir);
        isGrounded = true;
        if (isGrounded)
        {
            AIState.SetState(UnitState.Move);
            dir.Normalize();
            //var speed = Mathf.Abs(dir.x) + Mathf.Abs(dir.z);
            //speed = Mathf.Clamp(speed, 0, 1);
            if (dir.magnitude > 0)
            {
                animator.SetFloat("Speed", speed, 0.2f, Time.fixedDeltaTime);
                if (baseLayerStateInfo.IsName("Grounded.FreeMovement"))
                {
                    if (speed <= 0.5f)
                    {
                        SetVelocity(transform.forward * (walkSpeed * speed));
                    }
                    else if (speed > 0.5f && speed <= 1.0f)
                    {
                        SetVelocity(transform.forward * (runSpeed * speed));
                    }
                    else
                    {
                        SetVelocity(transform.forward * (sprintSpeed * speed));
                    }
                }

            }

        }
    }

    public bool ReachToPos()
    {
        float distance = Vector3.Distance(transform.position, attackTarget.transform.position);
        return distance <= weaponRange;

    }
    
    public void Idle()
    {
        SetVelocity(Vector3.zero);
        animator.SetFloat("RandomIdle", Random.Range(0, 2), 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("Speed", 0, 0.2f, Time.fixedDeltaTime);
    }

    //是否在地面上
    public bool IsGrounded()
    {
        float colliderSize = capsule.bounds.extents.y - .1f;
        if (Physics.CheckCapsule(capsule.bounds.center, capsule.bounds.center + Vector3.down * colliderSize, capsule.radius, CollisionLayer))
        {
            isGrounded = true;
            return true;
        }
        else
        {
            isGrounded = false;
            return false;
        }
    }

    public void Ready()
    {      
        StopAllCoroutines();
        CancelInvoke();
        AIState.SetState(UnitState.Idle);
        SetVelocity(Vector3.zero);
        animator.SetFloat("Speed", 0, 0.2f, Time.fixedDeltaTime);
    }

    public void ShowHitEffectAtPosition(Vector3 pos)
    {
        GameObject.Instantiate(Resources.Load("HitEffect"), pos, Quaternion.identity);
    }

    public void LookAtTarget(Transform _target)
    {
        if (_target != null)
        {
            Vector3 newDir = Vector3.zero;
            newDir = _target.position - transform.position;
            newDir.Normalize();

            newDir = Vector3.RotateTowards(transform.forward, newDir, rotationSpeed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    public void TurnToDir(Vector3 dir)
    {
        Vector3 newDir = dir;
        newDir.Normalize();
        newDir = Vector3.RotateTowards(transform.forward, newDir, rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);     
    }

    private void Turn(float turn)
    {
        float t = Mathf.Clamp(turn, -1, 1);
        Quaternion turnRotation = Quaternion.Euler(0f, t * rotationSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    public void SetRandomValues()
    {
        walkSpeed *= Random.Range(.8f, 1.2f);
        attackInterval *= Random.Range(.7f, 1.5f);
        KnockdownTimeout *= Random.Range(.7f, 1.5f);
        KnockdownUpForce *= Random.Range(.8f, 1.2f);
        KnockbackForce *= Random.Range(.7f, 1.5f);
    }

    bool WallSpotted()
    {
        Vector3 Offset = moveDirection.normalized * lookaheadDistance;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + capsule.center + Offset, capsule.radius, CollisionLayer);

        int i = 0;
        bool hasHitwall = false;
        while (i < hitColliders.Length)
        {
            if (CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer))
            {
                hasHitwall = true;
            }
            i++;
        }
        wallspotted = hasHitwall;
        return hasHitwall;
    }

    bool PitfallSpotted()
    {
        if (!ignoreCliffs)
        {
            float lookDownDistance = 1f;
            Vector3 StartPoint = transform.position + (Vector3.up * .3f) + (Vector3.right * (capsule.radius + lookaheadDistance) * moveDirection.normalized.x);
            RaycastHit hit;

#if UNITY_EDITOR
            Debug.DrawRay(StartPoint, Vector3.down * lookDownDistance, Color.red);
#endif

            if (!Physics.Raycast(StartPoint, Vector3.down, out hit, lookDownDistance, CollisionLayer))
            {
                cliffSpotted = true;
                return true;
            }
        }
        cliffSpotted = false;
        return false;
    }

    private Vector3 CalculateForce()
    {
        NavMeshHit hit;

        var futurePosition = myTransform.position + (rb.velocity * _minTimeToCollision);
        var movement = futurePosition - myTransform.position;

#if ANNOTATE_NAVMESH
            Debug.DrawRay(myTransform.position, movement, Color.cyan);
#endif

        if (_offMeshCheckingEnabled)
        {
            var probePosition = myTransform.position + _probePositionOffset;

            Profiler.BeginSample("Off-mesh checking");
            NavMesh.SamplePosition(probePosition, out hit, _probeRadius, _navMeshLayerMask);
            Profiler.EndSample();

            if (!hit.hit)
            {
                // we're not on the navmesh
                Profiler.BeginSample("Find closest edge");
                NavMesh.FindClosestEdge(probePosition, out hit, _navMeshLayerMask);
                Profiler.EndSample();

                if (hit.hit)
                {
                    // closest edge found
#if ANNOTATE_NAVMESH
                    Debug.DrawLine(probePosition, hit.position, Color.red);
#endif
                    return (hit.position - probePosition).normalized * maxForce;
                } // no closest edge - too far off the mesh
#if ANNOTATE_NAVMESH
                Debug.DrawLine(probePosition, probePosition + Vector3.up * 3, Color.red);
#endif
                return Vector3.zero;
            }
        }

        Profiler.BeginSample("NavMesh raycast");
        NavMesh.Raycast(myTransform.position, futurePosition, out hit, _navMeshLayerMask);
        Profiler.EndSample();

        if (!hit.hit)
            return Vector3.zero;

        Profiler.BeginSample("Calculate NavMesh avoidance");
        var moveDirection = rb.velocity.normalized;
        var avoidance = PerpendicularComponent(hit.normal, moveDirection);

        avoidance.Normalize();

#if ANNOTATE_NAVMESH
        Debug.DrawLine(myTransform.position, myTransform.position + avoidance, Color.white);
#endif

        avoidance += moveDirection * maxForce * _avoidanceForceFactor;

#if ANNOTATE_NAVMESH
        Debug.DrawLine(myTransform.position, myTransform.position + avoidance, Color.yellow);
#endif

        Profiler.EndSample();

        return avoidance;
    }

    //Perpendicular:垂直
    public static Vector3 PerpendicularComponent(Vector3 source, Vector3 unitBasis)
    {
        var projection = Vector3.Dot(source, unitBasis);
        var tmp = unitBasis * projection;
        return source - tmp;
    }

    public void NotifyToNeighbors()
    {
        //通知其他队友
    }

    public Vector3 GetNextPos()
    {
        Vector3 tPos = Vector3.zero;
        if (wayPoints == null)
        {
            Debug.Log("not WayPointPath component!!!! ");
            return tPos;
        }
        if(wayPoints.waypoints.Length > 0)
        {
            //这个变量可以用于在巡逻点加些扰动，使巡逻路线看上去更随机
            Vector3 rndPos = Vector3.zero;
            tPos = wayPoints.waypoints[Random.Range(0, wayPoints.waypoints.Length + 1)].position + rndPos;
            targetPos = tPos;
        }
        return tPos;
    }
     
    //巡逻状态
    public void Patrol()
    {
        var pos = GetNextPos();
        if (Vector3.Distance(transform.position, pos) <= patrolArrivalDistance)
        {
            pos = GetNextPos();
        }
        MoveTo();
    }

    public void CalculateTurnDir()
    {
        var futurePosition = myTransform.position + (rb.velocity * _minTimeToCollision);
        var moveDirection = rb.velocity.normalized;
        RaycastHit hitOther;
        Ray ray = new Ray(myTransform.position, futurePosition - myTransform.position);
        Physics.Raycast(ray, out hitOther, 5f);
        var avoidance = PerpendicularComponent(hitOther.normal, moveDirection);
        avoidance.Normalize();
    }

    #region Combat setting
    public void SetFear(float value)
    {
        Fear = value;
        if (Fear > FearMax)
            Fear = FearMax;
        else if (Fear < FearMin)
            Fear = FearMin;
    }

    public void SetRage(float value)
    {
        Rage = value;
        if (Rage > RageMax)
            Rage = RageMax;
        else if (Rage < RageMin)
            Rage = RageMin;
    }

    public void SetBerserk(float value)
    {
        Berserk = value;
        if (Berserk > BerserkMax)
            Berserk = BerserkMax;
        else if (Berserk < BerserkMin)
            Berserk = BerserkMin;
    }

    public void SetDodge(float value)
    {
        Dodge = value;
        if (Dodge > DodgeMax)
            Dodge = DodgeMax;
        else if (Dodge < DodgeMin)
            Dodge = DodgeMin;
    }

    public void UpdateCombatSetting()
    {        
        SetRage(Rage + RageModificator * Time.fixedDeltaTime);
        SetBerserk(Berserk + BerserkModificator * Time.fixedDeltaTime);
    
        //发现敌人之后
        //if (DesiredTarget && Owner.WorldState.GetWSProperty(E_PropKey.E_AHEAD_OF_ENEMY).GetBool())
        //    SetFear(Fear + FearModificator * Time.fixedDeltaTime);
        //else
        //    SetFear(Fear - FearModificator * Time.fixedDeltaTime);

        //if (Owner.WorldState.GetWSProperty(E_PropKey.E_IN_BLOCK).GetBool() != true)
        //    SetDodge(Dodge + Owner.BlackBoard.DodgeModificator * Time.fixedDeltaTime);
    }

    #endregion
}

