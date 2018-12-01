using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
//using UnityEditor;

public enum InputAction
{
    NONE,
    ATK_A,
    ATK_B,
    JUMP,
    DEFEND,
    ROLL,
    WEAPONATTACK,
}

public enum InputType
{
    KEYBOARD = 0,
    JOYPAD = 5,
    TOUCHSCREEN = 10,
}


public class InputManager: MonoBehaviour
{

    public InputType inputType = InputType.KEYBOARD;

    /// <summary>
    /// GAMEPAD VIBRATION - 调用这个方法使用振动手柄
    /// </summary>
    /// <param name="vibTime">duration of the vibration</param>
    /// <returns></returns>
    //public IEnumerator GamepadVibration(float vibTime)
    //{
    //    if (inputType == InputType.Controler)
    //    {
    //        XInputDotNetPure.GamePad.SetVibration(0, 1, 1);//手柄插件设置震动
    //        yield return new WaitForSeconds(vibTime);
    //        XInputDotNetPure.GamePad.SetVibration(0, 0, 0);
    //    }
    //}

    //键盘控制(PC)
    [Header("----------- Keyboard keys ------------ ")]
    public KeyCode ATK_A = KeyCode.J;
    public KeyCode ATK_B = KeyCode.X;
    public KeyCode DefendKey = KeyCode.C;
    public KeyCode JumpKey = KeyCode.Space;
    public KeyCode DodgeKey = KeyCode.Q;

    //手柄控制
    [Header("------------- Joypad keys ----------------")]
    public KeyCode JoypadPunch = KeyCode.JoystickButton2;
    public KeyCode JoypadKick = KeyCode.JoystickButton3;
    public KeyCode JoypadDefend = KeyCode.JoystickButton1;
    public KeyCode JoypadJump = KeyCode.JoystickButton0;

    public delegate void InputEventHandler(Vector2 dir);
    public static event InputEventHandler onInputEvent;
    public delegate void CombatInputEventHandler(InputAction action);
    public static event CombatInputEventHandler onCombatInputEvent;

    public delegate void ActionInputEventHandler(InputAction action);
    public static event ActionInputEventHandler onActionHandler;

    [HideInInspector]
    public Vector2 dir;
    private bool TouchScreenActive;
    public static bool defendKeyDown;
    private GameCamera gameCamera;

    void Start()
    {
        gameCamera = GameCamera.Instance;
#if (UNITY_IOS || UNITY_ANDROID)
	    inputType = InputType.TOUCHSCREEN;
#elif (UNITY_STANDALONE_WIN)
        inputType = InputType.KEYBOARD;
#else 
        InputType = InputType.JOYPAD;
#endif
    }

    public static void InputEvent(Vector2 dir)
    {
        if (onInputEvent != null)
            onInputEvent(dir);
    }

    public static void CombatInputEvent(InputAction action)
    {
        if (onCombatInputEvent != null)
            onCombatInputEvent(action);
    }

    public static void ActionInputEvent(InputAction action)
    {
        if (onActionHandler != null)
            onActionHandler(action);
    }

    public static void OnDefendButtonPress(bool state)
    {
        defendKeyDown = state;
    }

    void Update()
    {
        CameraInput();
        switch (inputType)
        {
            case InputType.KEYBOARD:
                KeyboardControls();
                break;
            case InputType.JOYPAD:
                JoyPadControls();
                break;
            case InputType.TOUCHSCREEN:
                //Android  | IOS
                //EnableDisableTouchScrn(UseTouchScreenInput);
                break;
            default:
                break;
        }

    }

    private void CameraInput()
    {
        if (inputType == InputType.TOUCHSCREEN)
        {
            gameCamera.RotateCamera(CrossPlatformInputManager.GetAxis("Mouse X"), CrossPlatformInputManager.GetAxis("Mouse Y"));
        }
        else if (inputType == InputType.KEYBOARD)
        {
            gameCamera.RotateCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            gameCamera.Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }
        else if (inputType == InputType.JOYPAD)
        {
            gameCamera.RotateCamera(Input.GetAxis("RightAnalogHorizontal"), Input.GetAxis("RightAnalogVertical"));
        }
    }

    public Vector2 MovementInput()
    {
        Vector2 _input = Vector2.zero;
        if (inputType == InputType.TOUCHSCREEN)
        {
            _input = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));
        }
        else if (inputType == InputType.KEYBOARD)
        {
            _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        else if (inputType == InputType.JOYPAD)
        {
            float deadzone = 0.25f;
            _input = new Vector2(Input.GetAxis("LeftAnalogHorizontal"), Input.GetAxis("LeftAnalogVertical"));
            if (_input.magnitude < deadzone)
                _input = Vector2.zero;
            else
                _input = _input.normalized * ((_input.magnitude - deadzone) / (1 - deadzone));
        }

        return _input;
    }

    private void KeyboardControls()
    {
        Vector2 input = MovementInput();
        InputEvent(input);

        if (Input.GetKeyDown(ATK_A) || Input.GetMouseButtonDown(0))
        {
            CombatInputEvent(InputAction.ATK_A);
        }

        if (Input.GetKeyDown(ATK_B) || Input.GetMouseButtonDown(1))
        {
            CombatInputEvent(InputAction.ATK_B);
        }

        if (Input.GetKeyDown(JumpKey))
        {
            ActionInputEvent(InputAction.JUMP);
        }

        if (Input.GetKeyDown(DodgeKey))
        {
            ActionInputEvent(InputAction.ROLL);
        }

        //格挡是有时间限制的todo
        defendKeyDown = Input.GetKey(DefendKey);

    }

    private void JoyPadControls()
    {
        float x = Input.GetAxis("Joypad Left-Right");
        float y = Input.GetAxis("Joypad Up-Down");

        dir = new Vector2(x, y);
        InputEvent(dir.normalized);

        if (Input.GetKeyDown(JoypadPunch))
        {
            CombatInputEvent(InputAction.ATK_A);
        }

        if (Input.GetKeyDown(JoypadKick))
        {
            CombatInputEvent(InputAction.ATK_B);
        }

        if (Input.GetKey(JoypadJump))
        {
            CombatInputEvent(InputAction.JUMP);
        }

        defendKeyDown = Input.GetKey(JoypadDefend);
    }

    //public void EnableDisableTouchScrn(bool state)
    //{
    //    InputEvent(dir.normalized);

    //    if (_UIManager != null)
    //    {
    //        if (state)
    //        {
    //            if (!TouchScreenActive)
    //            {
    //                _UIManager.ShowMenu("TouchScreenControls", false);
    //                TouchScreenActive = true;
    //            }

    //        }
    //        else
    //        {

    //            if (TouchScreenActive)
    //            {
    //                TouchScreenActive = false;
    //                _UIManager.CloseMenu("TouchScreenControls");
    //            }
    //        }
    //    }
    //}

    public bool isDefendKeyDown()
    {
        return defendKeyDown;
    }


}