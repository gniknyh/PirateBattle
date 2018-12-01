using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CharacterMotor : CharacterData
{
    public bool IsAlive = true;
    //物体可见度，100表示物体可以被发现
    public int objectVisibility = 100;

    #region Layers
    [Header("---! Layers !---")]
    public LayerMask groundLayer = 1 << 0;

    //LayerMask mask = 1 << 你需要开启的Layers层。
    public LayerMask autoCrouchLayer = 1 << 0;
    //头部碰撞检测，自动下蹲
    public float headDetect = 0.95f;

    public LayerMask actionLayer;
    public float actionRayHeight = 0.5f;
    public float actionRayDistance = 0.25f;

    public LayerMask stopMoveLayer;
    public float stopMoveHeight = 0.65f;
    public float stopMoveDistance = 0.5f;
    #endregion

    #region Character Variables

    [Header("--- Locomotion Setup ---")]

    [SerializeField]
    protected float walkSpeed = 1.5f;
    [SerializeField]
    protected float runningSpeed = 1.5f;
    [SerializeField]
    protected float sprintSpeed = 1.5f;
    [SerializeField]
    protected float crouchSpeed = 1.5f;
    [SerializeField]
    protected float lockOnSpeed = 1.5f;
    [SerializeField]
    protected float crouchLockOnSpeed = 1.5f;
    [SerializeField]
    protected bool rotateByWorld = false;
    [SerializeField]
    protected bool turnOnSpot = true;
    [SerializeField]
    protected bool quickStopAnim = true;
    [SerializeField]
    protected float freeRotationSpeed = 10f;
    [SerializeField]
    protected float strafeRotationSpeed = 5f;

    [Header("--- Grounded Setup ---")]
    public float groundMinDistance = 0.1f;
    public float groundMaxDistance = 0.1f;

    protected float groundCheckDistance = 0.2f;
    private float origGroundCheckDistance;

    protected float groundDistance;
    public RaycastHit groundHit;
    protected Vector3 groundNormal;
    public float gravityMultiplier = 2f;
    //
    public float stepOffsetEnd = 0.45f;
    //补偿高度，确保在地板上保持轻微的高度。
    public float stepOffsetStart = 0.05f;
    public float stepSmooth = 2f;

    //最大爬坡角度
    [SerializeField]
    protected float slopeLimit = 45f;
    //添加重力
    [SerializeField]
    protected float extraGravity = -0.35f;
    [SerializeField]
    protected float landHighVel = -12f;
    [SerializeField]
    protected float ragdollVel = -16f;

    [Header("--- Head Track & IK---")]
    [SerializeField]
    protected float headWeight = 1f;
    [SerializeField]
    protected float freeBodyWeight = 0.25f;
    [SerializeField]
    protected float strafeBodyWeight = 0.8f;
    [SerializeField]
    protected float maxAngle = 55f;
    [SerializeField]
    protected float headTrackMultiplier = 1f;
    [HideInInspector]
    public float handIKWeight;

    [Header("--- Debug Info ---")]
    [SerializeField]
    protected bool debugWindow;
    [Range(0f, 1f)]
    public float timeScale = 1f;

    #endregion

    #region Camera variable

    [HideInInspector]
    public GameCamera gameCamera;
    [HideInInspector]
    public string customCameraState;
    [HideInInspector]
    public string customlookAtPoint;
    [HideInInspector]
    public bool changeCameraState;
    [HideInInspector]
    public bool smoothCameraState;
    [HideInInspector]
    public Quaternion freeRotation;
    [HideInInspector]
    public float offSetPivot;
    [HideInInspector]
    public bool keepDirection;
    [HideInInspector]
    public Vector2 lastInput;
    [HideInInspector]
    public Vector3 cameraForward;
    [HideInInspector]
    public Vector3 cameraRight;
    public Transform myTransform;

    public Transform cameraTransform
    {
        get
        {
            Transform cameraTransform = null;
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            if (gameCamera)
                cameraTransform = gameCamera.transform;
            if (cameraTransform == transform)
            {
                Debug.LogWarning(" Missing GameCamera or MainCamera");
                this.enabled = false;
            }
            return cameraTransform;
        }
    }

    #endregion

    #region animator info

    [HideInInspector]
    public AnimatorStateInfo baseLayerStateInfo;
    private int baseLayer = 0;

    // head track, 控制头部朝向
    private Vector3 lookPosition;

    [HideInInspector]
    public float lastSpeed = 0.0f;

    public float acceleration
    {
        get
        {
            float speed = animator.GetFloat("Speed");
            float deltaA = (speed - lastSpeed) / Time.fixedDeltaTime;
            lastSpeed = speed;
            return Mathf.Round(deltaA);
        }
    }

    #endregion

    #region Components

    [HideInInspector]
    public Rigidbody rigidbody;
    public PhysicMaterial frictionPhysics, slippyPhysics;
    [HideInInspector]
    public CapsuleCollider capsuleCollider;

    #endregion

    #region Actions
    [HideInInspector]
    public bool onGround;
    protected bool stopMove, canSprint, crouch, strafing, landHigh, sliding, dashing;
    // actions bools, used to turn on/off actions animations        
    protected bool jumpOver, stepUp, climbUp, roll, isRolling, enterLadderBottom, enterLadderTop,
        usingLadder, exitLadderBottom, exitLadderTop, inAttack, blocking, hitReaction, hitRecoil;

    protected bool jump, isJumping, jumpAirControl;
    protected float jumpForce, jumpForward;

    protected bool canMoveForward, canMoveRight, canMoveLeft, canMoveBack;
    protected Vector3 dragEuler;
    protected bool dragStart;
    protected bool lockOn = false;

    // one bool to rule then all
    protected bool actions
    {
        get
        {
            return jumpOver || stepUp || climbUp || roll || usingLadder
                 || hitReaction || hitRecoil;
        }
    }

    #endregion

    [HideInInspector]
    public float colliderRadius, colliderHeight;
    [HideInInspector]
    public Vector3 colliderCenter;
    //输入
    [HideInInspector]
    public Vector2 input;
    [HideInInspector]
    public float speed, direction, verticalVelocity;
    [HideInInspector]
    public bool updateVelocity = false;
    [HideInInspector]
    public Vector3 rbVelocity = Vector3.zero;

    protected PlayerState playerState = null;

    #region init and update motor

    public void InitMotor()
    {
        myTransform = transform;
        animator = GetComponent<Animator>();
        gameCamera = GameCamera.Instance;

        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        offSetPivot = Vector3.Distance(transform.position, hips.position);

        if (gameCamera != null)
        {
            gameCamera.offsetPlayerPivot = offSetPivot;
            gameCamera.target = transform;
        }

        //防止在斜坡上滑行
        frictionPhysics = new PhysicMaterial();
        frictionPhysics.name = "frictionPhysics";
        frictionPhysics.staticFriction = 1f;
        frictionPhysics.dynamicFriction = 1f;
        frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

        // default physics 
        slippyPhysics = new PhysicMaterial();
        slippyPhysics.name = "slippyPhysics";
        slippyPhysics.staticFriction = 0f;
        slippyPhysics.dynamicFriction = 0f;
        slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

        rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        colliderCenter = GetComponent<CapsuleCollider>().center;
        colliderRadius = GetComponent<CapsuleCollider>().radius;
        colliderHeight = GetComponent<CapsuleCollider>().height;

        currentHealth = maxHealth;
        currentHealthRecoveryDelay = healthRecoveryDelay;
        currentStamina = maxStamina;

        //默认是就可以前后左右移动的
        canMoveForward = true;
        canMoveRight = true;
        canMoveLeft = true;
        canMoveBack = true;

        //TODO：摄像机初始化
        cameraTransform.SendMessage("Init", SendMessageOptions.DontRequireReceiver);

        baseLayer = animator.GetLayerIndex("Base Layer");
        if (!animator || !animator.enabled)
        {
            Debug.LogError("检查是否挂载animator组件");
            return;
        }
        baseLayerStateInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
        playerState = GetComponent<PlayerState>();
        origGroundCheckDistance = groundCheckDistance;
    }

    //FixedUpdate
    protected void UpdateMotor()
    {
        CheckGround();
        if (updateVelocity)
        {
            rigidbody.velocity = rbVelocity;
            updateVelocity = false;
        }
        baseLayerStateInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
        //string animString = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        //Debug.Log("animString:" + animString);
        //Debug.Log("state info:" + baseLayerStateInfo.IsName("Attack.AttackA"));
    }

    #endregion

    protected Vector3 targetDirection
    {
        get
        {
            Vector3 refDir = Vector3.zero;
            cameraForward = keepDirection
                ? cameraForward :
                cameraTransform.TransformDirection(Vector3.forward);
            cameraForward.y = 0;

            if (gameCamera == null || !gameCamera.currentState.cameraMode.Equals(CameraMode.FixedAngle) || !rotateByWorld)
            {
                cameraForward = keepDirection
                    ? cameraForward
                    : cameraTransform.TransformDirection(Vector3.forward);
                cameraForward.y = 0;

                //get the right-facing direction of the camera
                cameraRight = keepDirection
                    ? cameraRight
                    : cameraTransform.TransformDirection(Vector3.right);

                refDir = input.x * cameraRight + input.y * cameraForward;
            }
            else
            {
                refDir = new Vector3(input.x, 0, input.y);
            }
            return refDir;
        }
    }

    protected void LockOnMovement()
    {
        var _direction = Mathf.Clamp(input.x, canMoveLeft ? -1f : 0, canMoveRight ? 1f : 0);
        var _speed = Mathf.Clamp(input.y, canMoveBack ? -1f : 0, canMoveForward ? 1f : 0);

        speed = _speed;
        direction = _direction;
        //Debug.Log("speed:" + speed + "   direction:" + direction);
        if (canSprint)
        {
            speed += canMoveForward ? 0.5f : 0;
        }

        if (direction >= 0.7 || direction <= -0.7 || speed <= 0.1)
        {
            //有可能不需要加这个
            canSprint = false;
        }

        animator.SetBool("LockOn", true);
        animator.SetFloat("Direction", stopMove ? 0f : direction, 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("Speed", stopMove ? 0 : speed, 0.2f, Time.fixedDeltaTime);
        AddExtraMoveSpeed();

    }

    public void freeMovement()
    {
        speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        speed = Mathf.Clamp(speed, 0, canMoveForward ? 1 : 0);
        //冲刺时速度加0.5f(animator上),1.5f
        if (canSprint)
            speed += (canMoveForward ? 0.5f : 0);
        if (stopMove)
        {
            speed = 0.0f;
        }

        if (input == Vector2.zero)
        {
            direction = Mathf.Lerp(direction, 0f, 20f * Time.fixedDeltaTime);
        }

        if ((input != Vector2.zero) && targetDirection.magnitude > 0.1f)
        {
            freeRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            Vector3 lookDir = targetDirection.normalized;
            //print("lookDir:" + lookDir);
            freeRotation = Quaternion.LookRotation(lookDir);
            //旋转角度,因为只有左右旋转，所以只需要处理基于y轴的旋转
            float diferenceRota = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            float eulerY = transform.eulerAngles.y;

            //判定是否有旋转了
            if (diferenceRota < 0.0f && canMoveLeft || diferenceRota > 0.0f && canMoveRight)
            {
                eulerY = freeRotation.eulerAngles.y;
            }

            Vector3 euler = new Vector3(0, eulerY, 0);
            // 旋转用slerp更好相对于使用lerp
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), freeRotationSpeed * Time.fixedDeltaTime);

            if (!keepDirection)
            {
                lastInput = input;
            }

            if (Vector2.Distance(lastInput, input) > 0.9f && keepDirection)
            {
                keepDirection = false;
            }

        }

        animator.SetFloat("MoveSet_ID", 1);
        animator.SetFloat("Direction", stopMove ? 0f : direction, 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("Speed", !stopMove || stopMove ? speed : 0f, 0.2f, Time.fixedDeltaTime);
        AddExtraMoveSpeed();
    }

    private void SetColliderHeight()
    {
        if (isJumping || crouch || roll)
        {
            capsuleCollider.center = colliderCenter / 1.4f;
            capsuleCollider.height = colliderHeight / 1.4f;

        }
        else if (usingLadder)
        {
            capsuleCollider.radius = colliderRadius / 1.25f;
        }
        else
        {
            capsuleCollider.center = colliderCenter;
            capsuleCollider.height = colliderHeight;
            capsuleCollider.radius = colliderRadius;
        }
    }

    public void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            onGround = true;
            //animator.applyRootMotion = true;
        }
        else
        {
            onGround = false;
            groundNormal = Vector3.up;
            //animator.applyRootMotion = false;
        }
        StepOffset();
    }
     
    public void CheckGround()
    {
        CheckGroundDistance();

        // change the physics material to very slip when not grounded
        capsuleCollider.material = (onGround && GroundAngle() < slopeLimit)
            ? frictionPhysics : slippyPhysics;

        // we don't want to stick the character grounded if one of these bools is true
        bool groundStickConditions = !jumpOver && !stepUp && !climbUp
            && !usingLadder && !hitReaction;

        var magVel = (float)System.Math.Round(new Vector3(rigidbody.velocity.x,
            0, rigidbody.velocity.z).magnitude, 2);
        magVel = Mathf.Clamp(magVel, 0, 1);

        var groundCheckDistance = groundMinDistance;
        if (magVel > 0.25f) groundCheckDistance = groundMaxDistance;

        if (groundStickConditions)
        {
            if (playerState.currentState == UnitState.Attack) return;
            var onStep = StepOffset();

            if (groundDistance <= 0.05f)
            {
                onGround = true;
                //Sliding();
            }
            else
            {
                if (groundDistance >= groundCheckDistance)
                {
                    onGround = false;
                    // check vertical velocity
                    verticalVelocity = rigidbody.velocity.y;
                    // apply extra gravity when falling
                    if (!onStep && !roll && !jump)
                        rigidbody.AddForce(transform.up * extraGravity, ForceMode.VelocityChange);
                }
                else if (!onStep && !roll && !jump)
                    rigidbody.AddForce(transform.up * (extraGravity * 2), ForceMode.VelocityChange);
            }
        }
    }

    void CheckGroundDistance()
    {
        if (capsuleCollider != null)
        {
            float radius = capsuleCollider.radius * 0.9f;
            var dist = 10f;

            Vector3 pos = transform.position + Vector3.up * (capsuleCollider.radius);

            Ray ray1 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0),
                       Vector3.down);
            Ray ray2 = new Ray(pos, -Vector3.up);

            if (Physics.Raycast(ray1, out groundHit, 1f, groundLayer))
            {
                dist = transform.position.y - groundHit.point.y;
            }
            if (Physics.SphereCast(ray2, radius, out groundHit, 1f, groundLayer))
            {
                if (dist > (groundHit.distance - capsuleCollider.radius * 0.1f))
                    dist = (groundHit.distance - capsuleCollider.radius * 0.1f);
            }
            groundDistance = dist;
        }
    }

    public void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        rigidbody.AddForce(extraGravityForce);

        groundCheckDistance = rigidbody.velocity.y < 0 ? origGroundCheckDistance : 0.01f;
    }

    /// <summary>
    /// 返回地面坡度
    /// </summary>
    /// <returns></returns>
    private float GroundAngle()
    {
        float angle = Vector3.Angle(groundHit.normal, Vector3.up);
        return angle;
    }

    //idle
    public void Ready()
    {
        StopAllCoroutines();
        CancelInvoke();
        speed = 0;
        SetVelocity(Vector3.zero);
        freeMovement();
    }

    protected void StopMove()
    {
        if (input.sqrMagnitude < 0.1 || !onGround) return;

        RaycastHit hitinfo;
        Ray ray = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), transform.forward);

        if (Physics.Raycast(ray, out hitinfo, capsuleCollider.radius + stopMoveDistance, stopMoveLayer) && !usingLadder)
        {
            var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

            if (hitinfo.distance <= stopMoveDistance && hitAngle > 85)
                stopMove = true;
            else if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                stopMove = true;
        }
        else if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer)
            && !usingLadder)
        {
            var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);
            if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
            {
                // Debug.Log("stopMove");
                stopMove = true;
            }

        }
        else
            stopMove = false;
    }

    private bool StepOffset()
    {
        if (input.sqrMagnitude < 0.1 || !onGround) return false;

        var hit = new RaycastHit();
        Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + transform.forward * ((capsuleCollider).radius + 0.05f)), Vector3.down);

        if (Physics.Raycast(rayStep, out hit, stepOffsetEnd - stepOffsetStart, groundLayer))
        {
            if (!stopMove && hit.point.y >= (transform.position.y)
                && hit.point.y <= (transform.position.y + stepOffsetEnd))
            {
                var heightPoint = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, heightPoint, (speed * stepSmooth) * Time.fixedDeltaTime);
                return true;
            }
        }
        return false;
    }

    protected void OnAnimatorIK(int layerIndex)
    {

    }

    //添加额外的速度,如果自带root motion，这里就不必要了
    private void AddExtraMoveSpeed()
    {
        if (stopMove) return;
        if (playerState.currentState != UnitState.Attack)
        {
            if (baseLayerStateInfo.IsName("Grounded.LockOnMovement"))//访问子状态机
            {
                Vector3 dir = new Vector3(direction, 0, speed);
                if (dir.magnitude > 1f) dir.Normalize();
                dir = transform.InverseTransformDirection(dir);
                SetVelocity(dir * lockOnSpeed);

            }
            else if (baseLayerStateInfo.IsName("Grounded.LockOnCrouch"))
            {
                var newSpeed_Y = (crouchLockOnSpeed * speed);
                var newSpeed_X = (crouchLockOnSpeed * direction);
                newSpeed_Y = Mathf.Clamp(newSpeed_Y, -crouchLockOnSpeed, crouchLockOnSpeed);
                newSpeed_X = Mathf.Clamp(newSpeed_X, -crouchLockOnSpeed, crouchLockOnSpeed);
                rigidbody.AddForce(transform.forward * (newSpeed_Y), ForceMode.VelocityChange);
                rigidbody.AddForce(transform.right * (newSpeed_X), ForceMode.VelocityChange);
            }
            else if (baseLayerStateInfo.IsName("Grounded.FreeMovement"))
            {
                if (speed <= 0.5f)
                {
                    SetVelocity(transform.forward * (walkSpeed * speed));
                }
                else if (speed > 0.5f && speed <= 1.0f)
                {   
                    SetVelocity(transform.forward * (runningSpeed * speed));
                }
                else
                {
                    SetVelocity(transform.forward * (sprintSpeed * speed));
                }
            }
            else if (baseLayerStateInfo.IsName("Grounded.FreeCrouch"))
            {
                SetVelocity(transform.forward * (crouchSpeed * speed));
            }
        }
        else
        {
            speed = 0.0f;
        }

    }

    public void SetVelocity(Vector3 velocity)
    {
        rbVelocity = velocity;
        updateVelocity = true;
    }

    protected void Roll()
    {
        bool canRoll = input != Vector2.zero && onGround;
        playerState.SetState(UnitState.Roll);
        lockOn = false;
        canRoll = true;

        if (lockOn)
        {
    
                animator.SetBool("Roll", true);
                animator.SetFloat("Direction",direction, 0.1f, Time.fixedDeltaTime);
                animator.SetFloat("Speed", speed, 0.2f, Time.fixedDeltaTime);

            if (baseLayerStateInfo.IsName("Action.Dodge"))
            {
                rigidbody.useGravity = false;
                    if (verticalVelocity >= 1)
                        rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundHit.normal);

                    if (baseLayerStateInfo.normalizedTime > 0.3f)
                        rigidbody.useGravity = true;

                    if (baseLayerStateInfo.normalizedTime >= 0.85f)
                    {
                        playerState.SetState(UnitState.Idle);
                        animator.SetBool("Roll", false);
                        SetVelocity(Vector3.zero);
                    }
            }

        }
        else
        {
            if (canRoll)
            {   
                animator.SetBool("Roll2", true);
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, freeRotationSpeed * Time.fixedDeltaTime, 0.0f);
                freeRotation = Quaternion.LookRotation(newDir);
                var eulerAngles = new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z);
                transform.eulerAngles = eulerAngles;
                //string animString = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                //Debug.Log("animString:" + animString);
                Timer.InvokeMe(0.5f, () =>
                {
                    playerState.SetState(UnitState.Idle);
                    animator.SetBool("Roll2", false);
                    SetVelocity(Vector3.zero);
                });
                if (baseLayerStateInfo.IsName("Roll2"))
                {
                    Debug.Log("22222222222");
                    rigidbody.useGravity = false;
                    // prevent the character to rolling up 
                    if (verticalVelocity >= 1)
                        rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundHit.normal);

                    // reset the rigidbody a little ealier to the character fall while on air
                    if (baseLayerStateInfo.normalizedTime > 0.3f)
                        rigidbody.useGravity = true;

                    if ( baseLayerStateInfo.normalizedTime >= 0.85f)
                    {
                        playerState.SetState(UnitState.Idle);
                        animator.SetBool("Roll2", false);
                        SetVelocity(Vector3.zero);
                    }
                }
            }
        }
    }
    
}


