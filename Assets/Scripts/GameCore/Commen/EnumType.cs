using UnityEngine;
using System.Collections;

/**
 *     枚举类型的都可以往这边写，也有些枚举是写在别处的，但是建议往这里写 
 */

public enum AttackType
{
    None = -1,
    A = 0,
    B = 1,
    Fatality = 2,
}

public enum SwordLevel
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Max = 5
}
