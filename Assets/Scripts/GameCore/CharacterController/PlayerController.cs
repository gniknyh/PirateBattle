using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SporeWeaponTrail;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerState))]
public class PlayerController : CharacterMotor
{

    private List<UnitState> MovementStates = new List<UnitState> {
        UnitState.Idle,
        UnitState.Move,
        UnitState.Jumping,
        UnitState.Land,
    };

    private PlayerCombat playerCombat;

    #region unity method

    void OnEnable()
    {
        InputManager.onCombatInputEvent += InputEventAction;
        InputManager.onInputEvent += InputEvent;
        InputManager.onActionHandler += ActionInputEvent;

        //Messenger.AddListener<int>("death", Death);
    }

    void OnDisable()
    {
        InputManager.onCombatInputEvent -= InputEventAction;
        InputManager.onInputEvent -= InputEvent;
        InputManager.onActionHandler += ActionInputEvent;

        //Messenger.RemoveListener<int>("death", Death);

    }

    void Start()
    {
        InitMotor();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        UpdateMotor();
        ControlCameraState();
    }

    void LateUpdate()
    {
   
    }
    
    #endregion

    private void ControlCameraState()
    {
        if (gameCamera == null)
            return;
        gameCamera.ChangeState("Default", true);
    }

    private void InputEventAction(InputAction action)
    {
        //jump
        if (MovementStates.Contains(playerState.currentState) && !isDead)
        {
            if (action == InputAction.JUMP)
            {
                //if (playerState.currentState != UnitState.JUMPING && onGround())
                //{
                //    StopAllCoroutines();
               
                //}
            }
        }

    }

    private void InputEvent(Vector2 _input)
    {
        input = _input;
        lockOn = false;
        if (MovementStates.Contains(playerState.currentState) && !isDead)
        {
            if (!playerState.currentState.Equals(UnitState.Attack) && onGround)
            {
                if (!lockOn)
                {
                    freeMovement();
                }
                else
                {
                    LockOnMovement();
                }
            }

        }

    }

    private void ActionInputEvent(InputAction action)
    {
        Debug.Log("action:" + action.ToString());
        switch (action)
        {
            case InputAction.JUMP:
                break;
            case InputAction.DEFEND:
                break;
            case InputAction.ROLL:
                Roll();
                break;
            case InputAction.WEAPONATTACK:
                break;
            default:
                break;
        }
    }

    
}
