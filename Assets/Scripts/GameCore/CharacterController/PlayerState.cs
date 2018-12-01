using UnityEngine;
using System.Collections;

public enum UnitState
{
    None = -1,
    Idle,
    Move,
    Jumping,
    Land,
    Attack,
    Dash,
    Defend,
    Hit,
    Death,
    Throw,
    PickItem,
    Knockdown,
    KnockDownGrounded,
    GroundAttack,
    GroundHit,
    StandUp,
    UseWeapon,
    NoUseWeapon,
    Roll,
};

public class PlayerState : MonoBehaviour
{
    public UnitState currentState = UnitState.Idle;

    public void SetState(UnitState state)
    {
        currentState = state;
    }

    public UnitState GetCurrentState()
    {
        return currentState;
    }

}
